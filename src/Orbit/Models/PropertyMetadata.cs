using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orbit.Annotations;

namespace Orbit.Models
{
    public sealed class PropertyMetadata : IEquatable<PropertyMetadata>
    {
        public PropertyMetadata()
        {
            
        }
        public PropertyMetadata(RangeAttribute? totalRange, IdealRangeAttribute? idealRange, IdealValueAttribute? idealValue, UnitTypeAttribute? unitType)
        {
            if (totalRange != null)
            {
                this.TotalRange = new ValueRange(Convert.ToDouble(totalRange.Minimum), Convert.ToDouble(totalRange.Maximum));
            }
            else
            {
                this.TotalRange = default;
            }

            if (idealRange != null)
            {
                this.IdealRange = new ValueRange(Convert.ToDouble(idealRange.IdealMinimum), Convert.ToDouble(idealRange.IdealMaximum));
            }
            else
            {
                this.IdealRange = default;
            }

            this.IdealValue = idealValue?.IdealValue;
            this.UnitType = unitType?.UnitTypeName;
        }

        public ValueRange TotalRange { get; }

        public ValueRange IdealRange { get; }

        public double? IdealValue { get; }

        public string? UnitType { get; }

        /// <summary>
        ///   Optionally specify additional data via key-value pairs.
        /// </summary>
        public IDictionary<string, object> Extra { get; } = new Dictionary<string, object>();

        #region Equality members

        public override bool Equals(object? obj) => obj is PropertyMetadata information && this.Equals(information);

        public bool Equals(PropertyMetadata other)
        {
            return EqualityComparer<ValueRange?>.Default.Equals(this.TotalRange, other.TotalRange)
                   &&EqualityComparer<ValueRange?>.Default.Equals(this.IdealRange, other.IdealRange) 
                   && this.IdealValue == other.IdealValue
                   && this.UnitType == other.UnitType;
        }

        public override int GetHashCode() => HashCode.Combine(this.TotalRange, this.IdealRange, this.IdealValue, this.UnitType);

        public static bool operator ==(PropertyMetadata left, PropertyMetadata right) => left.Equals(right);

        public static bool operator !=(PropertyMetadata left, PropertyMetadata right) => !(left == right);

        #endregion Equality members
    }
}