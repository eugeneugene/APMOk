using APMOkSvc.Code;
using APMOkSvc.Data;
using APMOkSvc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace APMOkSvc
{
    internal class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConnectionStringsConfiguration connectionStringsConfiguration = new(_configuration);
            services.AddDbContext<DataContext>(options =>
            {
                var DataContext = connectionStringsConfiguration.DataContext.Value;
                if (string.IsNullOrEmpty(DataContext))
                    throw new Exception("DataContext is not set and must not be empty");

                var connectionOptions = new SqliteConnectionStringBuilder(DataContext);
                var dbFile = connectionOptions.DataSource;
                var dbPath = Path.GetDirectoryName(Path.GetFullPath(dbFile));
                if (string.IsNullOrEmpty(dbPath))
                    throw new Exception("DataContext path must not be empty");

                Directory.CreateDirectory(dbPath);

                options.UseSqlite(connectionStringsConfiguration.DataContext.Value);
                options.EnableSensitiveDataLogging(_environment.IsDevelopment());
                options.EnableDetailedErrors(_environment.IsDevelopment());
            });
            services.AddHostedService<StartupHostedService>();
            services.AddTransient<DiskInfoServiceImpl>();
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
