//-----------------------------------------------------------------------
// <copyright file = "IGameLibDemo.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// GameLib interface which contains functionality that simulates
    /// the operations outside Game Lib.
    /// </summary>
    public interface IGameLibDemo : ISimulateGameModeControl
    {
        /// <summary>
        /// Add money amount to Player Bank Meter.  Conditional to DEMO build
        /// </summary>
        /// <param name="value">Amount to add, in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination for the value to add.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when value is less than 0, or denomination is less than 1.
        /// </exception>
        void InsertMoney(long value, long denomination);

        /// <summary>
        /// Enable/Disable show mode.
        /// </summary>
        /// <param name="showMode">Flag indicating if show mode should be enabled.</param>
        /// <param name="showEnvironment">The show environment.  Will be ignored if <paramref name="showMode"/> is false.</param>
        void SetShowMode(bool showMode, ShowEnvironment showEnvironment = ShowEnvironment.Development);

        /// <summary>
        /// Set the display control status to hidden.
        /// </summary>
        void SetDisplayControlHidden();

        /// <summary>
        /// Set the display control status to normal.
        /// </summary>
        void SetDisplayControlNormal();

        /// <summary>
        /// Set the display control status to suspended.
        /// </summary>
        void SetDisplayControlSuspended();

        /// <summary>
        /// Configure auto credits. This function configures the automatic addition of credits to the GameLib.
        /// </summary>
        /// <param name="creditsToAdd">
        /// The number of credits to add when the available credits fall below the threshold.
        /// If it is 0, the auto credits will be disabled.
        /// </param>
        /// <param name="creditThreshold">
        /// Threshold at which to add more credits.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="creditsToAdd"/> or <paramref name="creditThreshold"/> is negative.
        /// </exception>
        /// <remarks>Setting <paramref name="creditsToAdd"/> to 0 will disable auto credits.</remarks>
        void SetAutoCredits(long creditsToAdd, long creditThreshold);

        /// <summary>
        /// Get the list of theme names and their lists of denominations available for the player to pick.
        /// </summary>
        /// <returns>The list of theme names and denominations available for the player to pick.</returns>
        IDictionary<string, IList<long>> GetAvailableThemes();

        /// <summary>
        /// Request to change to a new theme with the specified denomination.
        /// This is used by the simulated theme selection menu to respond to the
        /// player's pick of a new theme.
        /// </summary>
        /// <param name="newTheme">The name of the new theme.</param>
        /// <param name="newDenomination">The denomination to use with the new theme.</param>
        void RequestThemeChange(string newTheme, long newDenomination);

        /// <summary>
        /// Disable Ancillary Game offer.
        /// </summary>
        /// <remarks>
        /// This interface is added to simulate the foundation to disable the ancillary game offer,
        /// for testing purpose in standalone environment.
        /// </remarks>>
        void DisableAncillaryOffer();

        /// <summary>
        /// Attempts to trigger the Force Game-Completion condition with the specified parameters.
        /// The Force Game-Completion extension interface is required.
        /// </summary>
        /// <param name="warningTime">The time at which point the Game may guide the player to cash-out.</param>
        /// <param name="finishTime">The time at which point the Game is to take control and auto-finish
        /// the game-cycle.</param>
        /// <param name="messages">A dictionary of localized messages to display to the player keyed
        /// by culture.</param>
        void TriggerForceGameCompletion(DateTime warningTime, DateTime finishTime,
                                        Dictionary<string, string> messages);

        /// <summary>
        /// Attempts to clear any active Force Game-Completion condition.
        /// The Force Game-Completion extension interface is required.
        /// </summary>
        void ClearForceGameCompletion();

        /// <summary>
        /// Gets whether host initiated auto play is enabled.
        /// </summary>
        bool IsHostAutoPlayEnabled();

        /// <summary>
        /// Gets whether auto play is current in progress.
        /// </summary>
        bool IsAutoPlayOn();

        /// <summary>
        /// Requests to turn on AutoPlay via Host.
        /// This is used by the simulated HostInitiatedAutoPlay selection menu to respond to the
        /// player's wanting to start the Host AutoPlay.
        /// </summary>
        void RequestHostAutoPLayOn();

        /// <summary>
        /// Requests to turn off AutoPlay via Host.
        /// This is used by the simulated HostInitiatedAutoPlay selection menu to respond to the
        /// player's wanting to stop the Host AutoPlay.
        /// </summary>
        void RequestHostAutoPLayOff();

        /// <summary>
        /// Enforces the subsequent denomination change requests to fail.
        /// </summary>
        /// <param name="enforce">
        /// True to put the enforcement in effect; False to clear it.
        /// </param>
        void EnforceDenominationChangeFail(bool enforce);

        /// <summary>
        /// Gets whether denomination change requests are enforced to fail.
        /// </summary>
        /// <returns>
        /// True if denomination change requests are enforced to fail; False otherwise.
        /// </returns>
        bool IsDenominationChangeFailEnforced();

        /// <summary>
        /// Enforces the subsequent commit game cycle calls to fail.
        /// </summary>
        /// <param name="enforce">
        /// True to put the enforcement in effect; False to clear it.
        /// </param>
        void EnforceCommitGameCycleFail(bool enforce);

        /// <summary>
        /// Gets whether commit game cycle calls are enforced to fail.
        /// </summary>
        /// <returns>
        /// True if commit bet calls are enforced to fail; False otherwise.
        /// </returns>
        bool IsCommitGameCycleFailEnforced();

        /// <summary>
        /// Enforces the subsequent enroll game cycle calls to fail.
        /// </summary>
        /// <param name="enforce">
        /// True to put the enforcement in effect; False to clear it.
        /// </param>
        void EnforceEnrollGameCycleFail(bool enforce);

        /// <summary>
        /// Gets whether enroll game cycle calls are enforced to fail.
        /// </summary>
        /// <returns>
        /// True if enroll game cycle calls are enforced to fail; False otherwise.
        /// </returns>
        bool IsEnrollGameCycleFailEnforced();
    }
}
