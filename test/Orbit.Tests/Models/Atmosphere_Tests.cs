using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Orbit.Models;

namespace Orbit.Tests.Models
{
    public class Atmosphere_Tests
    {
        [Fact]
        public void TestTempWithinAcceptableUpperDeviance()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.Temperature = 19.5;
            ad.ProcessData();
            Assert.Equal(60, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestTempWithinAcceptableLowerDeviance()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.Temperature = 18.5;
            ad.ProcessData();
            Assert.Equal(60, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestCrewedHumidityWithinAcceptableUpperDeviance()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.HumidityLevel = 45;
            ad.ProcessData();
            Assert.Equal(60, ad.TempControlBafflePosition);
        }


        [Fact]
        public void TestHumidityWithinAcceptableLowerDeviance()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.HumidityLevel = 35;
            ad.ProcessData();
            Assert.Equal(60, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestTempToHot()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.Temperature = 19.6;
            ad.ProcessData();
            Assert.Equal(61, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestHumidToHigh()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.HumidityLevel = 60;
            ad.ProcessData();
            Assert.Equal(61, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestMaxedTempControlBaffle()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.TempControlBafflePosition = 100;
            ad.Temperature = 19.6;
            ad.ProcessData();
            Assert.Equal(100, ad.TempControlBafflePosition);
        }
        
        [Fact]
        public void TestUnCrewedToCold()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.Temperature = 2;
            ad.ProcessData();
            Assert.Equal(59, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestZeroedTempControlBaffle()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.TempControlBafflePosition = 0;
            ad.Temperature = 18.4;
            ad.ProcessData();
            Assert.Equal(0, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestUnCrewedToDry()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.HumidityLevel = 20;
            ad.ProcessData();
            Assert.Equal(59, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestCrewedToCold()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.CabinStatus = Modes.Crewed;
            ad.Temperature = 15;
            ad.ProcessData();
            Assert.Equal(59, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestCrewedToDry()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.HumidityLevel = 34;
            ad.ProcessData();
            Assert.Equal(59, ad.TempControlBafflePosition);
        }

        [Fact]
        public void TestLiquidInOutflow()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.LiquidInOutflow = true;
            ad.ProcessData();
            Assert.Equal(DiverterValvePositions.Reprocess, ad.ReprocessBafflePosition);
        }

        [Fact]
        public void TestLiquidInOutflowCleared()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.ReprocessBafflePosition = DiverterValvePositions.Reprocess;
            ad.LiquidInOutflow = false;
            ad.ProcessData();
            Assert.Equal(DiverterValvePositions.Accept, ad.ReprocessBafflePosition);
        }

        [Fact]
        public void TestNewTempValueGenerated()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            AtmosphereData newad = new AtmosphereData(ad);
            Assert.NotEqual(newad.Temperature, ad.Temperature);
        }
        
        [Fact]
        public void TestSeperatorOffWhileUncrewed()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            AtmosphereData newad = new AtmosphereData(ad);
            Assert.Equal(0, newad.SeperatorSpeed);
        }

        [Fact]
        public void TestSeperatorOnWhileCrewed()
        {
            AtmosphereData ad = new AtmosphereData();
            ad.SeedData();
            ad.CabinStatus = Modes.Crewed;
            AtmosphereData newad = new AtmosphereData(ad);
            newad.SeperatorSpeed.Should().BeGreaterThan(1000);
        }
    }
}
