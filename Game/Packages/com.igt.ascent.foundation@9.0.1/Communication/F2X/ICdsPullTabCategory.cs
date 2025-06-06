//-----------------------------------------------------------------------
// <copyright file = "ICdsPullTabCategory.cs" company = "IGT">
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
    using System.Collections.Generic;
    using Schemas.Internal.CdsPullTab;

    /// <summary>
    /// CdsPullTab category of messages.  Supports (Washington) Pull Tab.  Category: 1002  Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ICdsPullTabCategory
    {
        /// <summary>
        /// Message from the bin to the Foundation notifying the Foundation that the host outcome/enrollment information
        /// did not match the theme/EGM evaluated award information.  B2F: FI channel.
        /// </summary>
        /// <param name="logMessage">
        /// Log message detailing the specifics of the mismatch.  This string is for logging purposes and is NOT
        /// displayed to the player or in tilt messages.
        /// </param>
        void AwardMismatchDetected(string logMessage);

        /// <summary>
        /// Message from the bin to the Foundation requesting the current CDS configuration regarding offerable betting
        /// and top awards.  B2F: FI channel.
        /// </summary>
        /// <returns>
        /// Contains the attributes for an offerable denomination.
        /// </returns>
        IEnumerable<DenomConfig> GetConfigDataCdsGameConfig();

        /// <summary>
        /// Message from the bin to the Foundation requesting if multidraw is supported.  Multidraw is the ability to
        /// include multiple EnrollRequestItems in SetEnrollmentRequestData.  B2F: FI channel.
        /// </summary>
        /// <returns>
        /// True if multidraw is supported; false if not.
        /// </returns>
        bool GetConfigDataMultidrawSupported();

        /// <summary>
        /// Message from the bin to the Foundation retrieving the enrollment response data from the last successful
        /// response.  B2F: FI channel.
        /// </summary>
        /// <returns>
        /// The data from an enrollment response.
        /// </returns>
        GetEnrollmentResponseDataReplyContentEnrollmentResponse GetEnrollmentResponseData();

        /// <summary>
        /// Message from the bin to Foundation that provides the enrollment specifics to be used for the next enrollment
        /// request.  B2F: FI channel.
        /// </summary>
        /// <param name="enrollmentRequest">
        /// The data for an enrollment request. / List of enrollment request items. / Data for an enrollment request
        /// item.
        /// </param>
        void SetEnrollmentRequestData(IEnumerable<EnrollmentRequestItem> enrollmentRequest);

    }

}

