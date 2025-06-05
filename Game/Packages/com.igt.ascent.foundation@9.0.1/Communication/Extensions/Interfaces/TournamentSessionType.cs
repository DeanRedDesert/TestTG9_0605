//-----------------------------------------------------------------------
// <copyright file = "TournamentSessionType.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// The type of tournament play in a tournament session.
    /// </summary>
    public enum TournamentSessionType
    {
        /// <summary>
        /// An invalid tournament session type.
        /// </summary>
        Invalid,

        /// <summary>
        /// A time-based tournament, where the tournament session ends when a pre-defined
        /// period of time has elapsed.
        /// </summary>
        Timer,

        /// <summary>
        /// A credits-based tournament, where the tournament session ends when the available
        /// credits for play is less than the lowest allowable wager.
        /// </summary>
        Credits,

        /// <summary>
        /// A combination of both time-based and credits-based tournament.
        /// </summary>
        TimedCredits,
    }
}
