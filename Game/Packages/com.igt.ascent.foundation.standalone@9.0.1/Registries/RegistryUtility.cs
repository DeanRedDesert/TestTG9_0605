//-----------------------------------------------------------------------
// <copyright file = "RegistryUtility.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Linq;
    using F2LRegTypesVerTip = Core.Registries.Internal.F2L.F2LRegistryTypeVer2;

    /// <summary>
    /// This class defines utility methods for the game registries.
    /// </summary>
    internal static class RegistryUtility
    {
        /// <summary>
        /// Validate the bet limit value from a bet limit object.
        /// </summary>
        /// <param name="betLimit">The bet limit object from which the value is validated.</param>
        /// <exception cref="GameRegistryException">
        /// Thrown when the bet limit object contains invalid data.
        /// </exception>
        public static void ValidateBetLimitValue(F2LRegTypesVerTip.MaxBetType betLimit)
        {
            var valuePool = betLimit.ValuePool.Item;

            if(betLimit.Value.Any(betLimitValue =>
                    betLimitValue != null && !GetSupportedBetValuePool(valuePool).Contains((long)betLimitValue.Value)))
            {
                throw new GameRegistryException("There is bet limit value which is not in the bet value pool.");
            }
        }

        /// <summary>
        /// Convert and return a value pool from <see cref="F2LRegTypesVerTip.MaxBetTypeValuePoolList"/>  or
        /// <see cref="F2LRegTypesVerTip.MaxBetTypeValuePoolRange"/>.
        /// </summary>
        /// <param name="valuePool">
        /// Specify the value pool of either <see cref="F2LRegTypesVerTip.MaxBetTypeValuePoolList"/> or
        /// <see cref="F2LRegTypesVerTip.MaxBetTypeValuePoolRange"/>.
        /// </param>
        /// <returns>
        /// The value pool of supported values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="valuePool"/> passed in is null.
        /// </exception>
        public static IValuePool<long> GetSupportedBetValuePool(object valuePool)
        {
            switch(valuePool)
            {
                case null:
                    throw new ArgumentNullException(nameof(valuePool), "Parameters may not be null.");
                case F2LRegTypesVerTip.MaxBetTypeValuePoolList valueList:
                    return new ValueList<long>(valueList.Enumeration.ConvertAll(i => (long)i));
                case F2LRegTypesVerTip.MaxBetTypeValuePoolRange valueRange:
                    return new ValueRange<long>((long)valueRange.Min, (long)valueRange.Max);
                default:
                    return null;
            }
        }
    }
}