using System;
using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Orbit.Components;
using Orbit.Data;

namespace Orbit.Util
{
    public class OrbitServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _provider;

        private OrbitServiceProvider()
        {
            this._provider = this.Build();
        }

        private static readonly Lazy<OrbitServiceProvider> _instance = new Lazy<OrbitServiceProvider>(() => new OrbitServiceProvider());

        public static IServiceProvider Instance => _instance.Value;

        public static event EventHandler<IServiceCollection>? OnRegisteringServices;

        private IServiceProvider Build()
        {
            try
            {
                var services = new ServiceCollection();
                this.RegisterServices(services);
                OnRegisteringServices?.Invoke(this, services);
                ServiceProvider prov = services.BuildServiceProvider();
                EventMonitor.Instance.Start();
                return prov;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        private void RegisterServices(IServiceCollection services)
        {
            /* Registering "open" types allows the provider to map the requested type parameter appropriately,
             * assuming it exists in the database
             *
             * services.AddScoped(typeof(IMonitoredComponent<>), typeof(MonitoredComponent<>));
             * services.AddScoped(typeof(MonitoredComponent<>));
             */
            services.AddScoped(typeof(MonitoredComponent<>));
            services.AddScoped(typeof(IMonitoredComponent<>), typeof(MonitoredComponent<>));

            // Another way could be just directly registering the concrete class
            //services.AddScoped<BatteryComponent>();

            // Allow the singleton instance of the event monitor to be retrieved by the service provider if desired
            services.AddSingleton(_ => EventMonitor.Instance);
            services.AddSingleton(_ => DataGenerator.Instance);

            //TODO: Replace the following with actual database implementation when ready
            services.AddDbContext<OrbitDbContext>(o =>
            {
                o.UseInMemoryDatabase("OrbitDb");
            });

            // Reroutes EF Core logs to go through NLog
            services.AddLogging(b =>
            {
                b.ClearProviders();
                // Capture all logs into NLog, let the config file decide what to do about them.
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddNLog();
            });

        }

        object? IServiceProvider.GetService(Type serviceType) => this._provider.GetService(serviceType);
    }
}