//-----------------------------------------------------------------------
// <copyright file = "ISystemProgressiveController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Logic.ProgressiveController;

    /// <summary>
    /// This interface is used for standalone game lib's internal
    /// progressive controllers that simulate the behavior of
    /// system controlled progressives.
    /// </summary>
    internal interface ISystemProgressiveController : IProgressiveController
    {
        // Reload controller levels from the critical data.
        // Adjust the level's displayable amount according
        // the level's configuration if needed.
        void ReloadControllerLevels();

        /// <summary>
        /// Clear the level mapping of the controller.
        /// </summary>
        void ClearLevelMapping();

        /// <summary>
        /// Set the level mapping of the controller.
        /// </summary>
        /// <param name="mappingList">List of mappings of game level to controller level.</param>
        void SetLevelMapping(IDictionary<int, int> mappingList);

        /// <summary>
        /// Get the broadcast data of a specific controller level.
        /// </summary>
        /// <remarks>
        /// This gets the progressive data for a controller level, even
        /// if it is not currently linked to any game level.
        /// This is needed when getting progressive data for all available
        /// denominations for a theme.
        /// </remarks>
        /// <param name="controllerLevel">The controller level in query.</param>
        /// <returns>The broadcast data of the controller level.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="controllerLevel"/> does not exist in the controller.
        /// </exception>
        ProgressiveBroadcastData GetProgressiveBroadcastDataForControllerLevel(int controllerLevel);

        /// <summary>
        /// Add a contribution amount to the given event-based progressive game level.
        /// Do nothing if the level is not event-based.
        /// </summary>
        /// <remarks>
        /// The contribution amount is in base units.  The value is calculated by dividing <paramref name="amountNumerator"/>
        /// by <paramref name="amountDenominator"/>.  The result could contain a fractional amount of the base unit.
        /// 
        /// Assuming currency is denoted in US dollars, if <paramref name="amountNumerator"/> is 99999,
        /// <paramref name="amountDenominator"/> is 30000, then the contribution amount would be
        /// 99999/30000 = 3.3333 cents.
        /// </remarks>
        /// <param name="gameLevel">
        /// The level to add the contribution amount.
        /// </param>
        /// <param name="amountNumerator">
        /// The numerator of the contribution amount.
        /// </param>
        /// <param name="amountDenominator">
        /// The denominator of the contribution amount, needed for specifying a fractional amount.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amountNumerator"/> is negative, or
        /// <paramref name="amountDenominator"/> is 0 or negative.
        /// </exception>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        void ContributeToEventBasedProgressive(int gameLevel, long amountNumerator, long amountDenominator);
    }
}
