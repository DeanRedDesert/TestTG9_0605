// -----------------------------------------------------------------------
// <copyright file = "MoneyCalc.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;

    /// <summary>
    /// Static helper class to help with money related calculations.
    /// </summary>
    internal static class MoneyCalc
    {
        /// <summary>
        /// Converts the passed value from base units to units of the game denomination.
        /// </summary>
        /// <param name="valueInBaseUnits">
        /// The value to be converted, in base units.
        /// </param>
        /// <param name="gameDenomination">
        /// The game denomination to be used for conversion.
        /// </param>
        /// <param name="roundedUp">
        /// The flag indicating whether the fractional credits should be rounded up.
        /// This parameter is optional.  If not specified, it defaults to false.
        /// </param>
        /// <returns>The converted value in units of game denomination.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="valueInBaseUnits"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="gameDenomination"/> is less than or equal to 0.
        /// </exception>
        public static long ConvertToCredits(long valueInBaseUnits, long gameDenomination, bool roundedUp = false)
        {
            if(valueInBaseUnits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valueInBaseUnits), $"Value {valueInBaseUnits} is less than 0.");
            }

            if(gameDenomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gameDenomination),
                    "gameDenomination may not be less than or equal to 0.");
            }

            var quotient = Math.DivRem(valueInBaseUnits, gameDenomination, out var remainder);
            checked
            {
                return roundedUp && remainder != 0 ? quotient + 1 : quotient;
            }
        }

        /// <summary>
        /// Converts the passed value from units of the game denomination to base units.
        /// </summary>
        /// <param name="valueInCredits">
        /// The value to be converted, in units of game denomination.
        /// </param>
        /// <param name="gameDenomination">
        /// The game denomination to be used for conversion.
        /// </param>
        /// <returns>The converted value in base units.</returns>
        public static long ConvertToBaseUnits(long valueInCredits, long gameDenomination)
        {
            return checked(valueInCredits * gameDenomination);
        }
    }
}