// -----------------------------------------------------------------------
// <copyright file = "IFlashPlayerClock.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;

    /// <summary>
    /// This interface defines the APIs to communicate with the foundation for displaying a flash player clock.
    /// </summary>
    public interface IFlashPlayerClock
    {
        /// <summary>
        /// Get the Flash Player Clock Properties.
        /// </summary>
        FlashPlayerClockProperties FlashPlayerClockProperties { get; }

        /// <summary>
        /// Get the configuration of the Flash Player Clock.
        /// </summary>
        FlashPlayerClockConfig FlashPlayerClockConfig { get; }

        /// <summary>
        /// Event to be triggered when the <see cref="FlashPlayerClockProperties"/> flag changes.
        /// </summary>
        event EventHandler<FlashPlayerClockPropertiesChangedEventArgs> FlashPlayerClockPropertiesChangedEvent;
    }
}