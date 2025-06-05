//-----------------------------------------------------------------------
// <copyright file = "PortalScreen.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration containing different monitor options where portals can be created.
    /// </summary>
    public enum PortalScreen
    {
        /// <summary>
        /// Option indicating an invalid screen (a screen that is not supported).
        /// </summary>
        InvalidScreen,

        /// <summary>
        /// Option indicating a primary screen (the main screen where the primary content is shown).
        /// </summary>
        Primary,

        /// <summary>
        /// Option indicating a secondary screen (the top screen where secondary content is shown).
        /// </summary>
        Secondary,

        /// <summary>
        /// Option indicating a topper screen (the video topper; not supported for all cabinets).
        /// </summary>
        Topper,

        /// <summary>
        /// Option indicating a DPP screen (not supported for all cabinets).
        /// </summary>
        Dpp
    }
}