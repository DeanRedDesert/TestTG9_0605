//-----------------------------------------------------------------------
// <copyright file = "IService.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for requesting service.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Request service.
        /// </summary>
        void RequestService();

        /// <summary>
        /// Request cash out. The foundation will treat this as though a physical cash out button was pressed.
        /// This means, if pressing a physical cash out button would display the show gaff, sending this new message would do the same.
        /// As a result, a CSI button press event will be sent to the client.
        /// </summary>
        void RequestCashOut();

        /// <summary>
        /// Register for "Prompt Player On Cashout" config item changed events.
        /// </summary>
        /// <returns>
        /// True if the registration was successful,
        /// false if the registration was not successful or the API is not supported by the current target.
        /// </returns>
        bool RegisterForPromptPlayerOnCashoutConfigItemChangedEvents();

        /// <summary>
        /// Unregister for "Prompt Player On Cashout" config item changed events.
        /// </summary>
        /// <returns>
        /// True if the unregistration was successful,
        /// false if the unregistration was not successful or the API is not supported by the current target.
        /// </returns>
        bool UnregisterForPromptPlayerOnCashoutConfigItemChangedEvents();

        /// <summary>
        /// Event handler for "Prompt Player On Cashout" config item changed events.
        /// </summary>
        event EventHandler<PromptPlayerOnCashoutConfigItemChangedEventArgs> PromptPlayerOnCashoutConfigItemChangedEvent;

        /// <summary>
        /// Get the "Prompt Player On Cashout" config item value.
        /// </summary>
        /// <returns>
        /// The "Prompt Player On Cashout" config item value. This value will be false if the API is not supported by the current target.
        /// </returns>
        bool GetPromptPlayerOnCashoutConfigItemValue();

        /// <summary>
        /// Get the buttons that the EGM requires to be emulated.
        /// </summary>
        /// <returns>
        /// The buttons that the EGM requires to be emulated. This list will be empty if the API is not supported.
        /// </returns>
        IReadOnlyList<EmulatableButton> GetTheButtonsThatTheEgmRequiresToBeEmulated();

        /// <summary>
        /// Register for "Emulated Service Button Enabled" config item changed events.
        /// </summary>
        /// <returns>
        /// True if the registration was successful,
        /// false if the registration was not successful or the API is not supported by the current target.
        /// </returns>
        bool RegisterForEmulatedServiceButtonEnabledConfigItemChangedEvents();

        /// <summary>
        /// Unregister for "Emulated Service Button Enabled" config item changed events.
        /// </summary>
        /// <returns>
        /// True if the unregistration was successful,
        /// false if the unregistration was not successful or the API is not supported by the current target.
        /// </returns>
        bool UnregisterForEmulatedServiceButtonEnabledConfigItemChangedEvents();

        /// <summary>
        /// Event handler for "Emulated Service Button Enabled" config item changed events.
        /// </summary>
        event EventHandler<EmulatedServiceButtonEnabledConfigItemChangedEventArgs> EmulatedServiceButtonEnabledConfigItemChangedEvent;

        /// <summary>
        /// Gets the player call attendant state.
        /// </summary>
        /// <remarks>
        /// Depending on cabinet and configurations, pressing the service button could trigger
        /// changes to "PlayerCallAttendantState" and/or "PlayerServiceRequestState".
        /// </remarks>
        /// <returns>Flag indicating whether the state is on.</returns>
        bool GetPlayerCallAttendantState();

        /// <summary>
        /// Registers for the <see cref="PlayerCallAttendantStateChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// True if the registration was successful;
        /// False if the registration was not successful or the API is not supported by the current foundation target.
        /// </returns>
        bool RegisterPlayerCallAttendantStateEvent();

        /// <summary>
        /// Unregisters for the <see cref="PlayerCallAttendantStateChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// True if the unregistration was successful;
        /// False if the unregistration was not successful or the API is not supported by the current foundation target.
        /// </returns>
        bool UnregisterPlayerCallAttendantStateEvent();

        /// <summary>
        /// Events occur when the player call attendant state changes.
        /// </summary>
        event EventHandler<PlayerCallAttendantStateChangedEventArgs> PlayerCallAttendantStateChangedEvent;

        /// <summary>
        /// Gets the player service request state.
        /// </summary>
        /// <remarks>
        /// Depending on cabinet and configurations, pressing the service button could trigger
        /// changes to "PlayerCallAttendantState" and/or "PlayerServiceRequestState".
        /// </remarks>
        /// <returns>Flag indicating whether the state is on.</returns>
        bool GetPlayerServiceRequestState();

        /// <summary>
        /// Registers for the <see cref="PlayerServiceRequestStateChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// True is the registration was successful;
        /// False if the registration was not successful or the API is not supported by the current foundation target.
        /// </returns>
        bool RegisterPlayerServiceRequestStateEvent();

        /// <summary>
        /// Unregisters for the <see cref="PlayerServiceRequestStateChangedEvent"/>.
        /// </summary>
        /// <returns>
        /// True is the unregistration was successful;
        /// False if the unregistration was not successful or the API is not supported by the current foundation target.
        /// </returns>
        bool UnregisterPlayerServiceRequestStateEvent();

        /// <summary>
        /// Events occur when the player service request state changes.
        /// </summary>
        event EventHandler<PlayerServiceRequestStateChangedEventArgs> PlayerServiceRequestStateChangedEvent;
    }
}
