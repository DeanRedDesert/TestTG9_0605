//-----------------------------------------------------------------------
// <copyright file = "IPlayerSessionParameters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the APIs to communicate with the foundation for player session parameters resetting.
    /// </summary>
    public interface IPlayerSessionParameters
    {
        /// <summary>
        /// Event raised when current player session parameters for resetting are changed.
        /// </summary>
        event EventHandler<CurrentResetParametersChangedEventArgs> CurrentResetParametersChangedEvent;

        /// <summary>
        /// Gets the flag indicating if or not player session parameters reset is enabled by configuration.
        /// </summary>
        bool IsPlayerSessionParameterResetEnabled { get; }

        /// <summary>
        /// Gets the pending list of player session parameters to reset.
        /// </summary>
        /// <remarks>
        /// This pending list of player session parameters is only available for game in play mode.
        /// </remarks>
        IList<PlayerSessionParameterType> PendingParametersToReset { get; }

        /// <summary>
        /// Notifies the foundation a list of player session parameters that have been reset by this client.
        /// </summary>
        /// <param name="parametersBeingReset">
        /// A list of player session parameters that have been reset by the client.
        /// </param>
        /// <remarks>
        /// This interface call is only available for game in play mode.
        /// </remarks>
        void ReportParametersBeingReset(IEnumerable<PlayerSessionParameterType> parametersBeingReset);
    }
}
