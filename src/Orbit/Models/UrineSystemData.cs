﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Orbit.Annotations;

namespace Orbit.Models
{
    public class UrineSystemData : IAlertableModel, IEquatable<UrineSystemData>, ISeedableModel
    {
        #region Limits

        public const int urineTankUpperLimit = 100;
        public const int urineTankLevelTolerance = 10;
        
        public const int distillerSpeedUpperLimit = 1300;
        public const int distillerSpeedLowerLimit = 1100;
        public const int distillerSpeedTolerance = 100;
        
        public const int distillerTempUpperLimit = 55;
        public const int distillerTempLowerLimit = 35;
        public const int distillerTempTolerance = 5;
        
        public const int brineTankLevelUpperLimit = 100;
        public const int brineTankLevelTolerance = 5;
        
        public SystemStatus lastWorkingStatus;
        public int largeIncrement = 5;
        public int smallIncrement = 2;
        public int brineIncrement = 1;

        #endregion Limits

        #region Properties

        /// <summary>
        /// The name of the component.
        /// </summary>
        [NotMapped]
        public string ComponentName => "UrineSystem";

        public DateTimeOffset ReportDateTime { get; private set; } = DateTimeOffset.Now;

        /// <summary>
        /// Indicator of overall system status (Ready, Processing, Failure...)
        /// </summary>
        public SystemStatus Status { get; set; }

        ///<summary>
        /// denotes whether the system is operating in automatic or manual capacity
        /// </summary>
        public bool IsManualMode { get; set; }

        /// <summary>
        /// Fullness of treated urine holding tank as a percentage
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public double UrineTankLevel { get; set; }

        /// <summary>
        /// status of pump assembly used to pull fluid from urine tank to the distiller assembly then from distiller
        /// assembly to the brine tank and water processor
        /// </summary>
        public bool SupplyPumpOn { get; set; }

        /// <summary>
        /// Turning distiller on or off also turns on/off distiller motor and heater
        /// </summary>
        public bool DistillerOn { get; set; }

        /// <summary>
        /// Motor speed; nominal 1200 rpm
        /// </summary>
        [Range(0, 1500)]
        [IdealRange(1000, 1400)]
        [IdealValue(1200)]
        public int DistillerSpeed { get; set; }

        /// <summary>
        /// Temp of urine in the distiller; nominal 45C
        /// </summary>
        [Range(0, 60)]
        [IdealRange(35, 50)]
        [IdealValue(45)]
        public double DistillerTemp { get; set; }

        /// <summary>
        /// Routes distillate and gasses from distiller to gas/liquid separator cooled assembly aids condensation of
        /// water from gas
        /// </summary>
        public bool PurgePumpOn { get; set; }

        /// <summary>
        /// Stores concentrated minerals and contaminates from urine distillation process for later disposal shown as percentage
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public double BrineTankLevel { get; set; }

        #endregion Properties

        #region Constructors

        public UrineSystemData() { }

        public UrineSystemData (UrineSystemData other)
        {
            ReportDateTime = DateTimeOffset.Now;
            Status = other.Status;
            UrineTankLevel = other.UrineTankLevel;
            SupplyPumpOn = other.SupplyPumpOn;
            DistillerOn = other.DistillerOn;
            DistillerTemp = other.DistillerTemp;
            DistillerSpeed = other.DistillerSpeed;
            PurgePumpOn = other.PurgePumpOn;
            BrineTankLevel = other.BrineTankLevel;
        }

        #endregion Constructors

        #region Methods

        public void ProcessData(double wasteTankLevel)
        {
            bool trouble = false;
            GenerateData();

            if (IsManualMode)
                return;

            if (Status == SystemStatus.Standby)
            {
                // if urine tank is full and the waste and brine tanks are not, change to 'processing' state
                if (UrineTankLevel >= urineTankUpperLimit - urineTankLevelTolerance
                    && wasteTankLevel < urineTankUpperLimit
                    && BrineTankLevel < brineTankLevelUpperLimit)
                {
                    Status = SystemStatus.Processing;
                    //SimulateProcessing();
                }
                // if urine tank not full, stay in standby and simulate urine tank filling
                else
                {
                    if (SupplyPumpOn || PurgePumpOn || DistillerOn)
                    {
                        lastWorkingStatus = Status;
                        //Status = SystemStatus.Trouble;
                        trouble = true;
                    }

                    SimulateStandby();
                }
            }
            if (Status == SystemStatus.Processing)
            {
                // no more urine to process or waste or brine tank full, change to 'Standby' state
                if(BrineTankLevel >= 100)
                {
                    // simulate emptying full brine tank
                    BrineTankLevel = 0;
                }
                if (UrineTankLevel <= 0
                    || wasteTankLevel >= urineTankUpperLimit
                    || BrineTankLevel >= brineTankLevelUpperLimit)
                {
                    Status = SystemStatus.Standby;
                    //SimulateStandby();

                    UrineTankLevel = Math.Max(UrineTankLevel, 0);
                    BrineTankLevel = Math.Min(BrineTankLevel, brineTankLevelUpperLimit);
                }
                else
                {
                    if(!SupplyPumpOn || !PurgePumpOn || !DistillerOn)
                    {
                        lastWorkingStatus = Status;
                        //Status = SystemStatus.Trouble;
                    }
                    SimulateProcessing();
                }
            }
            //else if(Status == SystemStatus.Trouble)
            //{
            //    if (trouble)
            //    {
            //        Status = SystemStatus.Trouble;
            //        Trouble();
            //    }
            //    else
            //    {
            //        Status = lastWorkingStatus;
            //    }
            //}
        }

        private void SimulateProcessing()
        {
            // urine tank not empty, process 
            if (UrineTankLevel > largeIncrement)
            {
                UrineTankLevel -= largeIncrement;
            }
            // urine tank full, set to full value
            else
            {
                UrineTankLevel = urineTankUpperLimit;
            }

            // brine tank not full, process
            if (BrineTankLevel < brineTankLevelUpperLimit - brineIncrement)
            {
                BrineTankLevel += brineIncrement;
            }
            // brine tank full, set to full value
            else
            {
                BrineTankLevel = brineTankLevelUpperLimit;
            }

            // should be on, randomly trigger a false to simulate a fault
            SupplyPumpOn = RandomFalse();
            DistillerOn = RandomFalse();
            PurgePumpOn = RandomFalse();
        }

        private void SimulateStandby()
        {
            // urine tank  not full, simulate toilet usage
            if(UrineTankLevel < urineTankUpperLimit - smallIncrement)
            {
                UrineTankLevel += smallIncrement;
            }
            // urine tank full, set to ful value
            else
            {
                UrineTankLevel = urineTankUpperLimit;
            }

            // should be off, randomly trigger a true to simulate a fault
            SupplyPumpOn = RandomTrue();
            DistillerOn = RandomTrue();
            PurgePumpOn = RandomTrue();
        }

        private void Trouble()
        {
            if(lastWorkingStatus == SystemStatus.Processing)
            {
                SimulateProcessing();
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
                DistillerTemp = rand.Next(distillerTempLowerLimit, distillerTempUpperLimit);
                DistillerSpeed = rand.Next(distillerSpeedLowerLimit, distillerSpeedUpperLimit);
            }
            else
            {
                DistillerTemp = rand.Next(15, 32);  // something close to ambient air temp
                DistillerSpeed = 0;
            }
        }

        void ISeedableModel.SeedData()
        {
            IsManualMode = false;
            BrineTankLevel = 5;
            DistillerSpeed = 20;
            DistillerTemp = 20;
            DistillerOn = false;
            SupplyPumpOn = false;
            PurgePumpOn = false;
            Status = SystemStatus.Standby;
            UrineTankLevel = 20;
        }
		
		public void SeedData()
        {
            IsManualMode = false;
            BrineTankLevel = 5;
            DistillerSpeed = 20;
            DistillerTemp = 20;
            DistillerOn = false;
            SupplyPumpOn = false;
            PurgePumpOn = false;
            Status = SystemStatus.Standby;
            UrineTankLevel = 20;
        }

        private bool RandomTrue()
        {
            Random rand = new Random();

            if (rand.Next(1, 10) == 3)
            {
                return true;
            }
            else { return false; }
        }

        private bool RandomFalse()
        {
            Random rand = new Random();

            if (rand.Next(1, 10) == 3)
            {
                return false;
            }
            else { return true; }
        }

        public void ChangeCrewedStatus()
        {
            if(UrineTankLevel > 0)
            {
                Status = SystemStatus.Processing;
            }
        }

        #endregion Methods

        #region Alert Generation

        private IEnumerable<Alert> CheckUrineTankLevel()
        {
            // system should start/be processing if wastetank < 95%, else shut down
            if (UrineTankLevel >= urineTankUpperLimit)
            {
                yield return this.CreateAlert(a => a.UrineTankLevel, "Urine tank level is at capacity", AlertLevel.HighError);
            }
            else if (UrineTankLevel >= (urineTankUpperLimit - urineTankLevelTolerance))
            {
                yield return this.CreateAlert(a => a.UrineTankLevel, "Urine tank level is nearing capacity", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.UrineTankLevel);
            }
        }

        private IEnumerable<Alert> CheckDistillerSpeed()
        {
            if (DistillerSpeed >= distillerSpeedUpperLimit)
            {
                //TODO: shut down system
                yield return this.CreateAlert(a => a.DistillerSpeed, "Distiller speed above maximum", AlertLevel.HighError);
            }
            else if (DistillerSpeed > (distillerSpeedUpperLimit - distillerSpeedTolerance))
            {
                yield return this.CreateAlert(a => a.DistillerSpeed, "Distiller speed is too high", AlertLevel.HighWarning);
            }
            else if (DistillerOn && (DistillerSpeed < distillerSpeedLowerLimit))
            {
                yield return this.CreateAlert(a => a.DistillerSpeed, "Distiller speed is below minimum", AlertLevel.LowError);
            }
            else if (DistillerOn && (DistillerSpeed <= (distillerSpeedLowerLimit + distillerSpeedTolerance)))
            {
                // TODO: shut down system
                yield return this.CreateAlert(a => a.DistillerSpeed, "Distiller speed is too low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.DistillerSpeed);
            }
        }

        private IEnumerable<Alert> CheckDistillerTemp()
        {
            if (DistillerTemp >= distillerTempUpperLimit)
            {
                yield return this.CreateAlert(a => a.DistillerTemp, "Distiller temp above maximum", AlertLevel.HighError);
            }
            else if (DistillerTemp > (distillerTempUpperLimit - distillerTempTolerance))
            {
                yield return this.CreateAlert(a => a.DistillerTemp, "Distiller temp is too high", AlertLevel.HighWarning);
            }
            else if (DistillerOn && (DistillerTemp < distillerTempLowerLimit))
            {
                yield return this.CreateAlert(a => a.DistillerTemp, "Distiller temp is below minimum", AlertLevel.LowError);
            }
            else if (DistillerOn && (DistillerTemp <= (distillerTempLowerLimit + distillerTempTolerance)))
            {
                yield return this.CreateAlert(a => a.DistillerTemp, "Distiller Temp is too low", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.DistillerTemp);
            }
        }

        private IEnumerable<Alert> CheckBrineTankLevel()
        {
            if (BrineTankLevel >= brineTankLevelUpperLimit)
            {
                // TODO: shut down system
                yield return this.CreateAlert(a => a.BrineTankLevel, "Brine tank is at capacity", AlertLevel.HighError);
            }
            else if (BrineTankLevel >= (brineTankLevelUpperLimit - brineTankLevelTolerance))
            {
                yield return this.CreateAlert(a => a.BrineTankLevel, "Brine tank is nearing capacity", AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.BrineTankLevel);
            }
        }

        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return this.CheckUrineTankLevel()
                .Concat(this.CheckDistillerTemp())
                .Concat(this.CheckDistillerSpeed())
                .Concat(this.CheckBrineTankLevel());
        }

        public IEnumerable<Alert> GetAlerts()
        {
            return this.CheckUrineTankLevel()
                .Concat(this.CheckDistillerTemp())
                .Concat(this.CheckDistillerSpeed())
                .Concat(this.CheckBrineTankLevel());
        }

        #endregion Alert Generation

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other"> An object to compare with this object. </param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(UrineSystemData? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return this.ReportDateTime.Equals(other.ReportDateTime)
                   && this.Status == other.Status
                   && this.UrineTankLevel.Equals(other.UrineTankLevel)
                   && this.SupplyPumpOn == other.SupplyPumpOn
                   && this.DistillerOn == other.DistillerOn
                   && this.DistillerSpeed == other.DistillerSpeed
                   && this.DistillerTemp.Equals(other.DistillerTemp)
                   && this.PurgePumpOn == other.PurgePumpOn
                   && this.BrineTankLevel.Equals(other.BrineTankLevel);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return this.Equals(obj as UrineSystemData);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.UrineTankLevel,
                this.SupplyPumpOn,
                this.DistillerOn,
                this.DistillerSpeed,

                // Have to use tuple bc for some reason the method is capped at 8 args
                (this.DistillerTemp, this.BrineTankLevel)
            );
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Orbit.Models.UrineSystemData"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left"> The first value to compare. </param>
        /// <param name="right"> The second value to compare. </param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(UrineSystemData left, UrineSystemData right) => Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Orbit.Models.UrineSystemData"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left"> The first value to compare. </param>
        /// <param name="right"> The second value to compare. </param>
        /// <returns> true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false. </returns>
        public static bool operator !=(UrineSystemData left, UrineSystemData right) => !Equals(left, right);

        #endregion Equality members

    }
}