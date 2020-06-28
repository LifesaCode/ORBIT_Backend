using System;

namespace Orbit.Annotations
{
    /// <summary>
    ///   Indicates the unit type of the property the attribute is applied to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UnitTypeAttribute : Attribute
    {
        /// <summary>
        ///   Indicates the unit type of the property the attribute is applied to.
        /// </summary>
        /// <param name="unitTypeName"> The name of the unit type for the property. </param>
        public UnitTypeAttribute(string unitTypeName)
        {
            this.UnitTypeName = unitTypeName;
        }

        /// <summary>
        ///   The name of the unit type for the property.
        /// </summary>
        public string UnitTypeName { get; }
    }
}