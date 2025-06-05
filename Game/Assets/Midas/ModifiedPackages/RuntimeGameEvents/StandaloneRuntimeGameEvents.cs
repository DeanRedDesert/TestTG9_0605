// -----------------------------------------------------------------------
// <copyright file = "StandaloneRuntimeGameEvents.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    using Interfaces;

    /// <summary>
    /// Implementation of the runtime game events interface extension
    /// that is to be used in standalone mode.
    /// </summary>
    internal class StandaloneRuntimeGameEvents : IRuntimeGameEvents, IInterfaceExtension
    {
        #region Implementation of IRuntimeGameEvents

        /// <inheritdoc />
        public RuntimeGameEventsConfiguration RuntimeGameEventsConfiguration => new RuntimeGameEventsConfiguration();

        /// <inheritdoc />
        public void WaitingForGenericInputStarted()
        {
        }

        /// <inheritdoc />
        public void WaitingForGenericInputEnded()
        {
        }

        /// <inheritdoc />
        public void PlayerChoiceUpdate(uint playerChoiceIndex)
        {
        }

        /// <inheritdoc />
        public void GameSelectionEntered()
        {
        }

        /// <inheritdoc />
        public void GameSelectionExited()
        {
        }

        /// <inheritdoc />
        public void GameBetMeterUpdate(long betPerLine, uint selectedLines)
        {
        }

        /// <inheritdoc />
        public void GameBetMeterKeysUpdate(string horizontalKey, string verticalKey)
        {
        }

        #endregion
    }
}
