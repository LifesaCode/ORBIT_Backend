using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Orbit.Annotations;

namespace Orbit.Models
{
    internal static class AlertExtensions
    {
        private static readonly ConcurrentDictionary<int, PropertyMetadata> _metadata =
            new ConcurrentDictionary<int, PropertyMetadata>();

        internal static PropertyMetadata GetMetadata(MemberInfo member)
        {
            // Indexed by hashcode to reduce the memory footprint needed
            var info = _metadata.GetOrAdd(member.GetHashCode(), _ =>
             {
                 var attributes = member.GetCustomAttributes().ToArray();

                 var range = attributes.OfType<RangeAttribute>().SingleOrDefault();
                 var idealRange = attributes.OfType<IdealRangeAttribute>().SingleOrDefault();
                 var idealValue = attributes.OfType<IdealValueAttribute>().SingleOrDefault();
                 var unit = attributes.OfType<UnitTypeAttribute>().SingleOrDefault();

                 return new PropertyMetadata(range, idealRange, idealValue, unit);
             });

            return info;
        }

        /// <summary>
        ///   Retrieve the metadata of the given property. This could be used from within the alert generation methods
        ///   to compare against the range values.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="model">The model data object.</param>
        /// <param name="propSelector">An expression (lambda method) selecting the property which the alert is for.</param>
        /// <returns>The metadata for the provided property.</returns>
        public static PropertyMetadata GetMetadata<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> propSelector)
        {
            var body = (MemberExpression)propSelector.Body;

            return GetMetadata(body.Member);
        }

        /// <summary>
        ///   Create a new <see cref="Alert"/> with the provided properties.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="model">The model data object.</param>
        /// <param name="propSelector">An expression (lambda method) selecting the property which the alert is for.</param>
        /// <param name="message">The message for the alert.</param>
        /// <param name="level">The level of the alert.</param>
        /// <returns>The newly created alert.</returns>
        public static Alert CreateAlert<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> propSelector, string message = "", AlertLevel level = AlertLevel.Safe) where TModel : class, IAlertableModel
        {

            var memberName = ((MemberExpression)propSelector.Body).Member.Name;
            var info = model.GetMetadata(propSelector);

            var value = propSelector.Compile()(model);
            return new Alert(memberName, message, level, info, value);
        }
        
        public static int LimitValue<TModel>(this TModel model, Expression<Func<TModel, int>> selector) where TModel : IModel
        {
            var body = (MemberExpression)selector.Body;

            var meta = GetMetadata(body.Member);
            int value = selector.Compile()(model);

            return Limit(value, meta.TotalRange);
        }
        public static double LimitValue<TModel>(this TModel model, Expression<Func<TModel, double>> selector) where TModel : IModel
        {
            var body = (MemberExpression)selector.Body;

            var meta = GetMetadata(body.Member);
            double value = selector.Compile()(model);

            return Limit(value, meta.TotalRange);
        }

        private static int Limit(int value, ValueRange range)
        {
            return (int)Limit((double)value, range);
        }

        private static double Limit(double value, ValueRange range)
        {
            if (value > range.Maximum)
                return range.Maximum;
            if (value < range.Minimum)
                return range.Minimum;
            return value;
        }
    }
}