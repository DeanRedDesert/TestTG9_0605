//-----------------------------------------------------------------------
// <copyright file = "EmulatedServiceButtonEnabledConfigItemChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Event indicating that the "Emulated Service Button Enabled" config item has changed.
    /// </summary>
    public class EmulatedServiceButtonEnabledConfigItemChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The new value of the "Emulated Service Button Enabled" Config Item.
        /// </summary>
        public bool EmulatedServiceButtonEnabledConfigItemValue { get; }

        /// <summary>
        /// Construct an instance with the given value of the "Emulated Service Button Enabled" Config Item.
        /// </summary>
        /// <param name="configItemValue">The new value of the "Emulated Service Button Enabled" Config Item.</param>
        public EmulatedServiceButtonEnabledConfigItemChangedEventArgs(bool configItemValue)
        {
            EmulatedServiceButtonEnabledConfigItemValue = configItemValue;
        }
    }
}