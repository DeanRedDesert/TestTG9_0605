// -----------------------------------------------------------------------
// <copyright file = "IGamePlayBehaviorConfigs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// The config data related to game play behaviors, whose values
    /// are usually determined by jurisdictional requirements.
    /// </summary>
    public interface IGamePlayBehaviorConfigs
    {
        /// <summary>
        /// Gets the default bet selection style.
        /// </summary>
        BetSelectionStyleInfo DefaultBetSelectionStyle { get; }

        /// <summary>
        /// Gets the flag indicating whether higher total bets must return a higher RTP than a lesser bet.
        /// </summary>
        bool RtpOrderedByBetRequired { get; }
    }
}