//-----------------------------------------------------------------------
// <copyright file = "ThemeSelectionMenuOfferableStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Event which indicates the offer-able state of the Theme Selection Menu has changed.
    /// </summary>
    [Serializable]
    public class ThemeSelectionMenuOfferableStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Flag which indicates if the menu can be offered.
        /// </summary>
        public bool Offerable { private set; get; }

        /// <summary>
        /// Construct an instance of the event with the given offer-able status.
        /// </summary>
        /// <param name="offerable">True if the menu can be offered.</param>
        public ThemeSelectionMenuOfferableStatusChangedEventArgs(bool offerable)
        {
            Offerable = offerable;
        }
    }
}
