//-----------------------------------------------------------------------
// <copyright file = "CultureChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Event indicating that the EGM's culture has changed.
    /// </summary>
    public class CultureChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The new value of the EGM's culture.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Construct an instance with the given value of the EGM's culture.
        /// </summary>
        /// <param name="culture">The new value of the EGM's culture.</param>
        public CultureChangedEventArgs(string culture)
        {
            Culture = culture;
        }
    }
}