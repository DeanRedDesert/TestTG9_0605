//-----------------------------------------------------------------------
// <copyright file = "GameCyclePlayTypeConverters.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using Platform.Interfaces;
    using F2XWagerCatOutcome = Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay.WagerCatOutcome;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2X schema types.
    /// </summary>
    internal static class GameCyclePlayTypeConverters
    {
        /// <summary>
        /// Extension method to convert a <see cref="WagerCategoryOutcome"/> to
        /// an F2X <see cref="F2XWagerCatOutcome"/>.
        /// </summary>
        /// <param name="wagerCatOutcome">The public type of <see cref="WagerCategoryOutcome"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XWagerCatOutcome ToInternal(this WagerCategoryOutcome wagerCatOutcome)
        {
            checked
            {
                return new F2XWagerCatOutcome
                           {
                               WagerCatIndex = wagerCatOutcome.CategoryIndex,
                               WagerAmount = wagerCatOutcome.WagerAmount * wagerCatOutcome.Denomination
                           };
            }
        }
    }
}
