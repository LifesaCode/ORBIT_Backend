using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Orbit.Models;

namespace Orbit.Tests.Models
{
    public class Co2Tests
    {
        private readonly ITestOutputHelper output;

        public Co2Tests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestSimulateStandby()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Co2Level = 1;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Standby, co2.Status);
        }

        [Fact]
        public void TestStandbyFanOff()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Co2Level = 1;
            co2.ProcessData();
            Assert.False(co2.FanOn);
        }

        [Fact]
        public void TestChangeToProcessing()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.ProcessData();
            Assert.Equal(SystemStatus.Processing, co2.Status);
        }

        [Fact]
        public void TestChangeToStandby()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Co2Level = 1;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Standby, co2.Status);
        }

        [Fact]
        public void TestFanTrouble()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = false;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Trouble, co2.Status);
        }

        [Fact]
        public void TestSimulateProcessing()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed2Temperature = 240;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Processing, co2.Status);
        }

        [Fact]
        public void TestAbsorbingBedChangeToBed2()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed2Temperature = 230;
            co2.count = 31;
            co2.ProcessData();
            Assert.Equal(BedOptions.Bed2, co2.AbsorbingBed);
        }

        [Fact]
        public void TestRegeneratingBedChangeToBed2()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.BedSelectorValve = BedOptions.Bed2;
            co2.Bed1Temperature = 230;
            co2.AbsorbingBed = BedOptions.Bed2;
            co2.RegeneratingBed = BedOptions.Bed1;
            co2.count = 31;
            co2.ProcessData();
            Assert.Equal(BedOptions.Bed2, co2.RegeneratingBed);
        }

        [Fact]
        public void TestProcessingTrouble_FanOff()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.Bed2Temperature = 230;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Trouble, co2.Status);
        }

        [Fact]
        public void TestProcessingTrouble_HighCo2Output()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed2Temperature = 230;
            co2.Co2OutputLevel = 5;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Trouble, co2.Status);
        }

        [Fact]
        public void TestProcessingTrouble_HighAbsorbingBedTemp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed1Temperature = 230;
            co2.Bed2Temperature = 230;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Trouble, co2.Status);
        }

        [Fact]
        public void TestProcessingTrouble_LowRegeneratingBedTemp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed2Temperature = 21;
            co2.ProcessData();
            Assert.Equal(SystemStatus.Trouble, co2.Status);
        }

        /// <summary>
        /// bed 2 is regenerating, so it should be hot and bed 1 cold
        /// </summary>
        [Fact]
        public void TestProcessingTrouble_HighRegeneratingBedTemp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            co2.Bed2Temperature = 260;
            co2.ProcessData();
            co2.Status.Should().Be(SystemStatus.Trouble, "bed 2 is regenerating, so it should be hot and bed 1 cold");
        }

        [Fact]
        public void TestCopyConstructor()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            CarbonDioxideRemediation newco2 = new CarbonDioxideRemediation(co2);
            Assert.Equal(newco2.AbsorbingBed, co2.AbsorbingBed);
        }

        [Fact]
        public void TestGenerateNewCo2Level()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            CarbonDioxideRemediation newco2 = new CarbonDioxideRemediation(co2);
            Assert.NotEqual(newco2.Co2Level, co2.Co2Level);
        }

        [Fact]
        public void TestGenerateBed2RegeneratingTemp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            CarbonDioxideRemediation newco2 = new CarbonDioxideRemediation(co2);
            newco2.Bed2Temperature.Should().BeGreaterThan(119);
        }

        [Fact]
        public void TestGenerateBed1RegeneratingTemp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            co2.Status = SystemStatus.Processing;
            co2.FanOn = true;
            CarbonDioxideRemediation newco2 = new CarbonDioxideRemediation(co2);
            newco2.Bed1Temperature.Should().BeLessOrEqualTo(119);
        }

        [Fact]
        public void TestStandbyBed1Temp()
        {
            CarbonDioxideRemediation co2 = new CarbonDioxideRemediation();
            co2.SeedData();
            CarbonDioxideRemediation newco2 = new CarbonDioxideRemediation(co2);
            newco2.Bed1Temperature.Should().BeLessThan(33);
        }
    }
}
