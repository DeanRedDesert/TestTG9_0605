//-----------------------------------------------------------------------
// <copyright file = "PaytableConfiguration.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    /// <summary>
    /// Struct that represents a specific paytable configuration in a paytable file configured for a game theme.
    /// </summary>
    internal class PaytableConfiguration
    {
        /// <summary>
        /// The paytable variant for this game configuration.
        /// </summary>
        public PaytableVariant PaytableVariant { get; }

        /// <summary>
        /// The MaxBet of the paytable for this game configuration.
        /// </summary>
        public long? MaxBet { get; set; }

        /// <summary>
        /// The ButtonPanelMinBet of the paytable for this game configuration.
        /// </summary>
        public long? ButtonPanelMinBet { get; set; }

        /// <summary>
        /// Initialize a new instance of Paytable Configuration with a specific Paytable Variant.
        /// </summary>
        /// <param name="paytableVariant">Specify the Paytable Variant.</param>
        public PaytableConfiguration(PaytableVariant paytableVariant)
            : this(paytableVariant, null, null)
        {
        }

        /// <summary>
        /// Initialize a new instance of Paytable Configuration with a specific Paytable Variant
        /// and configured max bet, min bet.
        /// </summary>
        /// <param name="paytableVariant">Specify the Paytable Variant.</param>
        /// <param name="maxBet">Specify the max bet.</param>
        /// <param name="buttonPanelMinBet">Specify the button panel min bet.</param>
        public PaytableConfiguration(PaytableVariant paytableVariant, long? maxBet, long? buttonPanelMinBet)
        {
            PaytableVariant = paytableVariant;
            MaxBet = maxBet;
            ButtonPanelMinBet = buttonPanelMinBet;
        }
    }
}
