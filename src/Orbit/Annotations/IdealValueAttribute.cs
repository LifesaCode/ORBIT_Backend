using System;

namespace Orbit.Annotations
{
    /// <summary>
    ///   Indicates the ideal value for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdealValueAttribute : Attribute
    {
        /// <summary>
        ///   Indicates the ideal value for a property.
        /// </summary>
        /// <param name="idealValue"> The ideal value for the property. </param>
        public IdealValueAttribute(double idealValue)
        {
            this.IdealValue = idealValue;
        }

        /// <summary>
        ///   Indicates the ideal value for a property.
        /// </summary>
        /// <param name="idealValue"> The ideal value for the property. </param>
        public IdealValueAttribute(int idealValue)
        {
            this.IdealValue = idealValue;
        }

        /// <summary>
        ///   The ideal value for the property.
        /// </summary>
        public double IdealValue { get; }
    }
}