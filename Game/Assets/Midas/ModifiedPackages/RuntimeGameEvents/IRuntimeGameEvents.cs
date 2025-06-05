// -----------------------------------------------------------------------
// <copyright file = "IRuntimeGameEvents.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    /// <summary>
    /// This interface defines the APIs to communicate with the foundation for
    /// runtime game events.
    /// </summary>
    public interface IRuntimeGameEvents
    {
        /// <summary>
        /// Gets the latest runtime game events configuration.
        /// </summary>
        RuntimeGameEventsConfiguration RuntimeGameEventsConfiguration { get; }

        /// <summary>
        /// Sends notification that the game started waiting for generic player input to proceed.
        /// </summary>
        void WaitingForGenericInputStarted();

        /// <summary>
        /// Sends notification that the game finished waiting for generic player input to proceed.
        /// </summary>
        void WaitingForGenericInputEnded();

        /// <summary>
        /// Sends notification that a player chose a particular choice from a feature.
        /// </summary>
        /// <param name="playerChoiceIndex">
        /// The index of the player choice.
        /// </param>
        void PlayerChoiceUpdate(uint playerChoiceIndex);

        /// <summary>
        /// Sends notification that the client entered the denomination selection menu.
        /// </summary>
        void GameSelectionEntered();

        /// <summary>
        /// Sends notification that the client exited the denomination selection menu.
        /// </summary>
        void GameSelectionExited();

        /// <summary>
        /// Sends the current bet composition once a bet is committed.
        /// </summary>
        /// <param name="betPerLine">The bet per line amount.</param>
        /// <param name="selectedLines">The selected lines.</param>
        void GameBetMeterUpdate(long betPerLine, uint selectedLines);

        /// <summary>
        /// Sends information which used to keep track of the line/credit bet per line statistics.
        /// </summary>
        /// <param name="horizontalKey">
        /// The game bet horizontal key.
        /// </param>
        /// <param name="verticalKey">
        /// The game bet vertical key.
        /// </param>
        void GameBetMeterKeysUpdate(string horizontalKey, string verticalKey);
    }
}
