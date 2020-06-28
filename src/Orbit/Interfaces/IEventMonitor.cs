using System;
using Orbit.Models;

namespace Orbit
{
    public interface IEventMonitor
    {
        void Start();

        void Stop();

        event EventHandler? Started;

        event EventHandler? Stopped;

        event EventHandler<ValueReadEventArgs>? NewValueRead;

        /// <summary>
        /// Triggered when any alert is reported for a newly read value.
        /// </summary>
        event EventHandler<AlertEventArgs>? AlertReported;
    }


}