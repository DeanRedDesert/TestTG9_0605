// -----------------------------------------------------------------------
// <copyright file = "PlayerClockSessionActiveChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event indicating that the Flash Player Clock Properties have changed.
    /// </summary>
    public class FlashPlayerClockPropertiesChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Get if the PlayerClock Session is active.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? PlayerClockSessionActive { get; }

        /// <summary>
        /// Initializes an instance of <see cref="FlashPlayerClockPropertiesChangedEventArgs"/>.
        /// </summary>
        /// <param name="playerClockSessionActive">
        /// Flag indicating if the Player Clock Session is active.
        /// </param>
        public FlashPlayerClockPropertiesChangedEventArgs(bool? playerClockSessionActive)
        {
            PlayerClockSessionActive = playerClockSessionActive;
        }
    }
}