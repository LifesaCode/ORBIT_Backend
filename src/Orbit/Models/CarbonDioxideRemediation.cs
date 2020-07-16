using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Orbit.Annotations;

namespace Orbit.Models
{
    public enum BedOptions
    {
        Bed1,
        Bed2
    }
    /// <summary>
    /// current system for ISS has changed to testing a mineral 'sponge' (zeolite) that absorbs CO2 when cold, then
    /// releases it when heated or exposed to a vacuum (space. Another system being developed involves algae. For
    /// simplicity, this class is based on using the zeolite system.
    /// </summary>
   
    public class CarbonDioxideRemediation : IAlertableModel, IEquatable<CarbonDioxideRemediation>, ISeedableModel
    {
        #region Limits

        public const double TemperatureUpperLimit = 250;
        public const double TemperatureLowerLimit = 220;
        public const double TemperatureTolerance = 10;
        public const double CarbonDioxideOutputLimit = .5;  
        public const double CarbonDioxideOutputTolerance = 2;
        public const double CarbonDioxideStandbyValue = .5;
        
		Modes crewed = Modes.Crewed;
		int maxCO2 = 30;
		int maxCrewedCO2 = 30;
		int maxUncrewedCO2 = 80;
        // temporary pseudo-timer to trigger switching which bed is absorbing
        public int count = 0;
        private int countLength = 10;

        #endregion Limits

        #region Public Properties

        [NotMapped]
        public string ComponentName => "CarbonDioxideRemediation";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// current operating state of the system
        /// </summary>
        public SystemStatus Status { get; set; }
        
        /// <summary>
        /// Circulation fan to move air over carbon dioxide absorbing zeolite beds
        /// </summary>
        public bool FanOn { get; set; }

        /// <summary>
        /// determines bed that airflow will pass through
        /// </summary>
        public BedOptions BedSelectorValve { get; set; }

        /// <summary>
        /// specifies which bed is actively absorbing carbon dioxide
        /// </summary>
        public BedOptions AbsorbingBed { get; set; }
        
        /// <summary>
        /// specifies which bed is releasing carbon dioxide
        /// </summary>
        public BedOptions RegeneratingBed { get; set; }

        /// <summary>
        /// heater temp when zeolite adsorption capability is being 'regenerated' 400F designed temp, 126.7C (260F) on
        /// ISS to conserve power
        /// </summary>
        [Range(0, 232)]
        [IdealRange(120, 220)]
        [IdealValue(204)]
        public int Bed1Temperature { get; set; }

        [Range(0, 232)]
        [IdealRange(120, 220)]
        [IdealValue(204)]
        public int Bed2Temperature { get; set; }

        /// <summary>
        /// the level of co2 in the air leaving the unit and entering the cabin
        /// </summary>
        public double Co2OutputLevel { get; set; }

        /// <summary>
        /// the level of carbon dioxide in air entering the co2 scrubber
        /// </summary>
        [Range(0, 8)]
        [IdealRange(0, 5)]
        public double Co2Level { get; set; }

        #endregion Public Properties

        #region Constructors

        public CarbonDioxideRemediation() { }

        public CarbonDioxideRemediation(CarbonDioxideRemediation other)
        {
            ReportDateTime = DateTimeOffset.Now;
            Status = other.Status;
            FanOn = other.FanOn;
            BedSelectorValve = other.BedSelectorValve;
            AbsorbingBed = other.AbsorbingBed;
            RegeneratingBed = other.RegeneratingBed;
            Bed1Temperature = other.Bed1Temperature;
            Bed2Temperature = other.Bed2Temperature;
            Co2Level = other.Co2Level;

            GenerateData();
        }

    #endregion Constructors

        #region Public Methods

        public void SeedData()
        {
            Status = SystemStatus.Standby;
            FanOn = false;
            BedSelectorValve = BedOptions.Bed1;
            AbsorbingBed = BedOptions.Bed1;
            RegeneratingBed = BedOptions.Bed2;
            Bed1Temperature = 200;
            Bed2Temperature = 20;
            Co2OutputLevel = 0;
            Co2Level = 3;
        }

        public void ProcessData( )
        {
            GenerateData();

            if (Status == SystemStatus.Processing)
            {
                if (Co2Level <= CarbonDioxideOutputLimit)
                {
                    Status = SystemStatus.Standby;
                }
                else
                {
                    SimulateProcessing();
                }
            }
            else if (Status == SystemStatus.Standby)
            {
                if (Co2Level > CarbonDioxideOutputLimit)
                {
                    Status = SystemStatus.Processing;
                }
                else
                {
                    SimulateStandby();
                }
            }
            else { }
        }
		
		public void ChangeCrewedStatus(){
			if(crewed == Modes.Crewed){
				crewed = Modes.Uncrewed;
				maxCO2 = maxUncrewedCO2;
			}else{
				crewed = Modes.Crewed;
				maxCO2 = maxCrewedCO2;
			}
		}
			

        #endregion Public Methods

        #region Private Methods

        private void GenerateData()
        {
            Random rand = new Random();

            Co2Level = rand.Next(0, maxCO2) / 10.0;

            if(Status == SystemStatus.Processing)
            {
				FanOn = true;
                if (RegeneratingBed == BedOptions.Bed1)
                {
                    Bed1Temperature = rand.Next(175, 232);
                    Bed2Temperature = rand.Next(19, 32);
                }
                else
                {
                    Bed1Temperature = rand.Next(19, 32);
                    Bed2Temperature = rand.Next(175, 232);
                }
            }
            else
            {
				FanOn = false;
                Bed1Temperature = rand.Next(19, 32);
                Bed2Temperature = rand.Next(19, 32);
            }

            if (rand.Next(0, 10) == 4)
            {
                FanOn = !FanOn;
            }
        }
        private void SimulateProcessing()
        {
            if (!FanOn || Co2OutputLevel > CarbonDioxideOutputLimit)
            {
                //Trouble();
            }

            if (count < countLength)
            {
                // bed1 is absorbing and sould be cool, bed2 is regenerating and should be hot
                if (BedSelectorValve == BedOptions.Bed1)
                {
                    if ((Bed2Temperature > TemperatureUpperLimit)
                        || (Bed2Temperature < TemperatureLowerLimit)
                        || (Bed1Temperature > TemperatureLowerLimit))
                    {
                        //Trouble();
                    }
                }

                else
                {
                    if ((Bed1Temperature > TemperatureUpperLimit)
                        || (Bed1Temperature < TemperatureLowerLimit)
                        || (Bed2Temperature > TemperatureLowerLimit))
                    {
                       //Trouble();
                    }
                }

                count++;
            }
            else
            {        
                count = 0;
                if(BedSelectorValve == BedOptions.Bed1)        
                {
                    AbsorbingBed = BedOptions.Bed2;
                    RegeneratingBed = BedOptions.Bed1;
                    BedSelectorValve = BedOptions.Bed2;
                }
                else
                {
                    AbsorbingBed = BedOptions.Bed1;
                    RegeneratingBed = BedOptions.Bed2;
                    BedSelectorValve = BedOptions.Bed1;
                }
            }
        }

        private void SimulateStandby()
        {
            FanOn = false;
        }

        private void Trouble()
        {
            Status = SystemStatus.Trouble;
            SimulateStandby();
        }

        #endregion Private Methods

        #region Check Alerts 

        private IEnumerable<Alert> CheckFan()
        {
            if(Status == SystemStatus.Processing)
            {
                if(!FanOn)
                {
                    yield return this.CreateAlert(a => a.FanOn, "No fan running while system processing", AlertLevel.HighError);
                }
            }
            else if(Status == SystemStatus.Standby)
            {
                if (FanOn)
                {
                    yield return this.CreateAlert(a => a.FanOn, "Fan running while system in standby", AlertLevel.HighWarning);
                }
            }
            else
            {
                yield return this.CreateAlert(a => a.FanOn);
            }
        }
        private IEnumerable<Alert> CheckRegenerationTemp()
        {
            if(RegeneratingBed == BedOptions.Bed1)
            {
                if (Bed1Temperature > TemperatureUpperLimit)
                {
                    yield return this.CreateAlert(a => a.Bed1Temperature, "Bed 1 temperature is above maximum", AlertLevel.HighError);
                }
                else if (Bed1Temperature >= (TemperatureUpperLimit - TemperatureTolerance))
                {
                    yield return this.CreateAlert(a => a.Bed1Temperature, "Bed 1 temperature is elevated", AlertLevel.HighWarning);
                }
                else if (Bed1Temperature < TemperatureLowerLimit)
                {
                    yield return this.CreateAlert(a => a.Bed1Temperature, "Bed 1 temperature is below minimum", AlertLevel.LowError);
                }
                else if (Bed1Temperature <= (TemperatureLowerLimit + TemperatureTolerance))
                {
                    yield return this.CreateAlert(a => a.Bed1Temperature, "Bed 1 temperature is low", AlertLevel.LowWarning);
                }
                else
                {
                    yield return this.CreateAlert(a => a.Bed1Temperature);
                }
            }
            else
            {
                if (Bed2Temperature > TemperatureUpperLimit)
                {
                    yield return this.CreateAlert(a => a.Bed2Temperature, "Bed 2 temperature is above maximum", AlertLevel.HighError);
                }
                else if (Bed2Temperature >= (TemperatureUpperLimit - TemperatureTolerance))
                {
                    yield return this.CreateAlert(a => a.Bed2Temperature, "Bed 2 temperature is elevated", AlertLevel.HighWarning);
                }
                else if (Bed2Temperature < TemperatureLowerLimit)
                {
                    yield return this.CreateAlert(a => a.Bed2Temperature, "Bed 2 temperature is below minimum", AlertLevel.LowError);
                }
                else if (Bed2Temperature <= (TemperatureLowerLimit + TemperatureTolerance))
                {
                    yield return this.CreateAlert(a => a.Bed2Temperature, "Bed 2 temperature is low", AlertLevel.LowWarning);
                }
                else
                {
                    yield return this.CreateAlert(a => a.Bed2Temperature);
                }
            }
        }
        private IEnumerable<Alert> CheckOutputCo2Level()
        {
            if(Co2Level > CarbonDioxideOutputLimit )
            {
                yield return this.CreateAlert(a => a.Co2Level, "Carbon dioxide output is above maximum", AlertLevel.HighError);
            }
            else if(Co2Level >= (CarbonDioxideOutputLimit - CarbonDioxideOutputTolerance))
            {
                yield return this.CreateAlert(a => a.Co2Level, "CarbonDioxide output is elevated", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.Co2Level);
            }
        }
        private IEnumerable<Alert> CheckBedsAlternate()
        {
            if(AbsorbingBed == RegeneratingBed)
            {
                yield return this.CreateAlert(a => a.RegeneratingBed, "Regenerating bed is same as absorbing bed", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.RegeneratingBed);
            }
        }
        
        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return this.CheckRegenerationTemp()
                .Concat(CheckFan())
                .Concat(CheckOutputCo2Level())
                .Concat(CheckBedsAlternate());
        }

        #endregion Check Alerts

        #region Equality Members

        public bool Equals(CarbonDioxideRemediation other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return this.ReportDateTime.Equals(other.ReportDateTime)
                && this.Status == other.Status
                && this.FanOn == other.FanOn
                && this.BedSelectorValve == other.BedSelectorValve
                && this.AbsorbingBed == other.AbsorbingBed
                && this.RegeneratingBed == other.RegeneratingBed
                && this.Bed1Temperature == other.Bed1Temperature
                && this.Bed2Temperature == other.Bed2Temperature
                && this.Co2Level == other.Co2Level;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CarbonDioxideRemediation other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.FanOn,
                this.BedSelectorValve,
                this.AbsorbingBed,
                this.RegeneratingBed,
                this.Bed1Temperature,
                (this.Bed2Temperature, this.Co2Level)
                );
        }

        public static bool operator ==(CarbonDioxideRemediation left, CarbonDioxideRemediation right) => Equals(left, right);
        public static bool operator !=(CarbonDioxideRemediation left, CarbonDioxideRemediation right) => !Equals(left, right);

        #endregion Equality Members
    }
}