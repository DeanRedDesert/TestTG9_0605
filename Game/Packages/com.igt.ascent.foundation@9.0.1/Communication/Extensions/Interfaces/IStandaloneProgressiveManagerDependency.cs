// -----------------------------------------------------------------------
// <copyright file = "IStandaloneProgressiveManagerDependency.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines the standalone progressive manager API.
    /// </summary>
    public interface IStandaloneProgressiveManagerDependency
    {
        /// <summary>
        /// Get the progressive settings for currently linked game levels.
        /// </summary>
        /// <returns>
        /// A dictionary of progressive settings. See also <see cref="IProgressiveSettings"/>.
        /// The dictionary is keyed by the game level id.
        /// The dictionary contains only linked game levels.
        /// The dictionary is empty if no game level is linked.
        /// </returns>
        IDictionary<int, IProgressiveSettings> GetCurrentLinkedProgressiveSettings();

        /// <summary>
        /// Get a dictionary of progressive broadcast information as an interface, keyed by game levels.
        /// </summary>
        /// <returns>A dictionary of <see cref="IProgressiveBroadcastData"/> keyed by game levels.</returns>
        IReadOnlyDictionary<int, IProgressiveBroadcastData> GetIProgressiveBroadcastData();

        /// <summary>
        /// Adds a contribution amount to the given event-based progressive level.
        /// </summary>
        /// <remarks>
        /// The contribution amount is in base units.  The value is calculated by dividing <paramref name="amountNumerator"/>
        /// by <paramref name="amountDenominator"/>.  The result could contain a fractional amount of the base unit.
        ///
        /// Assuming currency is in US dollars, if <paramref name="amountNumerator"/> is 99999,
        /// <paramref name="amountDenominator"/> is 30000, the contribution amount would be
        /// 99999/30000 = 3.3333 cents.
        ///
        /// Note: When a level is configured to be "event-based", only contributions specifically declared by the game-client
        /// are accumulated by the controller; contributions are not accumulated based on a percentage of the bets/wagers.
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
        /// when <paramref name="amountDenominator"/> is 0 or negative.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="gameLevel"/> is not an "event-based" progressive level.
        /// </exception>
        void AddEventBasedContribution(int gameLevel, long amountNumerator, long amountDenominator = 1);

        /// <summary>
        /// Calculate the contribution of a player or background network bet and add it to the
        /// progressive amount for all available progressive game levels.
        /// </summary>
        /// <param name="bet">
        /// The bet, in units of denomination passed in.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet.
        /// </param>
        /// <param name="saveToCriticalData">
        /// Flag indicating if this new amount should be saved to critical data.
        /// </param>
        void ContributeToAllProgressives(long bet, long denomination, bool saveToCriticalData);

        /// <summary>
        /// Calculate the contribution of a player or background network bet and add it to the
        /// progressive amount for all available event based progressive game levels.
        /// </summary>
        /// <param name="bet">
        /// The bet, in units of denomination passed in.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet.
        /// </param>
        /// <param name="saveToCriticalData">
        /// Flag indicating if this new amount should be saved to critical data.
        /// </param>
        void ContributeToAllEventBasedProgressives(long bet, long denomination, bool saveToCriticalData);
    }
}
