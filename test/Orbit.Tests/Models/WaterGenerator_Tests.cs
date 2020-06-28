using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Orbit.Models;

namespace Orbit.Tests.Models
{
    public class WaterGenerator_Tests
    {
        private readonly ITestOutputHelper output;

        public WaterGenerator_Tests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestProcessingIncreasesMethane()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 500;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(35, wg.MethaneStoreLevel);
        }

        [Fact]
        public void TestProcessingDecreasesH2()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 400;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(18, wg.H2StoreLevel);
        }

        [Fact]
        public void TestProcessingDecreasesCo2()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 400;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(22, wg.Co2StoreLevel);
        }

        [Fact]
        public void TestProcessingMethaneFull()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.MethaneStoreLevel = 95;
            wg.ProcessData();
            Assert.Equal(0, wg.MethaneStoreLevel);
        }

        [Fact]
        public void TestProcessingH2Empty()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.H2StoreLevel = 2;
            wg.ProcessData();
            Assert.Equal(0, wg.H2StoreLevel);
        }

        [Fact]
        public void TestProcessingCo2Empty()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.Co2StoreLevel = 4;
            wg.ProcessData();
            Assert.Equal(0, wg.Co2StoreLevel);
        }

        [Fact]
        public void TestCo2Empty_ChangeToStandby()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.Co2StoreLevel = 4;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Standby, wg.Status);
        }

        [Fact]
        public void TestH2Empty_ChangeToStandby()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.H2StoreLevel = 2;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Standby, wg.Status);
        }

        [Fact]
        public void TestTurnOff()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.H2StoreLevel = 2;
            wg.ProcessData();
            Assert.False(wg.PumpOn);
        }

        [Fact]
        public void TestStayInStandby()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.ProcessData();
            Assert.Equal(SystemStatus.Standby, wg.Status);
        }

        [Fact]
        public void TestSimulateStandby()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.ProcessData();
            Assert.Equal(22, wg.H2StoreLevel); //20 start level + fill value of 2
        }

        [Fact]
        public void TestCo2Full_ChangeToProcessing()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Co2StoreLevel = 76;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Processing, wg.Status);
        }

        [Fact]
        public void TestH2Full_ChangeToProcessing()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.H2StoreLevel = 76;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Processing, wg.Status);
        }

        [Fact]
        public void TestTurnOn_ReactorHot()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Co2StoreLevel = 76;
            wg.ReactorTemp = 500;
            wg.ProcessData();
            Assert.False(wg.HeaterOn);
        }

        [Fact]
        public void TestTurnOn_ReactorCold()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Co2StoreLevel = 76;
            wg.ProcessData();
            Assert.True(wg.HeaterOn);
        }

        [Fact]
        public void TestCopyConstructor()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            WaterGeneratorData newwg = new WaterGeneratorData(wg);
            Assert.Equal(wg.Status, newwg.Status);
        }

        [Fact]
        public void TestGenerateProcessingData()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            WaterGeneratorData newwg = new WaterGeneratorData(wg);
            Assert.NotEqual(wg.ReactorTemp, newwg.ReactorTemp);
        }

        [Fact]
        public void TestGenerateStandbyData()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            WaterGeneratorData newwg = new WaterGeneratorData(wg);
            Assert.NotEqual(wg.ReactorTemp, newwg.ReactorTemp);
        }

        [Fact]
        public void TestProcessingTrouble_ColdReactor()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 449;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_HotReactor()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 651;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_SeperatorOff()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = false;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_PumpOff()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = false;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_MethaneFull()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 475;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.MethaneStoreLevel = 100;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_HeaterOffReactorCold()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 200;
            wg.HeaterOn = false;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_HeaterOnReactorHot()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 651;
            wg.HeaterOn = true;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2000;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_SeperatorToFast()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 500;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 2401;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestProcessingTrouble_SeperatorToSlow()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.Status = SystemStatus.Processing;
            wg.PumpOn = true;
            wg.ReactorTemp = 500;
            wg.SeperatorOn = true;
            wg.SeperatorMotorSpeed = 999;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestStandbyTrouble_ReactorHot()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.ReactorTemp = 426;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestStandbyTrouble_SeperatorOn()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.SeperatorOn = true;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestStandbyTrouble_PumpOn()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.PumpOn = true;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestStandbyTrouble_HeaterOn()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.HeaterOn = true;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }

        [Fact]
        public void TestStandbyTrouble_MethaneStoreFull()
        {
            WaterGeneratorData wg = new WaterGeneratorData();
            wg.SeedData();
            wg.MethaneStoreLevel = 91;
            wg.ProcessData();
            Assert.Equal(SystemStatus.Trouble, wg.Status);
        }



    }
}
