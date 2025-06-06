//-----------------------------------------------------------------------
// <copyright file = "ICdsHhrCategory.cs" company = "IGT">
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
    using Schemas.Internal.CdsHhr;

    /// <summary>
    /// The CdsHhr category of messages to support Historical Horse Racing.
    /// Category: 1035; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface ICdsHhrCategory
    {
        /// <summary>
        /// Message notifying the Foundation that the host outcome/enrollment information did not match the theme/EGM
        /// evaluated award information.
        /// </summary>
        /// <param name="logMessage">
        /// Log message detailing the specifics of the mismatch.  This string is for logging purposes and is NOT
        /// displayed to the player or in tilt messages.
        /// </param>
        void AwardMismatchDetected(string logMessage);

        /// <summary>
        /// Message requesting the current CDS configuration regarding offerable betting and top awards.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataCdsGameConfigReply message.
        /// </returns>
        IEnumerable<DenomConfig> GetConfigDataCdsGameConfig();

        /// <summary>
        /// Message requesting if multidraw is supported.  Multidraw is the ability to include multiple
        /// EnrollRequestItems in SetEnrollmentRequestData.
        /// </summary>
        /// <returns>
        /// The content of the GetConfigDataMultidrawSupportedReply message.
        /// </returns>
        bool GetConfigDataMultidrawSupported();

        /// <summary>
        /// Message retrieving the enrollment response data from the last successful response.
        /// </summary>
        /// <returns>
        /// The content of the GetEnrollmentResponseDataReply message.
        /// </returns>
        EnrollmentResponse GetEnrollmentResponseData();

        /// <summary>
        /// Message that provides the enrollment specifics to be used for the next enrollment request.
        /// </summary>
        /// <param name="enrollmentRequestItems">
        /// List of EnrollmentRequestItem.
        /// </param>
        void SetEnrollmentRequestData(IEnumerable<EnrollmentRequestItem> enrollmentRequestItems);

    }

}

