using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Orbit.Annotations;

namespace Orbit.Models
{
    public class WasteWaterStorageTankData : IAlertableModel, ISeedableModel
    {
        public int fillIncrement = 2;
        public int emptyIncrement = 3;
        public int full = 100;
        public int empty = 0;
        public int tolerance = 5;
 
        /// <summary>
        /// The name of the component.
        /// </summary>
        [NotMapped]
        public string ComponentName => "WasteWaterStorageTank";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// distinguish between separate tanks ('Tank 1', 'Tank 2')
        /// </summary>
        public string TankId { get; set; } = "Main Tank";

        /// <summary>
        /// Current level of the waste water collection tank receives output from urine processing system and other
        /// wastewater sources (hygiene...)
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public double Level { get; set; }

        public void ProcessData(SystemStatus urineProcessor, SystemStatus waterProcessor)
        {
            if(urineProcessor == SystemStatus.Processing)
            {
                if(Level <= (full - fillIncrement))
                {
                    Level += fillIncrement;
                }
                else
                {
                    Level = full;
                }
            }
            if (waterProcessor == SystemStatus.Processing)
            {
                if (Level >= (empty + emptyIncrement))
                {
                    Level -= emptyIncrement;
                }
                else
                {
                    Level = empty;
                }
            }
        }

        void ISeedableModel.SeedData()
        {
            TankId = "Main";
            Level = 30;
        }

        public void SeedData()
        {
            TankId = "Main";
            Level = 30;
        }


        IEnumerable<Alert> IAlertableModel.GenerateAlerts()

        {
            if (this.Level >= full)
            {
                yield return this.CreateAlert(a => a.Level, "Tank overflowing", AlertLevel.HighError);
            }
            else if (this.Level >= (full - tolerance))
            {
                yield return this.CreateAlert(a => a.Level, "Water level high", AlertLevel.HighWarning);
            }
            else if (this.Level <= empty)
            {
                yield return this.CreateAlert(a => a.Level, "Water level very low", AlertLevel.HighError);
            }
            else if (this.Level < (empty + tolerance))
            {
                yield return this.CreateAlert(a => a.Level, "Water level low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.Level);
            }
        }

        #region Equality members

        public override bool Equals(object obj)
        {
            if (obj is WasteWaterStorageTankData data)
            {
                return data.TankId == this.TankId && Math.Abs(data.Level - this.Level) < 0.001;
            }
            return false;
        }

        public override int GetHashCode() => HashCode.Combine(this.TankId, this.Level);

        #endregion Equality members
    }
}