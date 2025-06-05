//-----------------------------------------------------------------------
// <copyright file = "IEmulatedCashoutService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Interface for the emulated cashout/service window.
    /// </summary>
    public interface IEmulatedCashoutService
    {
        /// <summary>
        /// Register as the emulated cashout/service window.
        /// </summary>
        bool RegisterAsEmulatedCashoutServiceWindow();

        /// <summary>
        /// Show the emulated cashout/service window.
        /// </summary>
        event EventHandler<EventArgs> ShowEvent;

        /// <summary>
        /// Hide the emulated cashout/service window.
        /// </summary>
        event EventHandler<EventArgs> HideEvent;

        /// <summary>
        /// Event handler for EGM culture change events.
        /// </summary>
        event EventHandler<CultureChangedEventArgs> CultureChangedEvent;

        /// <summary>
        /// Request the visibility state of the emulated cashout/service window.
        /// </summary>
        /// <param name="visible">Boolean flag indicating whether or not the emulated cashout/service window should be visible.</param>
        /// <returns>True if the request is successful.</returns>
        bool SetEmulatedCashoutServiceVisible(bool visible);
    }
}
