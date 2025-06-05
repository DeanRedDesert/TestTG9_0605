//-----------------------------------------------------------------------
// <copyright file = "MidGameMaxBetBehavior.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The enumeration defines the type for telling max bet behaviors to be applied to mid-game bets.
    /// </summary>
    [Serializable]
    public enum MidGameMaxBetBehavior
    {
        /// <summary>
        /// Indicates that the total of all bets (mid-game plus starting bet) during a game cycle must
        /// not exceed the maximum amount specified by the <see cref="MachineWideBetConstraints.MidGameBetLimits"/>.
        /// </summary>
        LimitByTotalOfStartingPlusMidGame,

        /// <summary>
        /// Indicates that the total of the mid-game bets during a game cycle must not exceed the maximum
        /// amount specified by the <see cref="MachineWideBetConstraints.MidGameBetLimits"/>;
        /// the starting bet is excluded from the total.
        /// </summary>
        LimitByTotalOfMidGame
    }
}
