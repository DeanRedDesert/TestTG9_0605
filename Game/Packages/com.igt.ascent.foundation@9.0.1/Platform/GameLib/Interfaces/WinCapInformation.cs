//-----------------------------------------------------------------------
// <copyright file = "WinCapInformation.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This struct represents win cap behavior and related win cap values.
    /// </summary>
    /// <remarks>
    /// There is no assumption on the relationship between game/progressive/total win capping value.
    /// It would be up to the game to decide which win (progressive or game) would need to be scaled
    /// to make sure none of these three win capping values is exceeded.
    /// </remarks>
    public struct WinCapInformation
    {
        #region Public Properties

        /// <summary>
        /// Gets the configured win cap behavior.
        /// </summary>
        public WinCapBehaviorInfo GameWinCapBehaviorInfo { get; }

        /// <summary>
        /// Gets the progressive win cap limit, and 0 indicates the jurisdiction does not support it.
        /// </summary>
        public long ProgressiveWinCapLimit { get; }

        /// <summary>
        /// Gets the total win cap limit, and 0 indicates the jurisdiction does not support it.
        /// </summary>
        public long TotalWinCapLimit { get; }

        #endregion

        /// <summary>
        /// Constructs an instance of this struct.
        /// </summary>
        /// <param name="gameWinCapBehaviorInfo">
        /// The win cap behavior information getting from the Foundation side.
        /// </param>
        /// <param name="progressiveWinCapLimit">
        /// The progressive win cap limit getting from the Foundation side.
        /// </param>
        /// <param name="totalWinCapLimit">
        /// The total win cap limit getting from the Foundation side.
        /// </param>
        public WinCapInformation(WinCapBehaviorInfo gameWinCapBehaviorInfo, long progressiveWinCapLimit,
            long totalWinCapLimit) : this()
        {
            GameWinCapBehaviorInfo = gameWinCapBehaviorInfo;
            ProgressiveWinCapLimit = progressiveWinCapLimit;
            TotalWinCapLimit = totalWinCapLimit;
        }

        /// <summary>
        /// Gets the game win cap limit based on the current bet and max bet passed in
        /// if current win cap behavior is not FixedWinCapAmount; otherwise, the fixed
        /// win cap amount will be returned.
        /// </summary>
        /// <param name="currentBet">The current bet amount of this game cycle.</param>
        /// <param name="maxBet">The max bet amount of this game.</param>
        /// <returns>Calculated game win cap limit.</returns>
        public long GetGameWinCapLimit(long currentBet, long maxBet)
        {
            long winCapLimit = 0;

            switch(GameWinCapBehaviorInfo.WinCapBehavior)
            {
                case WinCapBehavior.MultipliedByCurrentBet:
                    winCapLimit = currentBet * GameWinCapBehaviorInfo.WinCapMultiplier;
                    break;
                case WinCapBehavior.MultipliedByMaxBet:
                    winCapLimit = maxBet * GameWinCapBehaviorInfo.WinCapMultiplier;
                    break;
                case WinCapBehavior.FixedWinCapAmount:
                    winCapLimit = GameWinCapBehaviorInfo.WinCapAmount;
                    break;
            }

            if(winCapLimit < 0)
            {
                winCapLimit = 0;
            }

            return winCapLimit;
        }
    }
}
