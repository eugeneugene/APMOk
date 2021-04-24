﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace RecurrentTasks
{
    public static class RecurrentTasksServiceCollectionExtensions
    {
        public static IServiceCollection AddTask<TRunnable>(
            this IServiceCollection services,
            Action<TaskOptions<TRunnable>> optionsAction = null,
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

            services.AddSingleton(_ =>
            {
                var o = new TaskOptions<TRunnable>();
                optionsAction?.Invoke(o);
                return o;
            });

            services.AddSingleton<ITask<TRunnable>, TaskRunner<TRunnable>>();
            services.AddSingleton<IHostedService>(s => s.GetRequiredService<ITask<TRunnable>>());

            return services;
        }
    }
}
