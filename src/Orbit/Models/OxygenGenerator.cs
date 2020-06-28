using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Orbit.Annotations;

namespace Orbit.Models
{
    public class OxygenGenerator : IAlertableModel, IEquatable<OxygenGenerator>, ISeedableModel
    {
        #region Limits
        // these should not be constant so crew can make changes in orbit
        [NotMapped]
        double cellOutputLiters = 270;
        [NotMapped]
        int oxygenNeedPerPersonLiters = 550;
        [NotMapped]
        int oxygenLevelTolerance = 2;
        [NotMapped]
        int totalNumOfCells = 10;

        public SystemStatus lastWorkingStatus;

        #endregion Limits

        #region Public Properties

        [NotMapped]
        public string ComponentName => "OxygenGenerator";

        public DateTimeOffset ReportDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// state of system; standby, processing, fail, etc
        /// </summary>
        public SystemStatus Status { get; set; }

        /// <summary>
        /// denotes if this system is to run autonomously or manually
        /// </summary>
        public Modes Mode { get; set; }

        /// <summary>
        /// sensor that checks for bubbles in inflow water, water is sent back to water processor if bubbles are present
        /// </summary>
        public bool InflowBubblesPresent { get; set; }

        /// <summary>
        /// switches water from going to reaction to water processor if bubbles are present
        /// </summary>
        public DiverterValvePositions DiverterValvePosition { get; set; }

        /// <summary>
        /// checks if hydrogen is present in product oxygen flow, if yes then there is a problem in the system and it 
        /// shuts down
        /// </summary>
        public bool HydrogenSensor { get; set; }

        /// <summary>
        /// Separates hydrogen from water outflow, water is recirculated back into water generator
        /// hydrogen is sent to water generator or vented to space
        /// </summary>
        public bool SeparatorOn { get; set; }

        /// <summary>
        /// circulates water from clean water feed and rotary separator to electrolysis cell stack
        /// </summary>
        public bool RecirculationPumpOn { get; set; }

        /// <summary>
        /// Each person aboard would require at least 3 cells to be active to maintain baseline oxygen requirements
        /// </summary>
        [Range(0, 12)]
        [IdealRange(3, 12)]
        public int NumActiveCells { get; set; }

        /// <summary>
        /// current oxygen output of the system
        /// </summary>
        [Range(0, 100)]
        [IdealRange(0, 90)]
        public double SystemOutput { get; set; }

        /// <summary>
        /// The desired oxygen concentration in the station air
        /// </summary>
        public int OxygenSetLevel { get; set; }

        /// <summary>
        /// actual oxygen level in the cabin
        /// </summary>
        [Range(15, 40)]
        [IdealRange(19, 23)]
        [IdealValue(21)]
        public int OxygenLevel { get; set; }

        #endregion Public Properties

        #region Constructors

        public OxygenGenerator() { }

        public OxygenGenerator(OxygenGenerator other)
        {
            Status = other.Status;

            Mode = other.Mode;
            InflowBubblesPresent = other.InflowBubblesPresent;
            DiverterValvePosition = other.DiverterValvePosition;
            HydrogenSensor = other.HydrogenSensor;
            SeparatorOn = other.SeparatorOn;
            RecirculationPumpOn = other.RecirculationPumpOn;
            NumActiveCells = other.NumActiveCells;
            SystemOutput = other.SystemOutput;
            OxygenSetLevel = other.OxygenSetLevel;
            lastWorkingStatus = other.lastWorkingStatus;

            GenerateData();
        }

        #endregion Constructors

        #region Methods

        public void SeedData()
        {
            Status = SystemStatus.Standby;
            Mode = Modes.Crewed;
            InflowBubblesPresent = false;
            DiverterValvePosition = DiverterValvePositions.Accept;
            HydrogenSensor = false;
            SeparatorOn = false;
            RecirculationPumpOn = false;
            NumActiveCells = 0;
            SystemOutput = 0;
            OxygenSetLevel = 20;
            OxygenLevel = 15;
            lastWorkingStatus = SystemStatus.Standby;
        }

        public void ProcessData()
        {
			GenerateData();
			
            if (Status == SystemStatus.Processing)
            {
                SimulateProcessing();
            }
            else if (Status == SystemStatus.Standby)
            {
                SimulateStandby();
            }
            else if (Status == SystemStatus.Trouble)
            {
                Trouble();
            }
            else
            { }

            SystemOutput = SimulateOutput();
        }

        #endregion  Methods

        #region Private Methods

        private void SimulateProcessing()
        {
            // system cannot work if any of these components fail (simulated by an 'off' state)
            if (HydrogenSensor || !SeparatorOn || !RecirculationPumpOn)
            {
                Status = SystemStatus.Trouble;
            }
            else
            {
                Status = SystemStatus.Processing;
            }

            if (OxygenLevel < OxygenSetLevel)
            {
                IncreaseActiveCells();
            }
            else if (OxygenLevel > OxygenSetLevel)
            {
                DecreaseActiveCells();
            }

            // if there are bubbles in replenishment water, reject it to avoid unwanted gasses in H2 flow downstream
            InflowBubbleSensor();
        }

        private void SimulateStandby()
        {
            if (OxygenLevel < OxygenSetLevel)
            {
                Status = SystemStatus.Processing;
                lastWorkingStatus = Status;
                SeparatorOn = true;
                RecirculationPumpOn = true;
            }

            // all components should be off while in 'Standby', else there is a system fault
            if (SeparatorOn || RecirculationPumpOn)
            {
                Status = SystemStatus.Trouble;
            }
			else
			{
				Status = SystemStatus.Processing;
			}
        }

        private void Trouble()
        {
            Status = SystemStatus.Trouble;
            if (lastWorkingStatus == SystemStatus.Processing)
            {
                SimulateProcessing();
            }
            else
            {
                SimulateStandby();
            }
        }

        public void GenerateData()
        {
            Random rand = new Random();
            OxygenLevel = rand.Next((OxygenSetLevel - oxygenLevelTolerance), (OxygenSetLevel + oxygenLevelTolerance));
            //OxygenLevel = this.LimitValue(t => t.OxygenLevel);

            if (Status == SystemStatus.Processing)
            {
                // trigger hydrogen sensor on occasion
                if (rand.Next(0, 100) == 9)
                {
                    HydrogenSensor = true;
                }
                else
                {
                    HydrogenSensor = false;
                }

                // trigger bubble sensor on occasion
                if (rand.Next(0, 10) == 7)
                {
                    InflowBubblesPresent = true;
                }
                else
                {
                    InflowBubblesPresent = false;
                }
            }
        }

        public void IncreaseActiveCells()
        {
            // increase number of cells making oxygen, if more are available
            if (NumActiveCells < totalNumOfCells)
            {
                NumActiveCells++;
            }
            else
            {
                NumActiveCells = totalNumOfCells;
            }
        }

        public void DecreaseActiveCells()
        {
            // decrease number of cells making oxygen
            if (NumActiveCells > 1)
            {
                NumActiveCells--;
            }
            // go to 'Standby' if last cell is 'turned off'
            else
            {
                NumActiveCells = 0;
                Status = SystemStatus.Standby;
                lastWorkingStatus = Status;
                SeparatorOn = false;
                RecirculationPumpOn = false;
            }
        }

        private void InflowBubbleSensor()
        {
            if (InflowBubblesPresent)
            {
                DiverterValvePosition = DiverterValvePositions.Reprocess;
            }
            else
            {
                DiverterValvePosition = DiverterValvePositions.Accept;
            }
        }

        private double SimulateOutput()
        {
            return NumActiveCells * cellOutputLiters;
        }

        #endregion Private Methods

        #region Check Alerts

        private IEnumerable<Alert> CheckHydrogenSensor()
        {
            if (HydrogenSensor)
            {
                Trouble();
                yield return this.CreateAlert(a => a.HydrogenSensor, "Hydrogen detected in outflow", AlertLevel.HighError);
            }
            else
            {
                yield return this.CreateAlert(a => a.HydrogenSensor);
            }
        }

        private IEnumerable<Alert> CheckSeperator()
        {
            if (Status.Equals(SystemStatus.Processing))
            {
                if (!SeparatorOn)
                {
                    Trouble();
                    yield return this.CreateAlert(a => a.SeparatorOn, "Seperator off while in processing status", AlertLevel.HighError);
                }
            }
            else if (Status.Equals(SystemStatus.Standby))
            {
                if (SeparatorOn)
                {
                    Trouble();
                    yield return this.CreateAlert(a => a.SeparatorOn, "Seperator on while in Standby status", AlertLevel.HighError);
                }
            }
            else
            {
                yield return this.CreateAlert(a => a.SeparatorOn);
            }
        }

        private IEnumerable<Alert> CheckPump()
        {
            if (Status.Equals(SystemStatus.Processing))
            {
                if (!RecirculationPumpOn)
                {
                    Trouble();
                    yield return this.CreateAlert(a => a.RecirculationPumpOn, "Pump is off while in Processing status", AlertLevel.HighError);
                }
            }
            else if (Status.Equals(SystemStatus.Standby))
            {
                if (RecirculationPumpOn)
                {
                    Trouble();
                    yield return this.CreateAlert(a => a.RecirculationPumpOn, "Pump in on while in Standby status", AlertLevel.HighError);
                }
            }
            else
            {
                yield return this.CreateAlert(a => a.RecirculationPumpOn);
            }
        }

        private IEnumerable<Alert> CheckMaxProduction()
        {
            // at maximum output and cabin oxygen concentration is still low
            if ((NumActiveCells >= totalNumOfCells) && (OxygenLevel < (OxygenSetLevel - oxygenLevelTolerance)))
            {
                const string msg = "Maximum oxygen production not maintaining set oxygen level";
                yield return this.CreateAlert(a => a.OxygenLevel, msg, AlertLevel.HighError);
                yield return this.CreateAlert(a => a.NumActiveCells, msg, AlertLevel.HighError);
            }
            else if (NumActiveCells >= totalNumOfCells && (OxygenLevel <= OxygenSetLevel))
            {
                const string msg = "All oxygen production cells currently active";
                yield return this.CreateAlert(a => a.OxygenLevel, msg, AlertLevel.HighWarning);
                yield return this.CreateAlert(a => a.NumActiveCells, msg, AlertLevel.HighWarning);
            }
            else
            {
                yield return this.CreateAlert(a => a.OxygenLevel);
                yield return this.CreateAlert(a => a.NumActiveCells);
            }
        }

        IEnumerable<Alert> IAlertableModel.GenerateAlerts()
        {
            return CheckHydrogenSensor()
                .Concat(this.CheckSeperator())
                .Concat(this.CheckPump())
                .Concat(this.CheckMaxProduction());
        }

        #endregion  Check Alerts

        #region Equality Members

        public bool Equals(OxygenGenerator other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return this.ReportDateTime.Equals(other.ReportDateTime)
                && this.Status == other.Status
                && this.InflowBubblesPresent == other.InflowBubblesPresent
                && this.DiverterValvePosition == other.DiverterValvePosition
                && this.HydrogenSensor == other.HydrogenSensor
                && this.SeparatorOn == other.SeparatorOn
                && this.RecirculationPumpOn == other.RecirculationPumpOn
                && this.NumActiveCells == other.NumActiveCells
                && this.SystemOutput == other.SystemOutput
                && this.Mode == other.Mode
                && this.OxygenSetLevel == other.OxygenSetLevel
                && this.OxygenLevel == other.OxygenLevel;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OxygenGenerator other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                this.ReportDateTime,
                this.Status,
                this.InflowBubblesPresent,
                this.DiverterValvePosition,
                this.HydrogenSensor,
                this.SeparatorOn,
                this.RecirculationPumpOn,

                // tuple for overflow arguments
                (this.NumActiveCells, this.SystemOutput, this.Mode, this.OxygenSetLevel, this.OxygenLevel)
                );
        }

        public static bool operator ==(OxygenGenerator left, OxygenGenerator right) => Equals(left, right);

        public static bool operator !=(OxygenGenerator left, OxygenGenerator right) => !Equals(left, right);

        #endregion Equality Members
    }
}