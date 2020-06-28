using System.Collections.Generic;
using System.Text;

namespace Orbit.Models
{
    public class AlertEventArgs : ValueReadEventArgs
    {
        public AlertEventArgs(IModel report, Alert alert) : base(report)
        {
            this.Alert = alert;
        }

        public Alert Alert { get; }
    }
}
