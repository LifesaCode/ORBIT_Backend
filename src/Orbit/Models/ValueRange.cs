using System;

namespace Orbit.Models
{
    public readonly struct ValueRange : IEquatable<ValueRange>
    {
        public ValueRange(double min, double max)
        {
            this.Minimum = min;
            this.Maximum = max;
        }

        public double Minimum { get; }

        public double Maximum { get; }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public bool IsSet => Minimum != Maximum;

        public bool IsInRangeExclusive(double value) => value > this.Minimum && value < this.Maximum;

        public bool IsInRangeInclusive(double value) => value >= this.Minimum && value <= this.Maximum;

        public double ToPercentage(double value)
        {
            return (value / (Maximum - Minimum)) * 100;
        }

        #region Equality members

        public override string ToString() => $"{Minimum} - {Maximum}";

        public override bool Equals(object? obj) => obj is ValueRange range && this.Equals(range);

        private const double DIFF_TOLERANCE = 0.00001;

        public bool Equals(ValueRange other) => Math.Abs(this.Minimum - other.Minimum) < DIFF_TOLERANCE && Math.Abs(this.Maximum - other.Maximum) < DIFF_TOLERANCE;

        public override int GetHashCode() => HashCode.Combine(this.Minimum, this.Maximum);

        public static bool operator ==(ValueRange left, ValueRange right) => left.Equals(right);

        public static bool operator !=(ValueRange left, ValueRange right) => !(left == right);

        #endregion Equality members
    }
}