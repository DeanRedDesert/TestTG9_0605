//-----------------------------------------------------------------------
// <copyright file = "PortalType.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration containing portal types.
    /// </summary>
    public enum PortalType
    {
        /// <summary>
        /// Option indicating an invalid portal type.
        /// </summary>
        InvalidType,

        /// <summary>
        /// Option indicating a scaled portal type.
        /// This option will cause lower priority content on the sceen to shrink,
        /// making room for the portal to display it's content.
        /// Portal position can't be float, full, or center for a scaled portal.
        /// </summary>
        Scale,

        /// <summary>
        /// Option indicating an overlay portal type.
        /// This option will cause the portal to be displayed on top of lower
        /// priority content on the screen, without causing the lower priority
        /// content to shrink.
        /// </summary>
        Overlay,

        /// <summary>
        /// Option indicating a modal portal type.
        /// This option is similar to <see cref="Overlay"/>, but prevents input
        /// (mouse clicks or user finger presses) from being sent to the lower
        /// priority content.
        /// </summary>
        Modal
    }
}