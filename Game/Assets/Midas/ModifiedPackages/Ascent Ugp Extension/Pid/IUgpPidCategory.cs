//-----------------------------------------------------------------------
// <copyright file = "IUgpPidCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    /// <summary>
    /// Defines an interface that allows a package to retrieve PID (Player Information Display) information.
    /// </summary>
    public interface IUgpPidCategory
    {
        /// <summary>
        /// Gets the current status of the service request.
        /// </summary>
        bool IsServiceRequested { get; }

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
        PidSessionDataInfo GetSessionData();

        /// <summary>
        /// Gets the PID configuration.
        /// </summary>
        /// <returns>
        /// The PID configuration.
        /// </returns>
        PidConfigurationInfo GetPidConfiguration();

        /// <summary>
        /// Sends notification that the player is entering or exiting the player information screens.
        /// </summary>
        /// <param name='currentStatus'>Specify the current status.</param>
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