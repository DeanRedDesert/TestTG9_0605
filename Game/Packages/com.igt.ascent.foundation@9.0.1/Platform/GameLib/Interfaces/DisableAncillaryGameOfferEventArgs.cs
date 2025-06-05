//-----------------------------------------------------------------------
// <copyright file = "DisableAncillaryGameOfferEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Event that indicates to the game that no ancillary game should be
    /// offered to the player any more.
    /// </summary>
    [Serializable]
    public class DisableAncillaryGameOfferEventArgs : EventArgs
    {
    }
}
