using System;

namespace Orbit.Annotations
{
    /// <summary>
    ///   Describes the ideal range (within the full possible range) of values for the given property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdealRangeAttribute : Attribute
    {
        /// <summary>
        ///   Describes the ideal range (within the full possible range) of values for the given property.
        /// </summary>
        /// <param name="idealMinimum"> The ideal minimum value (inclusive) for the property. </param>
        /// <param name="idealMaximum"> The ideal maximum value (inclusive) for the property. </param>
        public IdealRangeAttribute(double idealMinimum, double idealMaximum)
        {
            this.IdealMinimum = idealMinimum;
            this.IdealMaximum = idealMaximum;
        }

        /// <summary>
        ///   Describes the ideal range (within the full possible range) of values for the given property.
        /// </summary>
        /// <param name="idealMinimum"> The ideal minimum value (inclusive) for the property. </param>
        /// <param name="idealMaximum"> The ideal maximum value (inclusive) for the property. </param>
        public IdealRangeAttribute(int idealMinimum, int idealMaximum)
        {
            this.IdealMinimum = idealMinimum;
            this.IdealMaximum = idealMaximum;
        }

        /// <summary>
        ///   Describes the ideal range (within the full possible range) of values for the given property.
        /// </summary>
        /// <param name="idealMinimum"> The ideal minimum value (inclusive) for the property. </param>
        /// <param name="idealMaximum"> The ideal maximum value (inclusive) for the property. </param>
        public IdealRangeAttribute(float idealMinimum, float idealMaximum)
        {
            this.IdealMinimum = idealMinimum;
            this.IdealMaximum = idealMaximum;
        }

        /// <summary>
        ///   The ideal minimum value (inclusive) for the property.
        /// </summary>
        public double IdealMinimum { get; }

        /// <summary>
        ///   The ideal maximum value (inclusive) for the property.
        /// </summary>
        public double IdealMaximum { get; }
    }


}