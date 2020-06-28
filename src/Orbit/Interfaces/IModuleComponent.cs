using System.Collections.Generic;
using System.Threading;
using Orbit.Models;

namespace Orbit
{
    /// <summary>
    ///  Base interface for module components.
    /// </summary>
    public interface IModuleComponent
    {
        /// <summary>
        /// The name of the component.
        /// </summary>
        string ComponentName { get; }
    }
}