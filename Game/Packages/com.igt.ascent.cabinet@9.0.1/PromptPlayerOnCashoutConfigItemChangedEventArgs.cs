//-----------------------------------------------------------------------
// <copyright file = "PromptPlayerOnCashoutConfigItemChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Event indicating that the activity status has changed.
    /// </summary>
    public class PromptPlayerOnCashoutConfigItemChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The new value of the "Prompt Player On Cashout" Config Item.
        /// </summary>
        public bool PromptPlayerOnCashoutConfigItemValue { get; }

        /// <summary>
        /// Construct an instance with the given value of the "Prompt Player On Cashout" Config Item.
        /// </summary>
        /// <param name="configItemValue">The new value of the "Prompt Player On Cashout" Config Item.</param>
        public PromptPlayerOnCashoutConfigItemChangedEventArgs(bool configItemValue)
        {
            PromptPlayerOnCashoutConfigItemValue = configItemValue;
        }
    }
}