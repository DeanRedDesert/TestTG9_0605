//-----------------------------------------------------------------------
// <copyright file = "CreditMeterDisplayBehaviorConverters.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using F2L.Schemas.Internal;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Converts CreditMeterDisplayBehaviorType to CreditMeterDisplayBehaviorMode.
    /// </summary>
    internal static class CreditMeterDisplayBehaviorConverters
    {
        /// <summary>
        /// The conversion table.
        /// </summary>
        private static readonly Dictionary<CreditMeterDisplayBehaviorType, CreditMeterDisplayBehaviorMode> Conversion
            = new Dictionary<CreditMeterDisplayBehaviorType, CreditMeterDisplayBehaviorMode>
        {
            { CreditMeterDisplayBehaviorType.Invalid, CreditMeterDisplayBehaviorMode.Invalid },
            { CreditMeterDisplayBehaviorType.PlayerSelectableDefaultCredits, CreditMeterDisplayBehaviorMode.PlayerSelectableDefaultCredits },
            { CreditMeterDisplayBehaviorType.PlayerSelectableDefaultCurrency, CreditMeterDisplayBehaviorMode.PlayerSelectableDefaultCurrency },
            { CreditMeterDisplayBehaviorType.AlwaysCurrency, CreditMeterDisplayBehaviorMode.AlwaysCurrency },
        };

        /// <summary>
        /// Converts a CreditMeterDisplayBehaviorMode to CreditMeterDisplayBehaviorMode.
        /// </summary>
        /// <param name="mode">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static CreditMeterDisplayBehaviorMode ConvertFromF2LSchema(this CreditMeterDisplayBehaviorType mode)
        {
            return Conversion.ContainsKey(mode)
                       ? Conversion[mode]
                       : CreditMeterDisplayBehaviorMode.Invalid;
        }
    }
}
