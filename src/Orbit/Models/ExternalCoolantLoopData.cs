using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orbit.Annotations;

using System.Linq;

namespace Orbit.Models
{
    public class ExternalCoolantLoopData : IAlertableModel, IEquatable<ExternalCoolantLoopData>, ISeedableModel
    {
        #region Limits

        public const int radiatorRotationUpperLimit = 215;
        public const int radiatorRotationLowerLimit = -215;
        public const int radiatorRotationTolerance = 10;

        public const int fluidPressureUpperLimit = 3309;
        public const int fluidPressureLowerLimit = 345;
        public const int fluidPressureTolerance = 827;

        public const double outputFluidTemperatueUpperLimit = 8.1;
        public const double outputFluidTemperatureLowerLimit = 1.6;
        public const double outputFluidTemperatureTolerance = 2;

        public int mixValveUpperLimit = 100;
        public int mixValveLowerLimit = 0;
        public int mixValveTolerance = 5;

        public bool radiatorRotationIncreasing = true;

        #endregion Limits

        #region Public Properties

        public string ComponentName => "ExternalCoolantSystem";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// overall status of the system
        /// </summary>
        public SystemStatus Status { get; set; }

        ///<summary>
        /// denotes whether the system is operating in automatic or manual capacity
        /// </summary>
        public bool IsManualMode { get; set; }

        /// <summary>
        /// position in degrees from neutral point.
        /// </summary>
        [Range(-215, 215)]
        [IdealRange(-205, 205)]
        [UnitType("deg.")]
        public double RadiatorRotation { get; set; }

        /// <summary>
        /// pump for fluid circuit A
        /// </summary>
        public bool PumpAOn { get; set; }

        /// <summary>
        /// pump for fluid circuit B
        /// </summary>
        public bool PumpBOn { get; set; }

        /// <summary>
        /// position of the valve that mixes 'hot' fluid returning from heat exchanger and 'cold' fluid 
        /// returning from radiator. Acts like a shower temp valve. 0 = all 'hot', 100 = all 'cold'
        /// </summary>
        [Range(0, 100)]
        [UnitType("%")]
        public int MixValvePosition { get; set; }

        /// <summary>
        /// pressure of fluid in loop A
        /// </summary>
        [Range(345, 3309)]
        [IdealRange(1172, 2482)]
        [IdealValue(2068)]
        [UnitType("kpa")]
        public int LineAPressure { get; set; }

        /// <summary>
        /// pressure of fluid in loop B
        /// </summary>
        [Range(345, 3309)]
        [IdealRange(1172, 2482)]
        [IdealValue(2068)]
        [UnitType("kpa")]
        public int LineBPressure { get; set; }

        /// <summary>
        /// heats the external coolant if the heat offload from the station is too low to warm the -40F degree ammonia
        /// to at least 36F to prevent the internal coolant fluid (water) from freezing
        /// </summary>
        public bool LineHeaterOn { get; set; }

        /// <summary>
        /// True if radiator is extended (deployed), false if retracted (not deployed)
        /// </summary>
        public bool RadiatorDeployed { get; set; }

        /// <summary>
        /// temperature of fluid returning from radiator flow control valve to internal/external heat exchanger
        /// </summary>
        [Range(1.6, 8.1)]
        [IdealRange(2.2, 6.1)]
        [UnitType("C")]
        public double OutputFluidTemperature { get; set; }

        /// <summary>
        /// goal output fluid temperature
        /// </summary>
        [Range(2.2, 6.1)]
        [UnitType("C")]
        public double SetTemperature { get; set; }

        #endregion Public Properties

        #region Constructors

        public ExternalCoolantLoopData() { }

        public ExternalCoolantLoopData(ExternalCoolantLoopData other)
        {
            Status = other.Status;
            RadiatorRotation = other.RadiatorRotation;
            PumpAOn = other.PumpAOn;
            PumpBOn = other.PumpBOn;
            MixValvePosition = other.MixValvePosition;
            LineAPressure = other.LineAPressure;
            LineBPressure = other.LineBPressure;
            LineHeaterOn = other.LineHeaterOn;
            RadiatorDeployed = other.RadiatorDeployed;
            OutputFluidTemperature = other.OutputFluidTemperature;
            SetTemperature = other.SetTemperature;

            GenerateData();
        }

        #endregion Constructors

        #region Methods

        public void SeedData()
        {
            Status = SystemStatus.On;
            IsManualMode = false;
            RadiatorRotation = 0;
            PumpAOn = true;
            PumpBOn = true;
            MixValvePosition = 25;
            LineAPressure = 2050;
            LineBPressure = 2060;
            LineHeaterOn = false;
            RadiatorDeployed = true;
            OutputFluidTemperature = 2.8;
            SetTemperature = 2.8;
        }
        
        public void ProcessData()
        {
            GenerateData();

            if (IsManualMode)
                return;

            // take actions if system is 'On' or 'Processing', do nothing otherwise (crew selected off)
            if((Status == SystemStatus.On) || (Status == SystemStatus.Processing))
            {
                if (OutputFluidTemperature > SetTemperature)
                {
                    // open the mixing valve and allow more 'cold' fluid in the mix
                    IncreaseFluidMix();
                }
                else if (OutputFluidTemperature < SetTemperature)
                {
                    // close the mixing valve to keep more 'hot' fluid in the mix
                    DecreaseFluidMix();
                }

                // simulate radiator rotation
                RotateRadiator();
            }
        }

        private void GenerateData()
        {
            Random rand = new Random();

            LineAPressure = rand.Next(1500, 3000);
            LineBPressure = rand.Next(1500, 3000);

            // Simulate temp decrease when pump not on (not exchanging heat with station)
            if(Status == SystemStatus.Standby)
            {
                if(OutputFluidTemperature > -120.0)
                {
                    OutputFluidTemperature -= 1.75;
                }
            }
            else
            {
                OutputFluidTemperature = rand.Next(0, 81) / 10.0;
				
				// simulate a perodic pump failure
				if (rand.Next(0, 10) == 5)
				{
					PumpAOn = !PumpAOn;
				}
				if (rand.Next(0, 10) == 9)
				{
					PumpBOn = !PumpBOn;
				}
            }

        }

        private void IncreaseFluidMix()
        {
            // need better formula: if valve opened 1 'degree', how much would temp change, use difference
            // in outflow and set temp to determine amount to change valve

            if (LineHeaterOn)
            {
                // if heater is on, turn it off and recheck temp before moving valve position
                LineHeaterOn = false;
            }
            else
            {
                if (MixValvePosition < mixValveUpperLimit)
                {
                    MixValvePosition++;
                }
                else
                {
                    MixValvePosition = mixValveUpperLimit;
                }
            }
        }

        private void DecreaseFluidMix()
        {
            // Sould we use a formula? ex: if valve closed 1 'degree', how much would temp change, 
            // then use that change in outflow and set temp to determine amount to change valve
            if (MixValvePosition == mixValveLowerLimit)
            {
                // heat load from station is too low to keep fluid above min temp, turn heater on
                LineHeaterOn = true;
            }
            else
            {
                MixValvePosition--;
            }
        }

        private void RotateRadiator()
        {
            if (RadiatorDeployed)
            {
                // rotate radiator back and forth between range bounds
                if (radiatorRotationIncreasing && RadiatorRotation < (radiatorRotationUpperLimit - radiatorRotationTolerance))
                {
                    RadiatorRotation += .5;
                }
                else if (!radiatorRotationIncreasing && RadiatorRotation > (radiatorRotationLowerLimit + radiatorRotationTolerance))
                {
                    RadiatorRotation -= .5;
                }
                else
                {
                    // reached a bound, switch rotation direction
                    radiatorRotationIncreasing = !radiatorRotationIncreasing;
                }
            }
            else
            {
                // assumes radiator returns to a 'neutral' state when retracted
                RadiatorRotation = 0;
            }
        }

        public void ToggleOnOff()
        {
            if(Status == SystemStatus.Standby)
            {
                Status = SystemStatus.On;
                PumpAOn = true;
                PumpBOn = true;
            }
            else
            {
                Status = SystemStatus.Standby;
                PumpAOn = false;
                PumpBOn = false;
            }
        }

        public void ToggleRadiatorDeployed()
        {
            if(RadiatorDeployed){
				RadiatorDeployed = false;
				RadiatorRotation = 0;
			}
			else{
				RadiatorDeployed = true;
			}
        }

        #endregion Methods

        #region Alert Generation

        private IEnumerable<Alert> CheckPumpA()
        {
            if (!PumpAOn)
            {
                yield return this.CreateAlert(a => a.PumpAOn, "External coolant pump A is off", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.PumpAOn);
            }
        }

        private IEnumerable<Alert> CheckPumpB()
        {
            if (!PumpBOn)
            {
                yield return this.CreateAlert(a => a.PumpBOn, "External coolant pump B is off", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.PumpBOn);
            }
        }

        private IEnumerable<Alert> CheckMixValvePosition()
        {
            if (MixValvePosition >= mixValveUpperLimit)
            {
                yield return this.CreateAlert(a => a.MixValvePosition, "Mix valve position is at maximum", AlertLevel.HighError);
            }
            else if (MixValvePosition >= (mixValveUpperLimit - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.MixValvePosition, "Mix valve position is approaching maximum", AlertLevel.HighWarning);
            }
            else if (MixValvePosition <= mixValveLowerLimit)
            {
                yield return this.CreateAlert(a => a.MixValvePosition, "Mix valve position is at minimum", AlertLevel.LowError);
            }
            else if (MixValvePosition <= (mixValveLowerLimit - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.MixValvePosition, "Mix valve position is approaching minimum", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.MixValvePosition);
            }
        }

        private IEnumerable<Alert> CheckRadiatorDeployed()
        {
            if (!RadiatorDeployed)
            {
                yield return this.CreateAlert(a => a.RadiatorDeployed, "External coolant radiator is retracted", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.RadiatorDeployed);
            }
        }

        private IEnumerable<Alert> CheckRadiatorRotation()
        {
            if (RadiatorRotation > radiatorRotationUpperLimit)
            {
                yield return this.CreateAlert(a => a.RadiatorRotation, "Coolant radiator has exceeded maximum available rotation", AlertLevel.HighError);
            }
            else if (RadiatorRotation >= (radiatorRotationUpperLimit - radiatorRotationTolerance))
            {
                yield return this.CreateAlert(a => a.RadiatorRotation, "Coolant radiator has exceeded allowed rotation", AlertLevel.HighWarning);
            }
            else if (RadiatorRotation < radiatorRotationLowerLimit)
            {
                yield return this.CreateAlert(a => a.RadiatorRotation, "Coolant radiator has exceeded maximum available rotation", AlertLevel.LowError);
            }
            else if (RadiatorRotation <= (radiatorRotationLowerLimit + radiatorRotationTolerance))
            {
                yield return this.CreateAlert(a => a.RadiatorRotation, "Coolant radiator has exceeded allowed rotation", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.RadiatorRotation);
            }
        }

        private IEnumerable<Alert> CheckLineAPressure()
        {
            if (LineAPressure >= fluidPressureUpperLimit)
            {
                yield return this.CreateAlert(a => a.LineAPressure, "Coolant line A pressure is above maximum", AlertLevel.HighError);
            }
            else if (LineAPressure >= (fluidPressureUpperLimit - fluidPressureTolerance))
            {
                yield return this.CreateAlert(a => a.LineAPressure, "Coolant line A pressure is too high", AlertLevel.HighWarning);
            }
            else if (LineAPressure < fluidPressureLowerLimit)
            {
                yield return this.CreateAlert(a => a.LineAPressure, "Coolant line A pressure is below minimum", AlertLevel.LowError);
            }
            else if (LineAPressure <= (fluidPressureLowerLimit + fluidPressureTolerance))
            {
                yield return this.CreateAlert(a => a.LineAPressure, "Coolant line A pressure is too low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.LineAPressure);
            }
        }

        private IEnumerable<Alert> CheckLineBPressure()
        {
            if (LineBPressure >= fluidPressureUpperLimit)
            {
                yield return this.CreateAlert(a => a.LineBPressure, "Coolant line B pressure is above maximum", AlertLevel.HighError);
            }
            else if (LineBPressure >= (fluidPressureUpperLimit - fluidPressureTolerance))
            {
                yield return this.CreateAlert(a => a.LineBPressure, "Coolant line B  pressure is too high", AlertLevel.HighWarning);
            }
            else if (LineBPressure < fluidPressureLowerLimit)
            {
                yield return this.CreateAlert(a => a.LineBPressure, "Coolant line B pressure is below minimum", AlertLevel.LowError);
            }
            else if (LineBPressure <= (fluidPressureLowerLimit + fluidPressureTolerance))
            {
                yield return this.CreateAlert(a => a.LineBPressure, "Coolant line B pressure is too low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.LineBPressure);
            }
        }

        private IEnumerable<Alert> CheckOutputFluidTemp()
        {
            if (OutputFluidTemperature >= outputFluidTemperatueUpperLimit)
            {
                yield return this.CreateAlert(a => a.OutputFluidTemperature, "External coolant temperature is above maximum", AlertLevel.HighError);
            }
            else if (OutputFluidTemperature >= (outputFluidTemperatueUpperLimit - outputFluidTemperatureTolerance))
            {
                yield return this.CreateAlert(a => a.OutputFluidTemperature, "External coolant temperature is too high", AlertLevel.HighWarning);
            }
            else if (OutputFluidTemperature <= outputFluidTemperatureLowerLimit)
            {
                yield return this.CreateAlert(a => a.OutputFluidTemperature, "External coolant temperature is below minimum", AlertLevel.LowError);
            }
            else if (OutputFluidTemperature <= (outputFluidTemperatureLowerLimit + outputFluidTemperatureTolerance))
            {
                yield return this.CreateAlert(a => a.OutputFluidTemperature, "External coolant temperature is low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.OutputFluidTemperature);
            }
        }

        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return this.CheckPumpA()
                .Concat(CheckPumpB())
                .Concat(CheckMixValvePosition())
                .Concat(CheckRadiatorDeployed())
                .Concat(CheckRadiatorRotation())
                .Concat(CheckLineAPressure())
                .Concat(CheckLineBPressure())
                .Concat(CheckOutputFluidTemp());
        }

        public IEnumerable<Alert> GetAlerts()
        {
            return this.CheckPumpA()
                .Concat(CheckPumpB())
                .Concat(CheckMixValvePosition())
                .Concat(CheckRadiatorDeployed())
                .Concat(CheckRadiatorRotation())
                .Concat(CheckLineAPressure())
                .Concat(CheckLineBPressure())
                .Concat(CheckOutputFluidTemp());
        }

        #endregion Alert generation

        #region Equality Members

        public bool Equals(ExternalCoolantLoopData other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return this.ReportDateTime.Equals(other.ReportDateTime)
                && this.Status == other.Status
                && this.RadiatorRotation == other.RadiatorRotation
                && this.PumpAOn == other.PumpAOn
                && this.PumpBOn == other.PumpBOn
                && this.MixValvePosition == other.MixValvePosition
                && this.LineAPressure == other.LineAPressure
                && this.LineBPressure == other.LineBPressure
                && this.LineHeaterOn == other.LineHeaterOn
                && this.RadiatorDeployed == other.RadiatorDeployed
                && this.OutputFluidTemperature == other.OutputFluidTemperature
                && this.SetTemperature == other.SetTemperature;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ExternalCoolantLoopData other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.RadiatorRotation,
                this.PumpAOn,
                this.PumpBOn,
                this.MixValvePosition,
                this.LineAPressure,
                (this.LineBPressure,
                    this.LineHeaterOn,
                    this.RadiatorDeployed,
                    this.OutputFluidTemperature,
                    this.SetTemperature)
                );
        }

        public static bool operator ==(ExternalCoolantLoopData left, ExternalCoolantLoopData right) => Equals(left, right);

        public static bool operator !=(ExternalCoolantLoopData left, ExternalCoolantLoopData right) => !Equals(left, right);

        #endregion Equality Members
    }
}