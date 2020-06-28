using System;
using System.Collections.Generic;
using System.Text;
using Orbit.Models;
using Xunit;


namespace Orbit.Tests.Models
{
    public class InternalCoolantTests
    {
        [Fact]
        public void TestDualLoop_TempWithinRange()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.ProcessData();
            Assert.Equal(15, ic.LowTempMixValvePosition);
            Assert.Equal(32, ic.MedTempMixValvePosition);
        }

        [Fact]
        public void TestDualLoop_LowTempHigh()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.TempLowLoop = 8;
            ic.ProcessData();
            Assert.Equal(16, ic.LowTempMixValvePosition);
        }

        [Fact]
        public void TestDualLoop_LowTempLow()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.TempLowLoop = 0;
            ic.ProcessData();
            Assert.Equal(14, ic.LowTempMixValvePosition);
        }

        [Fact]
        public void TestDualLoop_MedTempHigh()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.TempMedLoop = 15;
            ic.ProcessData();
            Assert.Equal(33, ic.MedTempMixValvePosition);
        }

        [Fact]
        public void TestDualLoop_MedTempLow()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.TempMedLoop = 6;
            ic.ProcessData();
            Assert.Equal(31, ic.MedTempMixValvePosition);
        }

        [Fact]
        public void TestBothPumpsOff()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.LowTempPumpOn = false;
            ic.MedTempPumpOn = false;
            ic.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ic.Status);
        }

        [Fact]
        public void TestManualMedSingleLoop_WithinRange()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.LowTempPumpOn = false;
            ic.MedTempSingleLoop = true;
            ic.ProcessData();
            Assert.Equal(SystemStatus.On, ic.Status);
        }

        [Fact]
        public void TestManualLowSingleLoop_LowLoopTempHigh()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.MedTempPumpOn = false;
            ic.LowTempSingleLoop = true;
            ic.TempLowLoop = 7;
            ic.CrossoverMixValvePosition = 12;
            ic.ProcessData();
            Assert.Equal(13, ic.CrossoverMixValvePosition);
        }

        [Fact]
        public void TestManualLowSingleLoop_MedLoopTempLow()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.MedTempPumpOn = false;
            ic.LowTempSingleLoop = true;
            ic.TempMedLoop = 6;
            ic.CrossoverMixValvePosition = 12;
            ic.ProcessData();
            Assert.Equal(11, ic.CrossoverMixValvePosition);
        }

        [Fact]
        public void TestConvertFromDualToSingleLoop()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.MedTempPumpOn = false;
            ic.ProcessData();
            Assert.Equal(40, ic.CrossoverMixValvePosition);
        }

        [Fact]
        public void TestDualLoopPumpFailTrouble()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.LowTempPumpOn = false;
            ic.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ic.Status);
        }

        [Fact]
        public void TestSingleLowLoopPumpFailure()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.LowTempSingleLoop = true;
            ic.MedTempPumpOn = false;
            ic.LowTempPumpOn = false;
            ic.ProcessData();
            Assert.True(ic.MedTempPumpOn);
        }

        [Fact]
        public void TestSingleMedLoopPumpFailure()
        {
            InternalCoolantLoopData ic = new InternalCoolantLoopData();
            ic.SeedData();
            ic.MedTempSingleLoop = true;
            ic.MedTempPumpOn = false;
            ic.LowTempPumpOn = false;
            ic.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ic.Status);
        }
    }
}
