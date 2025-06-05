//-----------------------------------------------------------------------
// <copyright file = "IUgpPid.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;

    /// <summary>
    /// Defines an interface that allows a package to retrieve Pid information.
    /// </summary>
    public interface IUgpPid
    {
        /// <summary>
        /// Gets the current status of the service request.
        /// </summary>
        bool IsServiceRequested { get; }

        /// <summary>
        /// Event raised when receiving message of force PID activation.
        /// </summary>
        event EventHandler<PidActivationEventArgs> PidActivated;

        /// <summary>
        /// Event raised when the Pid service requested has changed.
        /// </summary>
        event EventHandler<PidServiceRequestedChangedEventArgs> IsServiceRequestedChanged;

        /// <summary>
        /// Event raised when the Pid configuration has changed.
        /// </summary>
        event EventHandler<PidConfigurationChangedEventArgs> PidConfigurationChanged;

        /// <summary>
        /// Starts the PID Tracking process.
        /// </summary>
        void StartTracking();

        /// <summary>
        /// Stops the PID Tracking process.
        /// </summary>
        void StopTracking();

        /// <summary>
        /// Gets the PID related session data that is tracked.
        /// </summary>
        /// <returns>
        /// The PID session data.
        /// </returns>
        PidSessionData GetSessionData();

        /// <summary>
        /// Gets the PID configuration.
        /// </summary>
        /// <returns>
        /// The PID configuration.
        /// </returns>
        PidConfiguration GetPidConfiguration();

        /// <summary>
        /// Sends notification that the player is entering or exiting the player information screens.
        /// </summary>
        /// <param name='currentStatus'>Specify the current activation status.</param>
        void ActivationStatusChanged(bool currentStatus);

        /// <summary>
        /// Sends notification that the player is entering the game information screen.
        /// </summary>
        void GameInformationScreenEntered();

        /// <summary>
        /// Sends notification that the player is entering the session information screen.
        /// </summary>
        void SessionInformationScreenEntered();

        /// <summary>
        /// Sends notification that the player is requesting attendant service.
        /// </summary>
        void AttendantServiceRequested();

        /// <summary>
        /// Requests a force payout.
        /// </summary>
        void RequestForcePayout();
    }
}
