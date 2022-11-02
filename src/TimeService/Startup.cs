using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DigitalOffice.Kernel.RedisSupport.Extensions;
using HealthChecks.UI.Client;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Kernel.BrokerSupport.Extensions;
using LT.DigitalOffice.Kernel.BrokerSupport.Middlewares.Token;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Kernel.EFSupport.Extensions;
using LT.DigitalOffice.Kernel.EFSupport.Helpers;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Middlewares.ApiInformation;
using LT.DigitalOffice.TimeService.Broker.Consumers;
using LT.DigitalOffice.TimeService.Business.Helpers.Emails;
using LT.DigitalOffice.TimeService.Business.Helpers.LeaveTimes;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Configurations;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LT.DigitalOffice.TimeService
{
  public class Startup : BaseApiInfo
  {
    public const string CorsPolicyName = "LtDoCorsPolicy";

    private readonly RabbitMqConfig _rabbitMqConfig;
    private readonly TimeConfig _timeConfig;
    private readonly BaseServiceInfoConfig _serviceInfoConfig;

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;

      _rabbitMqConfig = Configuration
        .GetSection(BaseRabbitMqConfig.SectionName)
        .Get<RabbitMqConfig>();

      _timeConfig = Configuration
        .GetSection(TimeConfig.SectionName)
        .Get<TimeConfig>();

      _serviceInfoConfig = Configuration
        .GetSection(BaseServiceInfoConfig.SectionName)
        .Get<BaseServiceInfoConfig>();

      Version = "1.1.8.2";
      Description = "TimeService is an API intended to work with the users time managment";
      StartTime = DateTime.UtcNow;
      ApiName = $"LT Digital Office - {_serviceInfoConfig.Name}";
    }

    #region public methods

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy(
          CorsPolicyName,
          builder =>
          {
            builder
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
          });
      });

      services.Configure<TokenConfiguration>(Configuration.GetSection("CheckTokenMiddleware"));
      services.Configure<BaseRabbitMqConfig>(Configuration.GetSection(BaseRabbitMqConfig.SectionName));
      services.Configure<TimeConfig>(Configuration.GetSection(TimeConfig.SectionName));
      services.Configure<BaseServiceInfoConfig>(Configuration.GetSection(BaseServiceInfoConfig.SectionName));

      services.AddHttpContextAccessor();
      services
        .AddControllers()
        .AddJsonOptions(options =>
        {
          options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        })
        .AddNewtonsoftJson(options =>
        {
          options.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.None;
        });

      string connStr = ConnectionStringHandler.Get(Configuration);

      string timeToTryAgaing = Environment.GetEnvironmentVariable("TimeToRestartCreatingRecords");
      if (string.IsNullOrEmpty(timeToTryAgaing) && int.TryParse(timeToTryAgaing, out int minutes))
      {
        _timeConfig.MinutesToRestart = minutes;
      }

      services.AddDbContext<TimeServiceDbContext>(options =>
      {
        options.UseSqlServer(connStr);
      });

      services.AddRedisSingleton(Configuration);

      services.AddBusinessObjects();

      ConfigureMassTransit(services);

      services.AddMemoryCache();
      services.AddTransient<WorkTimeCreator>();
      services.AddTransient<WorkTimeLimitCreator>();
      services.AddTransient<ProlongedLeaveTimeHelper>();
      services.AddTransient<EmailSender>();

      services
        .AddHealthChecks()
        .AddSqlServer(connStr)
        .AddRabbitMqCheck();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
      app.UpdateDatabase<TimeServiceDbContext>();

      app.UseForwardedHeaders();

      CreateTimeAsync(app).Wait();

      app.UseExceptionsHandler(loggerFactory);

      app.UseApiInformation();

      app.UseRouting();

      app.UseMiddleware<TokenMiddleware>();

      app.UseCors(CorsPolicyName);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers().RequireCors(CorsPolicyName);

        endpoints.MapHealthChecks($"/{_serviceInfoConfig.Id}/hc", new HealthCheckOptions
        {
          ResultStatusCodes = new Dictionary<HealthStatus, int>
          {
            { HealthStatus.Unhealthy, 200 },
            { HealthStatus.Healthy, 200 },
            { HealthStatus.Degraded, 200 },
          },
          Predicate = check => check.Name != "masstransit-bus",
          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
      });
    }

    #endregion

    #region private methods

    private (string username, string password) GetRabbitMqCredentials()
    {
      static string GetString(string envVar, string formAppsettings, string generated, string fieldName)
      {
        string str = Environment.GetEnvironmentVariable(envVar);
        if (string.IsNullOrEmpty(str))
        {
          str = formAppsettings ?? generated;

          Log.Information(
            formAppsettings == null
              ? $"Default RabbitMq {fieldName} was used."
              : $"RabbitMq {fieldName} from appsetings.json was used.");
        }
        else
        {
          Log.Information($"RabbitMq {fieldName} from environment was used.");
        }

        return str;
      }

      return (GetString("RabbitMqUsername", _rabbitMqConfig.Username, $"{_serviceInfoConfig.Name}_{_serviceInfoConfig.Id}", "Username"),
        GetString("RabbitMqPassword", _rabbitMqConfig.Password, _serviceInfoConfig.Id, "Password"));
    }

    private void ConfigureMassTransit(IServiceCollection services)
    {
      (string username, string password) = GetRabbitMqCredentials();

      services.AddMassTransit(x =>
      {
        x.AddConsumer<CreateWorkTimeConsumer>();

        x.UsingRabbitMq((context, cfg) =>
          {
            cfg.Host(_rabbitMqConfig.Host, "/", host =>
            {
              host.Username(username);
              host.Password(password);
            });

            cfg.ReceiveEndpoint(_rabbitMqConfig.CreateWorkTimeEndpoint, ep =>
            {
              ep.ConfigureConsumer<CreateWorkTimeConsumer>(context);
            });
          });

        x.AddRequestClients(_rabbitMqConfig);
      });

      services.AddMassTransitHostedService();
    }

    private async Task CreateTimeAsync(IApplicationBuilder app)
    {
      var scope = app.ApplicationServices.CreateScope();

      var workTimeCreator = scope.ServiceProvider.GetRequiredService<WorkTimeCreator>();
      var workTimeLimitCreator = scope.ServiceProvider.GetRequiredService<WorkTimeLimitCreator>();
      var prolongedLeaveTimeHelper = scope.ServiceProvider.GetRequiredService<ProlongedLeaveTimeHelper>();
      var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
      var workTimeRepository = scope.ServiceProvider.GetRequiredService<IWorkTimeRepository>();

      DbWorkTime lastWorkTime = await workTimeRepository.GetLastAsync();

      emailSender.Start();

      workTimeCreator.Start(
        _timeConfig.MinutesToRestart,
        lastWorkTime != null
          ? new DateTime(year: lastWorkTime.Year, month: lastWorkTime.Month, day: 1)
          : default);

      workTimeLimitCreator.Start(
        _timeConfig.MinutesToRestart,
        _timeConfig.CountNeededNextMonth);

      prolongedLeaveTimeHelper.Start(
        _timeConfig.MinutesToRestart,
        default);
    }

    #endregion
  }
}
