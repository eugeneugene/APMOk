﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace APMOkLib.RecurrentTasks
{
    public static class RecurrentTasksServiceCollectionExtensions
    {
        public static IServiceCollection AddTask<TRunnable>(
            this IServiceCollection services,
            Action<TaskStartupOptions<TRunnable>> optionsAction = null,
            ServiceLifetime runnableLifetime = ServiceLifetime.Transient)
            where TRunnable : IRunnable
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var runnableType = typeof(TRunnable);

            // Register TRunnable (if not an interface or abstract class) in DI container, if not registered already
            if (!runnableType.IsAbstract && !services.Any(x => x.ServiceType == runnableType))
            {
                services.Add(new ServiceDescriptor(runnableType, runnableType, runnableLifetime));
            }

            services.AddSingleton(serviceProvider =>
            {
                var options = new TaskStartupOptions<TRunnable>(serviceProvider, new TaskOptions<TRunnable>());
                optionsAction?.Invoke(options);
                return options.TaskOptions;
            });

            services.AddSingleton<ITask<TRunnable>, TaskRunner<TRunnable>>();
            services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ITask<TRunnable>>());

            return services;
        }
    }
}
