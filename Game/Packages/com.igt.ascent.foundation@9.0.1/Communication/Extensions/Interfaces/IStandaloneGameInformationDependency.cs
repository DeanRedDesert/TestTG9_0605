//-----------------------------------------------------------------------
// <copyright file = "IStandaloneGameInformationDependency.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Interface for accessing information for Standalone interface extensions.
    /// </summary>
    /// <remarks>
    /// Interface should be extended as needed by interface extensions.
    /// </remarks>
    public interface IStandaloneGameInformationDependency
    {
        /// <summary>
        /// Get the G2S theme identifier for the current theme.
        /// </summary>
        /// <returns>The theme identifier for the current theme.</returns>
        string GetG2SThemeId();

        /// <summary>
        /// The mode of the game.
        /// </summary>
        GameMode GameContextMode { get; }

        /// <summary>
        /// The flag indicating whether the game is in tournament mode.
        /// </summary>
        bool IsTournamentMode { get; }

        /// <summary>
        /// The tournament session type.
        /// </summary>
        TournamentSessionType TournamentSessionType { get; }

        /// <summary>
        /// Gets the game denomination in base units.
        /// </summary>
        long GameDenomination { get; }

        /// <summary>
        /// Gets the maximum bet for the current game in units of credit.
        /// </summary>
        long MaxBet { get; }

        /// <summary>
        /// The tournament countdown duration.
        /// </summary>
        int TournamentCountdownDuration { get; }

        /// <summary>
        /// The duration of the tournament session.
        /// </summary>
        int TournamentPlayDuration { get; }

        /// <summary>
        /// The starting credits of the tournament session.
        /// </summary>
        long InitialCredits { get; }

        /// <summary>
        /// Flag indicating if current game environment supports banked credit functionality.
        /// </summary>
        bool IsBankCreditEnvironment { get; }

        /// <summary>
        /// The host name used to connect to the STOMP broker.
        /// </summary>
        string StompBrokerHostname { get; }

        /// <summary>
        /// The port number used to connect to the STOMP broker.
        /// </summary>
        int StompBrokerPort { get; }

        /// <summary>
        /// The version number that indicates the specific foundation behavior of the STOMP server
        /// and G2S implementation.
        /// </summary>
        Version StompVersion { get; }
        
        /// <summary>
        /// Gets the list of enabled payvars of current theme for the specified denomination list.
        /// </summary>
        /// <param name="denominations">The list of denominations.</param>
        /// <returns>
        /// The list of enabled payvars for the denomination list.
        /// The returned pyavar information list may contain less items than the input denomination list in the case
        /// when not all the input denominations are enabled.
        /// </returns>
        ReadOnlyCollection<PayvarInformation> GetEnabledPayvars(IEnumerable<long> denominations);
    }
}
