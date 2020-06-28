using System;

namespace Orbit.Models
{
    /// <summary>
    /// Event arguments passed whenever the latest value is read from the database
    /// </summary>
    public class ValueReadEventArgs : EventArgs
    {
        public ValueReadEventArgs(IModel report)
        {
            this.Report = report;
        }

        public IModel Report { get; }
    }
}