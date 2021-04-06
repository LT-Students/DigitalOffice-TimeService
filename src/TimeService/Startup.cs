using FluentValidation;
using HealthChecks.UI.Client;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Middlewares.Token;
using LT.DigitalOffice.TimeService.Business;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Configuration;
using LT.DigitalOffice.TimeService.Data;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Mappers;
using LT.DigitalOffice.TimeService.Mappers.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace LT.DigitalOffice.TimeService
{
    public class Startup
    {
        private readonly RabbitMqConfig _rabbitMqConfig;

        public IConfiguration Configuration { get; }

        #region public methods

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _rabbitMqConfig = Configuration
                .GetSection(BaseRabbitMqOptions.RabbitMqSectionName)
                .Get<RabbitMqConfig>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BaseRabbitMqOptions>(Configuration.GetSection(BaseRabbitMqOptions.RabbitMqSectionName));

            services.AddHttpContextAccessor();

            services.AddKernelExtensions();

            string connStr = Environment.GetEnvironmentVariable("ConnectionString");

            // TODO: Fix HealthChecks work with RabbitMQ
            services
                .AddHealthChecks()
                .AddSqlServer(connStr);
                
            if (string.IsNullOrEmpty(connStr))
            {
                connStr = Configuration.GetConnectionString("SQLConnectionString");
            }

            services.AddDbContext<TimeServiceDbContext>(options =>
            {
                options.UseSqlServer(connStr);
            });

            services.AddControllers();

            services.Configure<TokenConfiguration>(Configuration);

            ConfigureCommands(services);
            ConfigureValidators(services);
            ConfigureMappers(services);
            ConfigureRepositories(services);
            ConfigureRabbitMq(services);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.AddExceptionsHandler(loggerFactory);

            UpdateDatabase(app);

#if RELEASE
            app.UseHttpsRedirection();
#endif

            app.UseRouting();

            string corsUrl = Configuration.GetSection("Settings")["CorsUrl"];

            app.UseCors(builder =>
                builder
                    .WithOrigins(corsUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseMiddleware<TokenMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks($"/{_rabbitMqConfig.Password}/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }

        #endregion

        #region private methods

        private void ConfigureCommands(IServiceCollection services)
        {
            services.AddTransient<IEditWorkTimeCommand, EditWorkTimeCommand>();
            services.AddTransient<ICreateLeaveTimeCommand, CreateLeaveTimeCommand>();
            services.AddTransient<ICreateWorkTimeCommand, CreateWorkTimeCommand>();
        }

        private void ConfigureValidators(IServiceCollection services)
        {
            services.AddTransient<IValidator<CreateLeaveTimeRequest>, CreateLeaveTimeRequestValidator>();
            services.AddTransient<IValidator<CreateWorkTimeRequest>, CreateWorkTimeRequestValidator>();
            services.AddTransient<IValidator<EditWorkTimeRequest>, EditWorkTimeRequestValidator>();
        }

        private void ConfigureMappers(IServiceCollection services)
        {
            services.AddTransient<IMapper<CreateLeaveTimeRequest, DbLeaveTime>, LeaveTimeMapper>();
            services.AddTransient<IMapper<CreateWorkTimeRequest, DbWorkTime>, WorkTimeMapper>();
            services.AddTransient<IMapper<EditWorkTimeRequest, DbWorkTime>, WorkTimeMapper>();
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            services.AddTransient<IDataProvider, TimeServiceDbContext>();

            services.AddTransient<ILeaveTimeRepository, LeaveTimeRepository>();
            services.AddTransient<IWorkTimeRepository, WorkTimeRepository>();
        }

        private void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<TimeServiceDbContext>();

            context.Database.Migrate();
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitMqConfig.Host, "/", host =>
                    {
                        host.Username($"{_rabbitMqConfig.Username}_{_rabbitMqConfig.Password}");
                        host.Password(_rabbitMqConfig.Password);
                    });
                });

                x.AddRequestClient<ICheckTokenRequest>(
                    new Uri($"{_rabbitMqConfig.BaseUrl}/{_rabbitMqConfig.ValidateTokenEndpoint}"));

                x.ConfigureKernelMassTransit(_rabbitMqConfig);
            });

            services.AddMassTransitHostedService();
        }

        #endregion
    }
}
