using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using NLog;
using Orbit.Components;
using Orbit.Models;

namespace Orbit.Util
{
    public sealed class EventMonitor : IEventMonitor, IDisposable
    {
        private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(LogManager.GetCurrentClassLogger);
        private static ILogger Logger => _logger.Value;
        // TODO: Determine an appropriate wait period

        private const double SecondsDelay = 2.0;

        private Task? _eventThread;

        private readonly Lazy<SynchronizationContext> _synchronization = new Lazy<SynchronizationContext>(() => OrbitServiceProvider.Instance.GetService<SynchronizationContext>());
        private SynchronizationContext? SynchronizationContext => _synchronization.Value;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
      
        #region Singleton pattern

        private EventMonitor()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => this._cancellationTokenSource.Cancel();
        }

        private static readonly Lazy<EventMonitor> _instance = new Lazy<EventMonitor>(() => new EventMonitor());

        public static IEventMonitor Instance => _instance.Value;

        #endregion Singleton pattern

        /// <summary>
        /// Returns a collection of the Report types defined in this assembly.
        /// </summary>
        private IReadOnlyCollection<Type> ReportTypes => this._reportTypes.Value;

        private readonly Lazy<IReadOnlyCollection<Type>> _reportTypes = new Lazy<IReadOnlyCollection<Type>>(
            () =>
                Assembly.GetExecutingAssembly()
                    .ExportedTypes
                    .Where(t => !t.IsInterface && t.GetInterfaces().Contains(typeof(IModel)))
                    .ToList());

        /// <summary>
        /// Invoked when the event monitor thread starts.
        /// </summary>
        public event EventHandler? Started;

        /// <summary>
        /// Invoked when the event monitor thread stops.
        /// </summary>
        public event EventHandler? Stopped;

        /// <summary>
        /// Triggered for every newly returned point of data for each registered report. The sender param is the component reporting the data.
        /// </summary>
        public event EventHandler<ValueReadEventArgs>? NewValueRead;

        /// <summary>
        /// Triggered when any alert is reported for a newly read value. The sender param is the component reporting the data.
        /// </summary>
        public event EventHandler<AlertEventArgs>? AlertReported;

        private static Type ExplicitlyMappedComponent(Type reportType)
        {
            var explicitType = typeof(IMonitoredComponent<>).MakeGenericType(reportType);

            Type explicitlyDefined = Assembly
                .GetExecutingAssembly()
                .ExportedTypes
                .SingleOrDefault(a => a.GetInterfaces().Contains(explicitType));

            // If there exists a class explicitly defined to handle the given report, use that Otherwise, assume it only
            // generates one report and create a MonitoredComponent<T> of the given type to handle it

            return explicitlyDefined ?? typeof(MonitoredComponent<>).MakeGenericType(reportType);
        }

        private readonly ConcurrentDictionary<Type, Type> _componentByReportType = new ConcurrentDictionary<Type, Type>();

        private async Task RunInCorrectSynchronizationContext(Action action)
        {
            // This is some complicated mumbo-jumbo that will really just make the WPF-side code cleaner.
            // Basically this should help so that event-handlers on the client don't have to check what thread is calling and do a whole bunch of context switching.
            var threadContext = SynchronizationContext;
            if (threadContext != null)
                await threadContext;

            action();
        }

        private Task OnAlertReported(object sender, AlertEventArgs args)
        {
            return this.RunInCorrectSynchronizationContext(() => this.AlertReported?.Invoke(sender, args));
        }

        private Task OnNewValueRead(object sender, ValueReadEventArgs args)
        {
            return this.RunInCorrectSynchronizationContext(() => this.NewValueRead?.Invoke(sender, args));
        }

        /// <summary>
        /// This method iterates through all known report types and generates alerts for them. It is then up to the
        /// consumer of said alerts as to how the alerts will be handled.
        /// </summary>
        private async Task IterateReportedValues()
        {
            this.Started?.Invoke(this, EventArgs.Empty);
            var token = this._cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                using IServiceScope scope = OrbitServiceProvider.Instance.CreateScope();
                var provider = scope.ServiceProvider;

                // Task.WhenAll allows each of these to run concurrently, effectively making this loop run in parallel
                await Task.WhenAll(this.ReportTypes.Select(async reportType =>
                {
                    try
                    {
                        Logger.Debug("Iterating type {reportType}", reportType);
                        Type componentType =
                                this._componentByReportType.GetOrAdd(reportType, ExplicitlyMappedComponent);

                        var component = (IMonitoredComponent)provider.GetRequiredService(componentType);
                        token.ThrowIfCancellationRequested();

                        // TODO: Should we check here that the value is actually new?
                        IModel? report = await component.GetLatestReportAsync(token);
                        
                        if (report == null)
                            return;

                        token.ThrowIfCancellationRequested();
                        await this.OnNewValueRead(component, new ValueReadEventArgs(report)).ConfigureAwait(true);

                        if (report is IAlertableModel a)
                        {
                            foreach (var alert in a.GenerateAlerts())
                            {
                                var args = new AlertEventArgs(a, alert);
                                await OnAlertReported(component, args).ConfigureAwait(true);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore cancellation errors
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error in EventMonitor loop");
                    }
                }));

                await Task.Delay(TimeSpan.FromSeconds(SecondsDelay), token).ConfigureAwait(false);
            }

            this.Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            if (this._eventThread == null)
            {
                this._eventThread = Task.Run(this.IterateReportedValues, this._cancellationTokenSource.Token);
            }
        }

        public void Stop()
        {
            if (!this._cancellationTokenSource.IsCancellationRequested)
            {
                this._cancellationTokenSource.Cancel();
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this._cancellationTokenSource.Dispose();
            this._eventThread?.Dispose();
        }

        #endregion Implementation of IDisposable
    }

}