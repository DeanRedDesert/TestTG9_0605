//-----------------------------------------------------------------------
// <copyright file = "ICdsBingoConfigCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.CdsBingoConfig;
    using Schemas.Internal.Types;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="CdsBingoConfig"/> category.
    /// CdsBingoConfig category of messages.  Provides access to configuration related values and services related to
    /// Bingo protocol operation.
    /// Category: 3010; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ICdsBingoConfigCategoryCallbacks
    {
        /// <summary>
        /// Notification that the Foundation has entered or exited configuration mode.
        /// </summary>
        /// <param name="inConfigurationMode">
        /// Boolean indicating the Foundation is in configuration mode when true.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessConfigurationModeStatusChanged(bool inConfigurationMode);

        /// <summary>
        /// Notification from the Foundation to the Client that the Taxable Event Threshold has changed.
        /// </summary>
        /// <param name="threshold">
        /// The new amount value for the Taxable Event Treshold.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessTaxableEventThresholdChanged(Amount threshold);

    }

}

