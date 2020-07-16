using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Orbit.Annotations;

namespace Orbit.Models
{
    public class InternalCoolantLoopData : IAlertableModel, IEquatable<InternalCoolantLoopData>, ISeedableModel
    {
        #region Limits

        public int mixValveMaxOpen = 100;
        public int mixValveMaxClosed = 0;
        public int mixValveTolerance = 5;

        public const double medCoolantLoopUpperLimit = 22;
        public const double medCoolantLoopLowerLimit = 12;
        public const double medCoolantLoopTolerance = 5;

        public const double lowTempCoolantLoopUpperLimit = 10;
        public const double lowTempCoolantLoopLowerLimit = 2;
        public const double lowTempCoolantLoopTolerance = 2;

        #endregion Limits

        #region Public Properties

        [NotMapped]
        public string ComponentName => "InternalCoolantSystem";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// overall system status
        /// </summary>
        public SystemStatus Status { get; set; }

        /// <summary>
        /// denotes whether system is operating manually or automatically
        /// </summary>
        public Modes Mode { get; set; }

        ///<summary>
        /// denotes whether the system is operating in automatic or manual capacity
        /// </summary>
        public bool IsManualMode { get; set; }

        /// <summary>
        /// true if pump is working, false if not working
        /// </summary>
        public bool LowTempPumpOn { get; set; }

        /// <summary>
        /// true if pump is working, false if not working
        /// </summary>
        public bool MedTempPumpOn { get; set; }

        /// <summary>
        /// determines mix of 'hot' coolant returning from equipment and 'cold' coolant coming from heat exchanger
        /// 0 = bypass heat exhanger, 100 = no fluid bypasses heat exchanger
        /// </summary>
        [Range(0, 100)]
        public int LowTempMixValvePosition { get; set; }

        /// <summary>
        /// determines mix of 'hot' coolant returning from equipment and 'cold' coolant coming from heat exchanger
        /// 0 = bypass heat exhanger, 100 = no fluid bypasses heat exchanger
        /// </summary>
        [Range(0, 100)]
        public int MedTempMixValvePosition { get; set; }

        /// <summary>
        /// determines mix of 'hot' coolant returning from equipment and 'cold' coolant coming from heat exchanger
        /// 0 = bypass heat exhanger, 100 = no fluid bypasses heat exchanger
        /// </summary>
        [Range(0, 100)]
        public int CrossoverMixValvePosition { get; set; }

        /// <summary>
        /// true when med and low loops are operating as a single loop and low temp pump is on
        /// </summary>
        public bool LowTempSingleLoop { get; set; }

        /// <summary>
        /// true when med and low loops are operating as a single loop and med temp pump is on
        /// </summary>
        public bool MedTempSingleLoop { get; set; }

        /// <summary>
        /// warmer temperature coolant loop for experiments and avionics 
        /// nominal is 10.5C
        /// </summary>
        [Range(0, 35)]
        [IdealRange(10, 27)]
        [IdealValue(10.5)]
        public double TempMedLoop { get; set; }

        /// <summary>
        /// colder temperature coolant loop for life support, cabin air assembly, and some experiments 
        /// nominal is 4C
        /// </summary>
        [Range(0, 20)]
        [IdealRange(2, 10)]
        [IdealValue(4)]
        public double TempLowLoop { get; set; }

        /// <summary>
        /// desired temperature for the medium temperature loop
        /// </summary>
        [Range(0, 35)]
        [IdealRange(10, 27)]
        public double SetTempMedLoop { get; set; }

        /// <summary>
        /// desired temperature for the low temperature loop
        /// </summary>
        [Range(0, 20)]
        [IdealRange(2, 10)]
        public double SetTempLowLoop { get; set; }

        #endregion Public Properties

        #region Constructors

        public InternalCoolantLoopData() { }

        public InternalCoolantLoopData(InternalCoolantLoopData other)
        {
            Status = other.Status;
            Mode = other.Mode;
            LowTempPumpOn = other.LowTempPumpOn;
            MedTempPumpOn = other.MedTempPumpOn;
            LowTempMixValvePosition = other.LowTempMixValvePosition;
            MedTempMixValvePosition = other.MedTempMixValvePosition;
            CrossoverMixValvePosition = other.CrossoverMixValvePosition;
            LowTempSingleLoop = other.LowTempSingleLoop;
            MedTempSingleLoop = other.MedTempSingleLoop;
            TempLowLoop = other.TempLowLoop;
            TempMedLoop = other.TempMedLoop;
            SetTempLowLoop = other.SetTempLowLoop;
            SetTempMedLoop = other.SetTempMedLoop;

            GenerateData();
        }

        #endregion Constructors

        #region Methods

        public void SeedData()
        {
            Status = SystemStatus.On;
            Mode = Modes.Uncrewed;
            LowTempPumpOn = true;
            MedTempPumpOn = true;
            LowTempMixValvePosition = 15;
            MedTempMixValvePosition = 32;
            CrossoverMixValvePosition = 0;
            LowTempSingleLoop = false;
            MedTempSingleLoop = false;
            TempLowLoop = 4;
            TempMedLoop = 10.5;
            SetTempLowLoop = 4;
            SetTempMedLoop = 10.5;
        }

        public void ProcessData()
        {
            GenerateData();

            if (IsManualMode)
                return;

            // both pumps on, dual loop operation
            if (LowTempPumpOn && MedTempPumpOn)
            {
                CrossoverMixValvePosition = 0;

                if (TempLowLoop < SetTempLowLoop)
                {
                    DecreaseMix("low");
                }
                if (TempLowLoop > (SetTempLowLoop + 7))
                {
                    IncreaseMix("low");
                }

                if (TempMedLoop < SetTempMedLoop)
                {
                    DecreaseMix("med");
                }
                if (TempMedLoop > SetTempMedLoop)
                {
                    IncreaseMix("med");
                }
            }
            //  manually triggered single loop operation with failure of corresponding pump, switch loops then process
            else
            {
                // running on med loop only and med loop pump has failed, switch loop pumps
                if (MedTempSingleLoop && !MedTempPumpOn)
                {
                    Trouble();
                    MedTempSingleLoop = false;
                    LowTempSingleLoop = true;
                    LowTempPumpOn = true;
                }
                // running on low temp loop only and low loop pump has failed, switch loop pumps
                else if (LowTempSingleLoop && !LowTempPumpOn)
                {
                    Trouble();
                    LowTempSingleLoop = false;
                    MedTempSingleLoop = true;
                    MedTempPumpOn = true;
                }
                // Single loop operation not manually selected
                else if(!LowTempSingleLoop && !MedTempSingleLoop)
                {
                    Trouble();

                    // give the crossover mix valve a starting 'open' position
                    if (CrossoverMixValvePosition == 0)
                    {
                        CrossoverMixValvePosition = 40;
                    }

                }
                // dual pump failure
                else if (!LowTempPumpOn && !MedTempPumpOn)
                {
                    Trouble();
                }


                if ((TempLowLoop < SetTempLowLoop) || (TempMedLoop < SetTempMedLoop))
                {
                    // coolant too cool, decrease amount of 'cold' coolant from heat exchanger
                    DecreaseCombinedValve();
                }

                else if ((TempLowLoop > SetTempLowLoop) || (TempMedLoop > SetTempMedLoop))
                {
                    // coolant too warm, add more 'cold' coolant from heat exchanger
                    IncreaseCombinedValve();
                }
            }
        }

        private void Trouble()
        {
            Status = SystemStatus.Trouble;
        }

        private void GenerateData()
        {
            Random rand = new Random();

            TempLowLoop = rand.Next(0, 150) / 10.0;
            TempMedLoop = rand.Next(0, 350) / 10.0;

            if (rand.Next(0, 10) == 5)
            {
                LowTempPumpOn = false;
            }
            else
            {
                LowTempPumpOn = true;
            }

            if (rand.Next(0, 10) == 9)
            {
                MedTempPumpOn = false;
            }
            else
            {
                MedTempPumpOn = true;
            }
            
        }

        private void IncreaseMix(string loop)
        {
            if (loop.Equals("low"))
            {
                if (LowTempMixValvePosition < mixValveMaxOpen)
                {
                    LowTempMixValvePosition++;
                }
                else
                {
                    LowTempMixValvePosition = mixValveMaxOpen;
                }
            }
            else if (loop.Equals("med"))
            {
                if (MedTempMixValvePosition < mixValveMaxOpen)
                {
                    MedTempMixValvePosition++;
                }
                else
                {
                    MedTempMixValvePosition = mixValveMaxOpen;
                }
            }
            else { }
        }

        private void IncreaseCombinedValve()
        {
            if(CrossoverMixValvePosition < mixValveMaxOpen)
            {
                CrossoverMixValvePosition++;
            }
            else
            {
                CrossoverMixValvePosition = mixValveMaxOpen;
            }
        }

        private void DecreaseMix(string loop)
        {
            if (loop.Equals("low"))
            {
                if (LowTempMixValvePosition > mixValveMaxClosed)
                {
                    LowTempMixValvePosition--;
                }
                else
                {
                    LowTempMixValvePosition = mixValveMaxClosed;
                }
            }
            else if (loop.Equals("med"))
            {
                if (MedTempMixValvePosition > mixValveMaxClosed)
                {
                    MedTempMixValvePosition--;
                }
                else
                {
                    MedTempMixValvePosition = mixValveMaxClosed;
                }
            }
            else { }
        }

        private void DecreaseCombinedValve()
        {
            if (CrossoverMixValvePosition > mixValveMaxClosed)
            {
                CrossoverMixValvePosition--;
            }
            else
            {
                CrossoverMixValvePosition = mixValveMaxClosed;
            }
        }


        #endregion Methods

        #region Alert Generation

        private IEnumerable<Alert> CheckLowLoopTemp()
        {
            if (TempLowLoop >= lowTempCoolantLoopUpperLimit)
            {
                yield return this.CreateAlert(a => a.TempLowLoop, "Low loop temperature is above maximum", AlertLevel.HighError);
            }
            else if (TempLowLoop >= (lowTempCoolantLoopUpperLimit - lowTempCoolantLoopTolerance))
            {
                yield return this.CreateAlert(a => a.TempLowLoop, "Low loop temperature is high", AlertLevel.HighWarning);
            }
            else if (TempLowLoop <= lowTempCoolantLoopLowerLimit)
            {
                yield return this.CreateAlert(a => a.TempLowLoop, "Low loop temperature is low", AlertLevel.LowError);
            }
            else if (TempLowLoop <= (lowTempCoolantLoopLowerLimit + lowTempCoolantLoopTolerance))
            {
                yield return this.CreateAlert(a => a.TempLowLoop, "Low loop temperature is below minimum", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a=>TempLowLoop);
            }
        }

        private IEnumerable<Alert> CheckMedLoopTemp()
        {
            if (TempMedLoop >= medCoolantLoopUpperLimit)
            {
                yield return this.CreateAlert(a => a.TempMedLoop, "Med loop temperature is above maximum", AlertLevel.HighError);
            }
            else if (TempMedLoop >= (medCoolantLoopUpperLimit - medCoolantLoopTolerance))
            {
                yield return this.CreateAlert(a => a.TempMedLoop, "Med loop temperature is high", AlertLevel.HighWarning);
            }
            else if (TempMedLoop <= medCoolantLoopLowerLimit)
            {
                yield return this.CreateAlert(a => a.TempMedLoop, "Med loop temperature is low", AlertLevel.LowError);
            }
            else if (TempMedLoop <= (medCoolantLoopLowerLimit + medCoolantLoopTolerance))
            {
                yield return this.CreateAlert(a => a.TempMedLoop, "Med loop temperature is below minimum", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.TempMedLoop);
            }
        }

        private IEnumerable<Alert> CheckLowTempMixValve()
        {
            if(LowTempMixValvePosition >= mixValveMaxOpen)
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition, "Low temp mixing valve is fully open", AlertLevel.HighError);
            }
            if(LowTempMixValvePosition > (mixValveMaxOpen - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition, "Low temp mixing valve is almost fully open", AlertLevel.HighWarning);
            }
            if(LowTempMixValvePosition <= mixValveMaxClosed)
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition, "Low temp mixing valve is fully closed", AlertLevel.LowError);
            }
            if(LowTempMixValvePosition < (mixValveMaxClosed - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition, "Low temp mixing valve is almost fully closed", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition);
            }
        }

        private IEnumerable<Alert> CheckMedTempMixValve()
        {
            if (MedTempMixValvePosition >= mixValveMaxOpen)
            {
                yield return this.CreateAlert(a => a.MedTempMixValvePosition, "Med temp mixing valve is fully open", AlertLevel.HighError);
            }
            if (MedTempMixValvePosition > (mixValveMaxOpen - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.MedTempMixValvePosition, "Med temp mixing valve is almost fully open", AlertLevel.HighWarning);
            }
            if (MedTempMixValvePosition <= mixValveMaxClosed)
            {
                yield return this.CreateAlert(a => a.LowTempMixValvePosition, "Med temp mixing valve is fully closed", AlertLevel.LowError);
            }
            if (MedTempMixValvePosition < (mixValveMaxClosed - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.MedTempMixValvePosition, "Med temp mixing valve is almost fully closed", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.MedTempMixValvePosition);
            }
        }

        private IEnumerable<Alert> CheckCrossoverMixValve()
        {
            if (CrossoverMixValvePosition >= mixValveMaxOpen)
            {
                yield return this.CreateAlert(a => a.CrossoverMixValvePosition, "Crossover mixing valve is fully open", AlertLevel.HighError);
            }
            if (CrossoverMixValvePosition > (mixValveMaxOpen - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.CrossoverMixValvePosition, "Crossover mixing valve is almost fully open", AlertLevel.HighWarning);
            }
            if (CrossoverMixValvePosition <= mixValveMaxClosed)
            {
                yield return this.CreateAlert(a => a.CrossoverMixValvePosition, "Crossover mixing valve is fully closed", AlertLevel.LowError);
            }
            if (CrossoverMixValvePosition < (mixValveMaxClosed - mixValveTolerance))
            {
                yield return this.CreateAlert(a => a.CrossoverMixValvePosition, "Crossover mixing valve is almost fully closed", AlertLevel.LowWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.CrossoverMixValvePosition);
            }
        }

        private IEnumerable<Alert> CheckLowTempPump()
        {
            if (!LowTempPumpOn)
            {
                yield return this.CreateAlert(a => a.LowTempPumpOn, "Low temperature pump is off", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.LowTempPumpOn);
            }
        }

        private IEnumerable<Alert> CheckMedTempPump()
        {
            if (!MedTempPumpOn)
            {
                yield return this.CreateAlert(a => a.MedTempPumpOn, "Med temperature pump is off", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.MedTempPumpOn);
            }
        }


        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return this.CheckLowLoopTemp()
                .Concat(CheckMedLoopTemp())
                .Concat(CheckLowTempMixValve())
                .Concat(CheckMedTempMixValve())
                .Concat(CheckCrossoverMixValve())
                .Concat(CheckLowTempPump())
                .Concat(CheckMedTempPump());
        }

        #endregion Alert Generation

        #region Equality Members

        public bool Equals(InternalCoolantLoopData other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;
            return this.ReportDateTime == other.ReportDateTime
                && this.Status == other.Status
                && this.Mode == other.Mode
                && this.LowTempPumpOn == other.LowTempPumpOn
                && this.MedTempPumpOn == other.MedTempPumpOn
                && this.LowTempMixValvePosition == other.LowTempMixValvePosition
                && this.MedTempMixValvePosition == other.MedTempMixValvePosition
                && this.LowTempSingleLoop == other.LowTempSingleLoop
                && this.MedTempSingleLoop == other.MedTempSingleLoop
                && this.CrossoverMixValvePosition == other.CrossoverMixValvePosition
                && this.TempMedLoop == other.TempMedLoop
                && this.TempLowLoop == other.TempLowLoop
                && this.SetTempMedLoop == other.SetTempMedLoop
                && this.SetTempLowLoop == other.SetTempLowLoop;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is InternalCoolantLoopData other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.Mode,
                this.LowTempPumpOn,
                this.MedTempPumpOn,
                this.LowTempMixValvePosition,
                this.MedTempMixValvePosition,
                
                (this.LowTempSingleLoop, this.MedTempSingleLoop, this.CrossoverMixValvePosition, this.TempLowLoop, this.TempMedLoop, this.SetTempLowLoop, this.SetTempMedLoop)
            );
        }

        public static bool operator ==(InternalCoolantLoopData left, InternalCoolantLoopData right) => Equals(left, right);

        public static bool operator !=(InternalCoolantLoopData left, InternalCoolantLoopData right) => !Equals(left, right);

        #endregion Equality Members
    }
}