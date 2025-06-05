//-----------------------------------------------------------------------
// <copyright file = "JurisdictionStrings.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using IGT.Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// The string representation of the jurisdictions which games commonly check for to enable/disable features.
    /// </summary>
    /// <remarks>
    /// IMPORTANT:
    ///
    /// The jurisdiction information is provided to the game from the Foundation in the
    /// <see cref="IGameLib.Jurisdiction"/> property, which is a hard coded string.
    ///
    /// DO NOT rely on a specific jurisdiction string value to implement a feature,
    /// as the jurisdiction string value is not enumerated, and could change over time.
    ///
    /// For example, Nevada used to be reported as USDM, but later as 00NV.
    ///
    /// The jurisdiction string is intended only for the purpose of temporary work-around, when the time-line
    /// of the official support for a feature in Foundation and/or SDK could not meet a game's
    /// specific timetable requirement.  The game should use this jurisdiction string at
    /// its own risks of breaking compatibility with future Foundation and/or SDK.
    ///
    /// This class is provided for the convenience of a game if it HAS TO check against a certain jurisdiction string value.
    /// However, as the values could be changed over time by the Foundation,
    /// SDK does not take the responsibility on updating these values in a timely manner.
    /// </remarks>
    public static class JurisdictionStrings
    {
        /// <summary>
        /// The British Columbia Lottery jurisdiction.  This can be used to enable/disable the game rules page to inform the
        /// player of the minimum and maximum payback percentages.
        /// </summary>
        public const string BritishColumbiaLottery = "BCLC";

        /// <summary>
        /// The Ontario jurisdiction.  This can be used to enable/disable the reel strip distortion feature.
        /// </summary>
        public const string Ontario = "0ONT";

        /// <summary>
        /// The Singapore jurisdiction.  This can be used to disclose certain information to the player and pay combos
        /// required by the Singapore jurisdiction.
        /// </summary>
        public const string Singapore = "SING";
    }
}
