//-----------------------------------------------------------------------
// <copyright file = "HaloLightDirection.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// The direction the light pattern should move.
    /// </summary>
    /// <remarks>
    /// Design rule is suppressed because this enum represents a hardware data field which is 1 byte.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum HaloLightDirection : byte
    {
        /// <summary>
        /// Move from left to right.
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// Move from top to bottom.
        /// </summary>
        TopToBottom = 1,
        /// <summary>
        /// Move from right to left.
        /// </summary>
        RightToLeft = 2,
        /// <summary>
        /// Move from bottom to top.
        /// </summary>
        BottomToTop = 3,
        /// <summary>
        /// Move from left and right towards the center.
        /// </summary>
        PinchFromLeftAndRight = 4,
        /// <summary>
        /// From from the top and bottom towards the center.
        /// </summary>
        PinchFromTopAndBottom = 5,
    }
}
