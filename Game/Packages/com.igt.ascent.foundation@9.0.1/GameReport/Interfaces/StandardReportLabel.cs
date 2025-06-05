//-----------------------------------------------------------------------
// <copyright file = "StandardReportLabel.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a Foundation-defined standard report item.
    /// </summary>
    public enum StandardReportLabel
    {
        /// <summary>
        /// 100% - <see cref="BaseRtpPercent"/>.
        /// </summary>
        BaseHoldPercent,

        /// <summary>
        /// Return to Player (RTP) percent at minimum bet.
        /// </summary>
        BaseRtpPercent,

        /// <summary>
        /// The game description or the name of the theme.
        /// </summary>
        GameDescription,

        /// <summary>
        /// Sum of the RTP's from the theme's progressives.
        /// </summary>
        JackpotRtp,

        /// <summary>
        /// The progressive link name for the theme.
        /// </summary>
        LinkSeriesModel,

        /// <summary>
        /// The theme's maximum number of line patterns for a valid bet.
        /// </summary>
        MaxLines,

        /// <summary>
        /// The theme's maximum number of multi-way patterns for a valid bet.
        /// </summary>
        MaxWays,

        /// <summary>
        /// The highest pay amount for a single Base Game win category on
        /// a single line/multi-way pattern at maximum bet, excluding progressive wins.
        /// </summary>
        MaxWinAmount,

        /// <summary>
        /// The highest pay in credits for a single Base Game win category on
        /// a single line/multi-way pattern at maximum bet, excluding progressive wins.
        /// </summary>
        MaxWinCredits,

        /// <summary>
        /// The minimum bet amount for the current theme configuration.
        /// </summary>
        MinBetAmount,

        /// <summary>
        /// The minimum bet in credits for the current theme configuration.
        /// </summary>
        MinBetCredits,

        /// <summary>
        /// The theme's minimum number of line patterns for a valid bet.
        /// </summary>
        MinLines,

        /// <summary>
        /// The theme's minimum number of multi-way patterns for a valid bet.
        /// </summary>
        MinWays,

        /// <summary>
        /// 100% - <see cref="TotalRtpPercent"/>.
        /// </summary>
        TotalHoldPercent,

        /// <summary>
        /// Return to Player (RTP) percent at maximum bet.
        /// </summary>
        TotalRtpPercent
    }
}
