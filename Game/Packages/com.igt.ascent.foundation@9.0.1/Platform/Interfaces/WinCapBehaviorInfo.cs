//-----------------------------------------------------------------------
// <copyright file = "WinCapBehaviorInfo.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// The WinCapBehaviorInfo represents data structure of Win Cap feature.
    /// </summary>
    public struct WinCapBehaviorInfo
    {
        /// <summary>
        /// The win cap limit behavior for this game.
        /// </summary>
        public WinCapBehavior WinCapBehavior { get; }

        /// <summary>
        /// The win cap limit for this game if behavior is set to FixedWinCapAmount, in base units.
        /// </summary>
        public long WinCapAmount { get; }

        /// <summary>
        /// A multiplier value if WinCapBehavior is MultipliedByCurrentBet or MultipliedByMaxBet.
        /// </summary>
        public uint WinCapMultiplier { get; }

        /// <summary>
        /// The WinCapBehaviorInfo struct constructor.
        /// </summary>
        /// <param name="winCapBehavior">To initialize the win cap limit behavior.</param>
        /// <param name="winCapAmount">To initialize the win cap limit, in base units.</param>
        /// <param name="winCapMultiplier">To initialize the multiplier value.</param>
        public WinCapBehaviorInfo(WinCapBehavior winCapBehavior, long winCapAmount, uint winCapMultiplier) : this()
        {
            WinCapBehavior = winCapBehavior;
            WinCapAmount = winCapAmount;
            WinCapMultiplier = winCapMultiplier;
        }
    }
}
