#if !NETSTANDARD_21
#pragma warning disable
//ReSharper disable all

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.CodeAnalysis
{
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited = false)]
    public sealed partial class AllowNullAttribute : System.Attribute
    {
        public AllowNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited = false)]
    public sealed partial class DisallowNullAttribute : System.Attribute
    {
        public DisallowNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Method, Inherited = false)]
    public sealed partial class DoesNotReturnAttribute : System.Attribute
    {
        public DoesNotReturnAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    public sealed partial class DoesNotReturnIfAttribute : System.Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;
        public bool ParameterValue { get; }
    }

    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, Inherited = false)]
    public sealed partial class MaybeNullAttribute : System.Attribute
    {
        public MaybeNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    public sealed partial class MaybeNullWhenAttribute : System.Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, Inherited = false)]
    public sealed partial class NotNullAttribute : System.Attribute
    {
        public NotNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    public sealed partial class NotNullIfNotNullAttribute : System.Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;
        public string ParameterName { get; }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    public sealed partial class NotNullWhenAttribute : System.Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }

}
#endif
