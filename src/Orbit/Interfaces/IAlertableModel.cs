using System;
using System.Collections.Generic;

using Orbit.Models;

namespace Orbit
{
    public interface IModel
    {
        /// <summary>
        /// The name of the component.
        /// </summary>
        string ComponentName { get; }

        DateTimeOffset ReportDateTime { get; }
    }

    /// <summary>
    /// Indicates a component that is able to generate snapshot alerts of its values.
    /// </summary>
    public interface IAlertableModel : IModel
    {
        IEnumerable<Alert> GenerateAlerts();
    }
}