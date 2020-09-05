using System;
using FluentValidation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Middlewares.Token;
using LT.DigitalOffice.TimeManagementService.Commands;
using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Database;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using LT.DigitalOffice.TimeManagementService.Validators;
using LT.DigitalOffice.Kernel;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LT.DigitalOffice.Kernel.Broker;

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
            services.Configure<RabbitMQOptions>(Configuration);

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
            services.AddTransient<ILeaveTimeRepository, LeaveTimeRepository>();
            services.AddTransient<IWorkTimeRepository, WorkTimeRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(tempApp => tempApp.Run(CustomExceptionHandler.HandleCustomException));

            UpdateDatabase(app);

            app.UseHttpsRedirection();

            app.UseRouting();

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
            const string serviceSection = "RabbitMQ";
            string serviceName = Configuration.GetSection(serviceSection)["Username"];
            string servicePassword = Configuration.GetSection(serviceSection)["Password"];

            string appSettingMassTransitUris = "MassTransitUris";
            string checkTokenUri = Configuration.GetSection(appSettingMassTransitUris)["CheckToken"];

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", host =>
                    {
                        host.Username($"{serviceName}_{servicePassword}");
                        host.Password(servicePassword);
                    });
                });

                x.AddRequestClient<IUserJwtRequest>(new Uri(checkTokenUri));
            });

            services.AddMassTransitHostedService();
        }
    }
}
