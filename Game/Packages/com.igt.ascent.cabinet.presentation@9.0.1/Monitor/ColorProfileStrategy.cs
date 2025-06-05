//-----------------------------------------------------------------------
// <copyright file = "ColorProfileStrategy.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Monitor
{
    /// <summary>
    /// Strategy to use when setting color profiles.
    /// </summary>
    public enum ColorProfileStrategy
    {
        /// <summary>
        /// Prioritize the use of calibrated color profiles. If a calibrated profile is not available, then color profiles
        /// will not be applied.
        /// </summary>
        Calibrated,

        /// <summary>
        /// Prioritize profiles intended to make monitors in the cabinet match each other. This option should be used when
        /// matching monitors is a priority over accuracy. If a matched configuration is not available, then it will use
        /// calibrated profiles instead.
        /// </summary>
        Matched,

        /// <summary>
        /// Do not apply a color profile to any monitor.
        /// </summary>
        Uncalibrated,

        /// <summary>
        /// Use custom profiles. If the custom profiles do not match the monitors, then calibrated profiles will be used
        /// instead.
        /// </summary>
        Custom,
    }
}
