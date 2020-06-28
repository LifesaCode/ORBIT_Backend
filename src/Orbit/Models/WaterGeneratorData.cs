using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Orbit.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Orbit.Models
{
    public class WaterGeneratorData : IAlertableModel, IEquatable<WaterGeneratorData>, ISeedableModel
    {
        #region Limits

        private int seperatorSpeedUpperLimit = 2400;
        private int seperatorSpeedLowerLimit = 1000;
        private int seperatorSpeedTolerance = 100;
        private int seperatorSpeedStandbyMax = 100;

        private int reactorTempUpperLimit = 650;
        private int reactorTempLowerLimit = 450;
        private int reactorTempTolerance = 25;
        private int reactorTempStandbyMax = 100;

        private int storeFull = 100;
        private int storeEmpty = 5;
        private int storeTolerance = 10;
        private int storeReadyToProcess = 75;

        private int MethaneFillValue = 5;
        private int Co2FillValue = 3;
        private int H2FillValue = 3;

        private SystemStatus lastStatus;

        #endregion Limits

        #region Public Properties

        [NotMapped]
        public string ComponentName => "Water Generator";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        public SystemStatus Status { get; set; }

        /// <summary>
        /// seperates water from methane gas, false when SeperatorMotorSpeed = 0
        /// </summary>
        public bool SeperatorOn { get; set; }

        /// <summary>
        /// current operating speed of the motor in RPM
        /// </summary>
        [Range(0, 2500)]
        [IdealRange(1000, 2400)]
        [IdealValue(2000)]
        public int SeperatorMotorSpeed { get; set; }

        /// <summary>
        /// set max operating speed of the motor in RPM
        /// </summary>
        public int SeperatorMotorSetSpeed { get; set; }  

        /// <summary>
        /// used to start reaction when reactor is cold. Reaction is exothermic so heater is
        /// not required to be on throughout the reaction
        /// </summary>
        public bool HeaterOn { get; set; }

        /// <summary>
        /// highest temperature in the reactor
        /// </summary>
        [Range(0, 700)]
        [IdealRange(450, 650)]
        [IdealValue(500)]
        public int ReactorTemp { get; set; }

        /// <summary>
        /// desired reaction temperature, reactor temperature can be reduced by cooling system
        /// </summary>
        public int ReactorSetTemp { get; set; }

        /// <summary>
        /// moves combined water and methane product to seperator
        /// </summary>
        public bool PumpOn { get; set; }

        /// <summary>
        /// amount of methane in holding tank, vents to space when full
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public int MethaneStoreLevel { get; set; }

        /// <summary>
        /// simulate storage of H2 for reaction
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public int H2StoreLevel { get; set; }

        /// <summary>
        /// simulate storage of Co2 for reaction
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public int Co2StoreLevel { get; set; }

        #endregion Public Properties

        #region Constructors

        public WaterGeneratorData() { }

        public WaterGeneratorData(WaterGeneratorData other)
        {
            ReportDateTime = DateTimeOffset.Now;
            Status = other.Status;
            SeperatorOn = other.SeperatorOn;
            SeperatorMotorSpeed = other.SeperatorMotorSpeed;
            SeperatorMotorSetSpeed = other.SeperatorMotorSetSpeed;
            HeaterOn = other.HeaterOn;
            ReactorTemp = other.ReactorTemp;
            ReactorSetTemp = other.ReactorSetTemp;
            PumpOn = other.PumpOn;
            MethaneStoreLevel = other.MethaneStoreLevel;
            H2StoreLevel = other.H2StoreLevel;
            Co2StoreLevel = other.Co2StoreLevel;

            GenerateData();
        }

        #endregion Constructors

        #region Methods

        public void SeedData()
        {
            ReportDateTime = DateTimeOffset.Now;
            Status = SystemStatus.Standby;
            SeperatorOn = false;
            SeperatorMotorSpeed = 0;
            SeperatorMotorSetSpeed = 2000;
            HeaterOn = false;
            ReactorTemp = 18;
            ReactorSetTemp = 500;
            PumpOn = false;
            MethaneStoreLevel = 30;
            H2StoreLevel = 20;
            Co2StoreLevel = 25;
        }

        public void ProcessData()
        {
            GenerateData();

            if(Status == SystemStatus.Processing)
            {
                SimulateProcessing();

                // if stores are almost empty, change to standby state
                if ((H2StoreLevel <= storeEmpty) || (Co2StoreLevel <= storeEmpty))
                {
                    Status = SystemStatus.Standby;
                    lastStatus = Status;
                    TurnOff();
                }
            }
            else if(Status == SystemStatus.Standby)
            {
                SimulateStandby();

                // if stores are getting full, change to processing state
                if ((H2StoreLevel > storeReadyToProcess) || (Co2StoreLevel > storeReadyToProcess))
                {
                    Status = SystemStatus.Processing;
                    lastStatus = Status;
                    TurnOn();
                }
            }
            else 
            {
                SimulateStandby();
            }
        }

        private void GenerateData()
        {
            Random rand = new Random();

            if(Status == SystemStatus.Processing)
            {
                SeperatorMotorSpeed = rand.Next(900, 2500);
                ReactorTemp = rand.Next(399, 700);
            }
            else
            {
                SeperatorMotorSpeed = 0;
                ReactorTemp = rand.Next(16, reactorTempStandbyMax);
            }
        }

        private void SimulateProcessing()
        {
            // check for trouble states
            if ((ReactorTemp < reactorTempLowerLimit)
                || (ReactorTemp > reactorTempUpperLimit)
                || (!SeperatorOn)
                || (!PumpOn)
                || (MethaneStoreLevel >= storeFull)
                || (!HeaterOn && (ReactorTemp < reactorTempLowerLimit))
                || (HeaterOn && (ReactorTemp > reactorTempUpperLimit))
                || (SeperatorMotorSpeed > seperatorSpeedUpperLimit)
                || (SeperatorMotorSpeed < seperatorSpeedLowerLimit))
            {
                //Trouble();
            }

            // simulate removal of reactant gasses to reaction  
            // TODO: fix subtraction rates to be more in line with reaction consumption ratio
            if (H2StoreLevel >= (storeEmpty + H2FillValue))
            {              
                H2StoreLevel -= H2FillValue;
            }
            else
            {
                H2StoreLevel = 0;
            }
            
            if (Co2StoreLevel >= (storeEmpty + Co2FillValue))
            {
                Co2StoreLevel -= Co2FillValue;
            }
            else
            {
                Co2StoreLevel = 0;
            }

            // simulate methane fill and venting
            if (MethaneStoreLevel < (storeFull - storeTolerance))
            {
                MethaneStoreLevel += MethaneFillValue;
            }
            else
            {
                MethaneStoreLevel = 0;
            }

            if(MethaneStoreLevel >= 100)
            {
                // simulate emptying of methane tank
                MethaneStoreLevel = 0;
            }
        }

        private void SimulateStandby()
        {
            // check for trouble states
            if ((ReactorTemp > reactorTempStandbyMax)
                || SeperatorOn
                || PumpOn
                || HeaterOn
                || (MethaneStoreLevel > (storeFull - storeTolerance)))
            {
                //Trouble();
            }

            // simulate addition of reactant gasses to stores  
            // TODO: fix addition rates to be more in line with oxygen generation production ratio
            if (H2StoreLevel < storeFull)
            {
                H2StoreLevel += H2FillValue;
            }
            else
            {
                H2StoreLevel = storeFull;
            }
            if (Co2StoreLevel < storeFull)
            {
                Co2StoreLevel += Co2FillValue;
            }
            else
            {
                Co2StoreLevel = storeFull;
            }
        }

        private void Trouble()
        {
            Status = SystemStatus.Trouble;

            // stop system so reactants don't build up or damage components from 'dry' operation
            TurnOff();
        }

        private void TurnOff()
        {
            SeperatorOn = false;
            PumpOn = false;
            HeaterOn = false;
        }

        private void TurnOn()
        {
            // if reaction is just being started, heater will need to 'preheat' the reactor
            if (ReactorTemp < reactorTempLowerLimit)
            {
                HeaterOn = true;
            }
            else
            {
                // heater not needed once reaction is in progress
                HeaterOn = false;
            }

            SeperatorOn = true;
            PumpOn = true;
        }

        #endregion Methods

        #region Check Alerts

        private IEnumerable<Alert> CheckSeperatorOn()
        {
            if(Status == SystemStatus.Processing)
            {
                if (SeperatorOn)
                {
                    yield return this.CreateAlert(a => a.SeperatorOn);
                }
                else
                {
                    yield return this.CreateAlert(a => a.SeperatorOn, "Seperator off while system processing", AlertLevel.HighWarning);
                }
            }
            else
            {
                if (SeperatorOn)
                {
                    yield return this.CreateAlert(a => a.SeperatorOn, "Seperator on while system not processing", AlertLevel.HighError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.SeperatorOn);
                }
            }
        }

        private IEnumerable<Alert> CheckSeperatorSpeed()
        {
            if(Status == SystemStatus.Processing)
            {
                if(SeperatorMotorSpeed > (seperatorSpeedUpperLimit - seperatorSpeedTolerance))
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed, "Seperator speed is above maximum", AlertLevel.HighError);
                }
                else if(SeperatorMotorSpeed > (SeperatorMotorSetSpeed + seperatorSpeedTolerance))
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed, "Seperator speed is above set speed", AlertLevel.HighWarning);
                }
                else if(SeperatorMotorSpeed < (seperatorSpeedLowerLimit + seperatorSpeedTolerance))
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed, "Seperator speed is below minimum", AlertLevel.LowError);
                }
                else if (SeperatorMotorSpeed < (SeperatorMotorSetSpeed - seperatorSpeedTolerance))
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed, "Seperator speed is below set speed", AlertLevel.LowWarning);
                }
                else
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed);
                }
            }
            else  
            {
                if(SeperatorMotorSpeed > (seperatorSpeedLowerLimit - seperatorSpeedTolerance))
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed, "Seperator on while system in standby", AlertLevel.HighError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.SeperatorMotorSpeed);
                }
            }
        }

        private IEnumerable<Alert> CheckHeaterOn()
        {
            if(Status == SystemStatus.Processing)
            {
                if(!HeaterOn && (ReactorTemp < reactorTempLowerLimit))
                {
                    yield return this.CreateAlert(a => a.HeaterOn, "Heater off while reactor temp too low for processing", AlertLevel.LowError);
                }
                else if(HeaterOn && (ReactorTemp > reactorTempLowerLimit))
                {
                    yield return this.CreateAlert(a => a.HeaterOn, "Heater on while reactor above minimum temperature", AlertLevel.HighError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.HeaterOn);
                }
            }
            else  
            {
                if (HeaterOn)
                {
                    yield return this.CreateAlert(a => a.HeaterOn, "Heater on while system not processing", AlertLevel.HighError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.HeaterOn);
                }
            }
        }

        private IEnumerable<Alert> CheckReactorTemp()
        {  
            if (Status == SystemStatus.Processing)
            {
                if (ReactorTemp > reactorTempUpperLimit)
                {
                    yield return this.CreateAlert(a => a.ReactorTemp, "Reactor temperature is above maximum", AlertLevel.HighError);
                }
                else if (ReactorTemp > (ReactorSetTemp + reactorTempTolerance))
                {
                    yield return this.CreateAlert(a => a.ReactorTemp, "Reactor temperature is above set temperature", AlertLevel.HighWarning);
                }
                else if (ReactorTemp < reactorTempLowerLimit)
                {
                    yield return this.CreateAlert(a => a.ReactorTemp, "Reactor temperature is below  minumum", AlertLevel.LowError);
                }
                else if (ReactorTemp < (ReactorSetTemp - reactorTempTolerance))
                {
                    yield return this.CreateAlert(a => a.ReactorTemp, "Reactor temperature is below set temperture", AlertLevel.LowWarning);
                }
                else
                {
                    yield return this.CreateAlert(a => a.ReactorTemp);
                }
            }
            else 
            {
                if(ReactorTemp > (reactorTempLowerLimit - reactorTempTolerance))
                {
                    yield return this.CreateAlert(a => a.ReactorTemp, "Reactor temperature elevated", AlertLevel.HighError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.ReactorTemp);
                }
            }

        }    

        private IEnumerable<Alert> CheckPumpOn()
        {
            if(Status == SystemStatus.Processing)
            {
                if (PumpOn)
                {
                    yield return this.CreateAlert(a => a.PumpOn);
                }
                else
                {
                    yield return this.CreateAlert(a => a.PumpOn, "Pump off while processing", AlertLevel.LowError);
                }
            }
            else
            {
                if (PumpOn)
                {
                    yield return this.CreateAlert(a => a.PumpOn, "Pump on while system off", AlertLevel.LowError);
                }
                else
                {
                    yield return this.CreateAlert(a => a.PumpOn);
                }
            }
        }

        private IEnumerable<Alert> CheckMethaneStoreLevel()
        {
            if(MethaneStoreLevel >= storeFull)
            {
                yield return this.CreateAlert(a => a.MethaneStoreLevel, "Methane store is at capacity", AlertLevel.HighError);
            }
            else if(MethaneStoreLevel > (storeFull - storeTolerance))
            {
                yield return this.CreateAlert(a => a.MethaneStoreLevel, "Methane store is nearing capacity", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.MethaneStoreLevel);
            }
        }

        private IEnumerable<Alert> CheckCo2StoreLevel()
        {
            if(Co2StoreLevel >= storeFull)
            {
                yield return this.CreateAlert(a => a.Co2StoreLevel, "CO2 store is at capacity", AlertLevel.HighError);
            }
            else if(Co2StoreLevel > (storeFull - storeTolerance))
            {
                yield return this.CreateAlert(a => a.Co2StoreLevel, "Co2 store is nearing capacity", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.Co2StoreLevel);
            }
        }

        private IEnumerable<Alert> CheckH2StoreLevel()
        {
            if(H2StoreLevel >= storeFull)
            {
                yield return this.CreateAlert(a => a.H2StoreLevel, "Hydrogen store is at capacity", AlertLevel.HighError);
            }
            else if (H2StoreLevel > (storeFull - storeTolerance))
            {
                yield return this.CreateAlert(a => a.H2StoreLevel, "Hydrogen store is nearing capacity", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.H2StoreLevel);
            }

        }

        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return this.CheckSeperatorOn()
                .Concat(CheckSeperatorSpeed())
                .Concat(CheckHeaterOn())
                .Concat(CheckReactorTemp())
                .Concat(CheckPumpOn())
                .Concat(CheckMethaneStoreLevel())
                .Concat(CheckCo2StoreLevel())
                .Concat(CheckH2StoreLevel());
        }


        #endregion Check Alerts


        #region Equality Members

        public bool Equals(WaterGeneratorData other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return this.ReportDateTime == other.ReportDateTime
                && this.Status == other.Status
                && this.SeperatorOn == other.SeperatorOn
                && this.SeperatorMotorSpeed == other.SeperatorMotorSpeed
                && this.SeperatorMotorSetSpeed == other.SeperatorMotorSetSpeed
                && this.HeaterOn == other.HeaterOn
                && this.ReactorTemp == other.ReactorTemp
                && this.ReactorSetTemp == other.ReactorSetTemp
                && this.PumpOn == other.PumpOn
                && this.MethaneStoreLevel == other.MethaneStoreLevel
                && this.Co2StoreLevel == other.Co2StoreLevel
                && this.H2StoreLevel == other.H2StoreLevel;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is WaterGeneratorData other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.SeperatorOn,
                this.SeperatorMotorSpeed,
                this.SeperatorMotorSetSpeed,
                this.HeaterOn,
                this.ReactorTemp,

                (this.ReactorSetTemp, this.PumpOn, this.MethaneStoreLevel, this.Co2StoreLevel, this.H2StoreLevel)
                
            );
        }

        public static bool operator ==(WaterGeneratorData left, WaterGeneratorData right) => Equals(left, right);

        public static bool operator !=(WaterGeneratorData left, WaterGeneratorData right) => !Equals(left, right);

        #endregion Equality Members
    }
}
