using APMOkLib.CustomConfiguration;
using APMOkLib.RecurrentTasks;
using APMOkSvc.Code;
using APMOkSvc.Data;
using APMOkSvc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace APMOkSvc
{
    internal class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ConfigurationParameterFactory>();
            services.AddSingleton<ITasksStartupConfiguration, TasksStartupConfiguration>();
            services.AddSingleton<IConnectionStringsConfiguration, ConnectionStringsConfiguration>();

            services.AddTask<PowerStateReaderTask>(options =>
            {
                options.TaskOptions.Interval = TimeSpan.FromSeconds(1);
                options.TaskOptions.FirstRunDelay = TimeSpan.FromSeconds(5);
            });
            services.AddDbContext<DataContext>();
            services.AddHostedService<PowerStateWatcher>();
            services.AddSingleton<PowerStateContainer>();
            services.AddTransient<DiskInfoServiceImpl>();
            services.AddTransient<APMServiceImpl>();
            services.AddTransient<PowerStateServiceImpl>();
            services.AddTransient<ConfigurationServiceImpl>();
            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DiskInfoGRPCService>();
                endpoints.MapGrpcService<APMGRPCService>();
                endpoints.MapGrpcService<PowerStateGRPCService>();
                endpoints.MapGrpcService<ConfigurationGRPCService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
