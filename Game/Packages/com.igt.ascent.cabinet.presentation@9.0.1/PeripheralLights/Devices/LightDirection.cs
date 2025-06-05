// -----------------------------------------------------------------------
// <copyright file = "LightDirection.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// This enumeration specifies the direction a light sequence should go.
    /// </summary>
    /// <remarks>
    /// Design rule is suppressed because this enum represents a hardware data field which is 1 byte.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum LightDirection : byte
    {
        /// <summary>
        /// Clockwise.
        /// </summary>
        Clockwise = 0,

        /// <summary>
        /// Counter Clockwise.
        /// </summary>
        Counterclockwise = 1,
    }
}