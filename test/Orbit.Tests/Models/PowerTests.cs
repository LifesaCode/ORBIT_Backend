using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Orbit.Models;

namespace Orbit.Tests.Models
{
    public class PowerTests
    {
        [Fact]
        public void TestGenerateSolarVoltage_NotDeployed()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarDeployed = false;
            PowerSystemData newps = new PowerSystemData(ps);
            Assert.NotEqual(newps.SolarArrayVoltage, ps.SolarArrayVoltage);
        }

        [Fact]
        public void TestGenerateSolarVoltage()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            PowerSystemData newps = new PowerSystemData();
            Assert.NotEqual(ps.SolarArrayVoltage, newps.SolarArrayVoltage);
        }

        [Fact]
        public void TestBatteryTempLow()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.BatteryTemperature = -10;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status);
        }
        [Fact]
        public void TestBatteryTempHigh()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.BatteryTemperature = 35;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status);
        }

        [Fact]
        public void TestSimulateCharge_BatteryNotFull()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.ProcessData();
            Assert.Equal(86, ps.BatteryChargeLevel);
        }

        [Fact]
        public void TestSimulateCharge_BatteryFull()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.BatteryChargeLevel = 105;
            ps.ProcessData();
            Assert.Equal(105, ps.BatteryChargeLevel);
        }

        [Fact]
        public void TestSimulateCharge_VoltageToHigh()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 181;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status);
        }

        [Fact]
        public void TestChangeToDischarge()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.ProcessData();
            Assert.Equal(PowerShuntState.Discharge, ps.ShuntStatus);
        }

        [Fact]
        public void TestBatteryDischarge()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.ProcessData();
            Assert.Equal(84, ps.BatteryChargeLevel);
        }

        [Fact]
        public void TestBatteryDischarge_BatteryEmpty()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.BatteryChargeLevel = -1;
            ps.ProcessData();
            Assert.Equal(0, ps.BatteryChargeLevel);
        }

        [Fact]
        public void TestBatteryDischargeTrouble_BatteryLow()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.BatteryChargeLevel = 50;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status); ;
        }

        [Fact]
        public void TestBatteryDischargeTrouble_VoltageLow()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.BatteryVoltage = 109;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status); ;
        }

        [Fact]
        public void TestBatteryDischargeTrouble_VoltageHigh()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayVoltage = 159;
            ps.BatteryVoltage = 161;
            ps.ProcessData();
            Assert.Equal(SystemStatus.Trouble, ps.Status); ;
        }

         [Fact]
         public void TestRotatePanels_Increasing()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.ProcessData();
            Assert.Equal(1, ps.SolarArrayRotation);
        }

        [Fact]
        public void TestRotatePanels_Decreasing()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarRotationIncreasing = false;
            ps.ProcessData();
            Assert.Equal(-1, ps.SolarArrayRotation);
        }

        [Fact]
        public void TestPanelSwitchDirection()
        {
            PowerSystemData ps = new PowerSystemData();
            ps.SeedData();
            ps.SolarArrayRotation = 205;
            ps.ProcessData();
            Assert.False(ps.SolarRotationIncreasing);
        }
    }
}
