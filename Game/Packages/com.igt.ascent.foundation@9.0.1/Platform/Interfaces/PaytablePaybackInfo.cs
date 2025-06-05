// -----------------------------------------------------------------------
// <copyright file = "PaytablePaybackInfo.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This class contains the information on a paytable's payback percentages.
    /// </summary>
    [Serializable]
    public class PaytablePaybackInfo
    {
        /// <summary>
        /// Gets the identifier of the paytable.
        /// This is the identifier maintained by the Foundation, used in communication with the Foundation
        /// to identify an individual "pay variation".
        /// </summary>
        /// <remarks>
        /// Users (games or other applications) should not interpret this identifier in any ways.
        /// </remarks>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the theoretical maximum payback percentage of the paytable,
        /// including progressive contributions where applicable.
        /// </summary>
        public decimal PaybackPercentage { get; }

        /// <summary>
        /// Gets the theoretical minimum payback percentage, including progressive contributions where applicable.
        /// </summary>
        /// <remarks>
        /// If the game designer has included the (mandatory) progressive contribution as part of this value,
        /// then the <see cref="MinPaybackPercentageWithoutProgressives"/> will also be a valid value.
        /// </remarks>
        public decimal MinPaybackPercentage { get; }

        /// <summary>
        /// Gets the theoretical minimum payback percentage excluding progressive contributions.
        /// <see cref="MinPaybackPercentage"/> value.
        /// </summary>
        /// <remarks>
        /// The value is 0 if the game designer has not included the (mandatory) progressive contribution as part of
        /// </remarks>
        public decimal MinPaybackPercentageWithoutProgressives { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PaytablePaybackInfo"/>.
        /// </summary>
        /// <param name="paytableIdentifier">
        /// The identifier of the paytable.
        /// </param>
        /// <param name="paybackPercentage">
        /// The theoretical maximum payback percentage of the paytable.
        /// </param>
        /// <param name="minPaybackPercentage">
        /// The theoretical minimum payback percentage, including progressive contributions where applicable.
        /// </param>
        /// <param name="minPaybackPercentageWithoutProgressives">
        /// The theoretical minimum payback percentage excluding progressive contributions.
        /// </param>
        public PaytablePaybackInfo(string paytableIdentifier,
                                   decimal paybackPercentage,
                                   decimal minPaybackPercentage,
                                   decimal minPaybackPercentageWithoutProgressives)
        {
            PaytableIdentifier = paytableIdentifier;
            PaybackPercentage = paybackPercentage;
            MinPaybackPercentage = minPaybackPercentage;
            MinPaybackPercentageWithoutProgressives = minPaybackPercentageWithoutProgressives;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                $"Paytable Identifier({PaytableIdentifier})/PaybackPercentages({PaybackPercentage}% {MinPaybackPercentage}% {MinPaybackPercentageWithoutProgressives}%)";
        }
    }
}