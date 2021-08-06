using HealthChecks.UI.Client;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Middlewares.ApiInformation;
using LT.DigitalOffice.Kernel.Middlewares.Token;
using LT.DigitalOffice.TimeService.Broker.Consumers;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Dto.Configurations;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeService
{
    public class Startup : BaseApiInfo
    {
        public const string CorsPolicyName = "LtDoCorsPolicy";

        private readonly RabbitMqConfig _rabbitMqConfig;
        private readonly BaseServiceInfoConfig _serviceInfoConfig;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _rabbitMqConfig = Configuration
                .GetSection(BaseRabbitMqConfig.SectionName)
                .Get<RabbitMqConfig>();

            _serviceInfoConfig = Configuration
                .GetSection(BaseServiceInfoConfig.SectionName)
                .Get<BaseServiceInfoConfig>();

            Version = "1.1.7";
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
                            .WithOrigins(
                                "https://*.ltdo.xyz",
                                "http://*.ltdo.xyz",
                                "http://ltdo.xyz",
                                "http://ltdo.xyz:9802",
                                "http://localhost:4200",
                                "http://localhost:4500")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.Configure<TokenConfiguration>(Configuration.GetSection("CheckTokenMiddleware"));
            services.Configure<BaseRabbitMqConfig>(Configuration.GetSection(BaseRabbitMqConfig.SectionName));
            services.Configure<BaseServiceInfoConfig>(Configuration.GetSection(BaseServiceInfoConfig.SectionName));

            services.AddHttpContextAccessor();
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddNewtonsoftJson();

            string connStr = Environment.GetEnvironmentVariable("ConnectionString");

            if (string.IsNullOrEmpty(connStr))
            {
                connStr = Configuration.GetConnectionString("SQLConnectionString");
            }

            services.AddDbContext<TimeServiceDbContext>(options =>
            {
                options.UseSqlServer(connStr);
            });

            services.AddBusinessObjects();

            ConfigureMassTransit(services);

            services.AddTransient<WorkTimeCreater>();
            services.AddTransient<WorkTimeLimitCreater>();

            services
                .AddHealthChecks()
                .AddSqlServer(connStr)
                .AddRabbitMqCheck();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            UpdateDatabase(app);

            app.UseForwardedHeaders();

            CreateTime(app);

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

        private void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<TimeServiceDbContext>();

            context.Database.Migrate();
        }

        private void ConfigureMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<CreateWorkTimeConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitMqConfig.Host, "/", host =>
                    {
                        host.Username($"{_serviceInfoConfig.Name}_{_serviceInfoConfig.Id}");
                        host.Password(_serviceInfoConfig.Id);
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

        private void CreateTime(IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();

            var workTimeCreater = scope.ServiceProvider.GetRequiredService<WorkTimeCreater>();
            var workTimeLimitCreater = scope.ServiceProvider.GetRequiredService<WorkTimeLimitCreater>();
            var workTimeRepository = scope.ServiceProvider.GetRequiredService<IWorkTimeRepository>();
            var limitRepository = scope.ServiceProvider.GetRequiredService<IWorkTimeMonthLimitRepository>();

            var lastWorkTime = workTimeRepository.GetLast();
            var lastLimit = limitRepository.GetLast();

            workTimeCreater.Start(1, 10, lastWorkTime != null ? new DateTime(year: lastWorkTime.Year, month: lastWorkTime.Month, day: 1) : default);
            workTimeLimitCreater.Start(1, 10, lastLimit != null ? new DateTime(year: lastLimit.Year, month: lastLimit.Month, day: 1) : default);
        }

        #endregion
    }
}
