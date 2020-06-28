using System.Collections.Generic;

namespace Orbit.Models
{
    public class Alert
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        internal Alert(string propertyName,
            string message,
            AlertLevel level,
            PropertyMetadata? metadata = null,
            object? currentValue = null)
        {
            this.PropertyName = propertyName;
            this.Message = message;
            this.AlertLevel = level;
            this.Metadata = metadata ?? new PropertyMetadata();
            this.CurrentValue = currentValue;
        }

        /// <summary>
        ///   The name of the property of the model that generated the alert.
        /// </summary>
        public string PropertyName { get; }

        public string Message { get; }

        public AlertLevel AlertLevel { get; }

        /// <summary>
        ///   Contains additional information about the property that triggered the alert, such as range information and
        ///   unit type.
        /// </summary>
        public PropertyMetadata Metadata { get; }

        /// <summary>
        ///   The value of the relevant property at the time of the alert.
        /// </summary>
        public object? CurrentValue { get; set; }

        /// <summary>
        ///   Optionally specify additional data via key-value pairs.
        /// </summary>
        public IReadOnlyDictionary<string, object> Data => this._data;

        public bool IsSafe => this.Message.Length == 0 && this.AlertLevel == AlertLevel.Safe;

        public override string ToString() => $"({PropertyName} {AlertLevel}): {Message}";

        /// <summary>
        ///   Adds the provided <see cref="key"/> and <see cref="value"/> to the <see cref="Data"/> of this <see cref="Alert"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This Alert for method chaining.</returns>
        internal Alert WithData(string key, object value)
        {
            this._data.Add(key, value);
            return this;
        }

        /// <summary>
        ///   Adds the provided KeyValuePairs to the <see cref="Data"/> of this <see cref="Alert"/>.
        /// </summary>
        /// <param name="data">A collection of key-value pairs, e.g. an <see cref="IDictionary{TKey, TValue}"/></param>
        /// <returns>This Alert for method chaining.</returns>
        internal Alert WithData(IEnumerable<KeyValuePair<string, object>> data)
        {
            foreach (var item in data)
            {
                this._data[item.Key] = item.Value;
            }
            return this;
        }
    }
}