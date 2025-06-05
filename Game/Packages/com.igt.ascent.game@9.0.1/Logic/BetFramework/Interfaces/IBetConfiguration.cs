//-----------------------------------------------------------------------
// <copyright file = "IBetConfiguration.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Interfaces
{
    using System;
    using Exceptions;

    /// <summary>
    /// Interface for classes containing bet configuration.
    /// </summary>
    public interface IBetConfiguration
    {
        /// <summary>
        /// Validate a bet data against its own rules, and against the current committed credits,
        /// maximum wager, and minimum wager if the bet data's Commit flag is set. This does
        /// not check against the system.
        /// </summary>
        /// <param name="betData">Bet data to validate.</param>
        /// <returns>True if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        bool IsValid(IBetData betData);

        /// <summary>
        /// The maximum wager allowed by the system.
        /// </summary>
        long MaximumWager { get; }

        /// <summary>
        /// The minimum wager allowed by the system.
        /// </summary>
        long MinimumWager { get; }

        /// <summary>
        /// The minimum bet the button panel should be configured for.
        /// </summary>
        long ButtonPanelMinBet { get; }

        /// <summary>
        /// The number of items in the game that may be bet upon (e.g. paylines).
        /// </summary>
        /// <exception cref="UninitializedVariableException">Thrown if this property is read before getting set.</exception>
        long NumberOfBetItems { get; }

        /// <summary>
        /// The maximum wager allowed on each bet item (e.g. bet per payline).
        /// </summary>
        /// <exception cref="UninitializedVariableException">Thrown if this property is read before getting set.</exception>
        long MaximumWagerPerBetItem { get; }
    }
}
