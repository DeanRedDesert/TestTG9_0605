//-----------------------------------------------------------------------
// <copyright file = "HoverLevel.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Enumeration which indicates the desired amount of reel hovering.
    /// </summary>
    public enum HoverLevel
    {
        /// <summary>
        /// No hover.
        /// </summary>
        Off,

        /// <summary>
        /// Hover  between two specified values.
        /// </summary>
        Custom,

        /// <summary>
        /// Hover between +3 and -1 from the reel stop.
        /// </summary>
        Top,

        /// <summary>
        /// Hover between +2 and -2 from the reel stop.
        /// </summary>
        Center,

        /// <summary>
        /// Hover between +1 and -3 from the reel stop.
        /// </summary>
        Bottom
    }
}
