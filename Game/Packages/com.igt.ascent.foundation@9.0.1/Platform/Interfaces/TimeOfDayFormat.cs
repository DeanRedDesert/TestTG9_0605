//-----------------------------------------------------------------------
// <copyright file = "TimeOfDayFormat.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The format to display time of day values.
    /// </summary>
    [Serializable]
    public enum TimeOfDayFormat
    {
        /// <summary>
        /// An unspecified format.The application is free to use any format it likes.
        /// </summary>
        Invalid,

        /// <summary>
        /// Twenty four hour.
        /// </summary>
        TwentyFourHour,

        /// <summary>
        /// Twelve hour with am and pm.
        /// </summary>
        TwelveHourWithAmPm,

        /// <summary>
        /// Twelve hour without am and pm.
        /// </summary>
        TwelveHourWithoutAmPm
    }
}