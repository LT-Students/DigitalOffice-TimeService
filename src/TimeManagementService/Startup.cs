using FluentValidation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Middlewares.Token;
using LT.DigitalOffice.TimeManagementService.Business;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Configuration;
using LT.DigitalOffice.TimeManagementService.Data;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using LT.DigitalOffice.TimeManagementService.Validation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LT.DigitalOffice.TimeManagementService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.AddDbContext<TimeManagementDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SQLConnectionString"));
            });

            services.AddControllers();

            services.Configure<TokenConfiguration>(Configuration);

            ConfigureCommands(services);
            ConfigureValidators(services);
            ConfigureMappers(services);
            ConfigureRepositories(services);
            ConfigureRabbitMq(services);
        }

        private void ConfigureCommands(IServiceCollection services)
        {
            services.AddTransient<IGetUserWorkTimesCommand, GetUserWorkTimesCommand>();
            services.AddTransient<ICreateWorkTimeCommand, CreateWorkTimeCommand>();
            services.AddTransient<IEditWorkTimeCommand, EditWorkTimeCommand>();

            services.AddTransient<IGetUserLeaveTimesCommand, GetUserLeaveTimesCommand>();
            services.AddTransient<ICreateLeaveTimeCommand, CreateLeaveTimeCommand>();
            services.AddTransient<IEditLeaveTimeCommand, EditLeaveTimeCommand>();

        }

        private void ConfigureValidators(IServiceCollection services)
        {
            services.AddTransient<IValidator<WorkTimeRequest>, WorkTimeRequestValidator>();
            services.AddTransient<IValidator<EditWorkTimeRequest>, EditWorkTimeRequestValidator>();

            services.AddTransient<IValidator<LeaveTimeRequest>, LeaveTimeRequestValidator>();
            services.AddTransient<IValidator<EditLeaveTimeRequest>, EditLeaveTimeRequestValidator>();
        }

        private void ConfigureMappers(IServiceCollection services)
        {
            services.AddTransient<IMapper<LeaveTimeRequest, DbLeaveTime>, LeaveTimeMapper>();
            services.AddTransient<IMapper<DbLeaveTime, LeaveTimeResponse>, LeaveTimeMapper>();

            services.AddTransient<IMapper<WorkTimeRequest, DbWorkTime>, WorkTimeMapper>();
            services.AddTransient<IMapper<DbWorkTime, WorkTimeResponse>, WorkTimeMapper>();
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            services.AddTransient<IDataProvider, TimeManagementDbContext>();

            services.AddTransient<ILeaveTimeRepository, LeaveTimeRepository>();
            services.AddTransient<IWorkTimeRepository, WorkTimeRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthChecks("/api/healthcheck");

            app.UseExceptionHandler(tempApp => tempApp.Run(CustomExceptionHandler.HandleCustomException));

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
            });
        }

        private void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<TimeManagementDbContext>();

            context.Database.Migrate();
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var rabbitMqConfig = Configuration.GetSection(BaseRabbitMqOptions.RabbitMqSectionName).Get<RabbitMqConfig>();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", host =>
                    {
                        host.Username($"{rabbitMqConfig.Username}_{rabbitMqConfig.Password}");
                        host.Password(rabbitMqConfig.Password);
                    });
                });

                x.AddRequestClient<IGetUserRequest>(new Uri(rabbitMqConfig.UserServiceUrl));
                x.AddRequestClient<IGetProjectUserRequest>(new Uri(rabbitMqConfig.ProjectService_ProjectUserUrl));
                x.AddRequestClient<ICheckTokenRequest>(new Uri(rabbitMqConfig.AuthenticationServiceValidationUrl));
            });

            services.AddMassTransitHostedService();
        }
    }
}
