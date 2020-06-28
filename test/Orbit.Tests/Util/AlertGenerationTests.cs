using System;
using System.ComponentModel.DataAnnotations;
using AutoFixture.Xunit2;
using FluentAssertions;

using Orbit.Annotations;
using Orbit.Models;

using Xunit;

namespace Orbit.Tests.Models
{
    public class AlertGenerationTests
    {
        [Fact]
        public void Should_properly_infer_range_limits()
        {
            var data = new WaterProcessorData();

            var alert = data.CreateAlert(a => a.ProductTankLevel).Metadata.TotalRange;

            alert.Minimum.Should().Be(0, because: "the property has a RangeAttribute applied with the min set to 0");
            alert.Maximum.Should().Be(100, because: "the property has a RangeAttribute applied with the max set to 100");
        }
        
        [Fact]
        public void Should_be_able_to_infer_metadata_attributes_using_extension_method()
        {
            var data = new ExternalCoolantLoopData();
            var alert = data.CreateAlert(a => a.LineAPressure);

            alert.AlertLevel.Should().Be(AlertLevel.Safe, because: "invoking CreateAlert with no parameters should create a 'Safe' alert");

            alert.Metadata.Should().NotBeNull();

            alert.Metadata.IdealRange.Should().NotBeNull(because: "this property has an {0} applied", nameof(IdealRangeAttribute));
            alert.Metadata.TotalRange.Should().NotBeNull(because: "this property has a {0} applied", nameof(RangeAttribute));
            alert.Metadata.IdealValue.Should().NotBeNull(because: "this property has an {0} applied", nameof(IdealValueAttribute));
            alert.Metadata.UnitType.Should().NotBeNullOrEmpty(because: "this property has a {0} applied", nameof(UnitTypeAttribute));
        }

        [Theory]
        [AutoData]
        public void Should_be_able_to_get_current_value_via_creator_extension(int value)
        {
            var data = new ExternalCoolantLoopData {LineAPressure = value};
            var alert = data.CreateAlert(a => a.LineAPressure);

            alert.IsSafe.Should().BeTrue();

            alert.CurrentValue.Should().Be(value);
        }
    }
}