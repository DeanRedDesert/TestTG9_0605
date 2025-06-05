// -----------------------------------------------------------------------
// <copyright file = "ItalyJackpotLevelConverters.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    /// <summary>
    /// Provides methods for converting between <see cref="ItalyJackpotLevel"/>
    /// and schema (F2L and F2E) values.
    /// </summary>
    public static class ItalyJackpotLevelConverters
    {
        /// <summary>
        /// Converts the public <see cref="ItalyJackpotLevel"/> to the schema jackpot level index.
        /// </summary>
        /// <param name="jackpotLevel">Value to convert.</param>
        /// <returns>Conversion result.</returns>
        public static int ToSchema(this ItalyJackpotLevel jackpotLevel)
        {
            int jackpotLevelIndex;

            switch(jackpotLevel)
            {
                case ItalyJackpotLevel.Mega:
                    jackpotLevelIndex = 0;
                    break;

                case ItalyJackpotLevel.Super:
                    jackpotLevelIndex = 1;
                    break;

                case ItalyJackpotLevel.Easy:
                    jackpotLevelIndex = 2;
                    break;

                default:
                    jackpotLevelIndex = -1;
                    break;
            }

            return jackpotLevelIndex;
        }

        /// <summary>
        /// Converts the schema jackpot level index to the public <see cref="ItalyJackpotLevel"/>.
        /// </summary>
        /// <param name="jackpotLevelIndex">Value to convert.</param>
        /// <returns>Conversion result.</returns>
        public static ItalyJackpotLevel ConvertFrom(int jackpotLevelIndex)
        {
            ItalyJackpotLevel jackpotLevel;

            switch(jackpotLevelIndex)
            {
                case 0:
                    jackpotLevel = ItalyJackpotLevel.Mega;
                    break;

                case 1:
                    jackpotLevel = ItalyJackpotLevel.Super;
                    break;

                case 2:
                    jackpotLevel = ItalyJackpotLevel.Easy;
                    break;

                default:
                    jackpotLevel = ItalyJackpotLevel.Invalid;
                    break;
            }

            return jackpotLevel;
        }
    }
}
