//-----------------------------------------------------------------------
// <copyright file = "PortalPosition.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration containing portal positions.
    /// </summary>
    public enum PortalPosition
    {
        /// <summary>
        /// Option indicating an invalid portal position.
        /// This type of position is not supported by the Foundation.
        /// </summary>
        InvalidPortalPosition,

        /// <summary>
        /// Option indicating the portal is positioned to the left.
        /// If chosen, the portal will be anchored to left side of monitor, strectching
        /// from the top of the monitor to the bottom of the monitor.
        /// </summary>
        Left,

        /// <summary>
        /// Option indicating the portal is positioned to the right.
        /// If chosen, the portal will be anchored to right side of monitor, strectching
        /// from the top of the monitor to the bottom of the monitor.
        /// </summary>
        Right,

        /// <summary>
        /// Option indicating the portal is positioned to the bottom.
        /// If chosen, the portal will be anchored to bottom of the monitor, strectching
        /// from the left side of the monitor to the right side of the monitor.
        /// </summary>
        Bottom,

        /// <summary>
        /// Option indicating the portal is positioned to the top.
        /// If chosen, the portal will be anchored to top of the monitor, strectching
        /// from the left side of the monitor to the right side of the monitor.
        /// </summary>
        Top,

        /// <summary>
        /// Option indicating the portal is positioned to float.
        /// If chosen, the portal is free to be positioned anywhere on the monitor, presuming
        /// the provided location and dimensions do not exceed the monitor's pixel range.
        /// </summary>
        Float,

        /// <summary>
        /// Option indicating the portal is positioned to take the full screen.
        /// If chosen, the portal will consume the entire monitor and completely engulf
        /// the underlying content.
        /// </summary>
        Full,

        /// <summary>
        /// Option indicating the portal is positioned to the center of the monitor.
        /// If chosen, the center of the portal will align with the center of the 
        /// monitor.
        /// </summary>
        Center
    }
}