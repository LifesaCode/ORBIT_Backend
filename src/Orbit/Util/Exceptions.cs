using System;
using System.Globalization;
using System.Resources;
using Orbit.Properties;

namespace Orbit.Util
{
    internal static class Exceptions
    {
        // This is an example of how way we can design with multiple cultures/languages in mind
        public static Exception NoDataFound()
        {
            return new InvalidOperationException(Resources.ERR_NO_DATA_RETRIEVED);
        }
    }
}