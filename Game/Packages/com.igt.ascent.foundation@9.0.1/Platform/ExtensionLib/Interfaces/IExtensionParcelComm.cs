//-----------------------------------------------------------------------
// <copyright file = "IExtensionParcelComm.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for F2X Extension Parcel Communication.
    /// </summary>
    public interface IExtensionParcelComm
    {
        /// <summary>
        /// Invokes a non transactional parcel call from the extension to the specified target.
        /// </summary>
        /// <param name="sourceIdentifier">The source identifier of the extension that sends the parcel call.</param>
        /// <param name="targetEndpoint">The target entity of the parcel call.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The acceptance status of the parcel call.</returns>
        ParcelCallStatus SendNonTransactionalParcelCall(string sourceIdentifier, ParcelCommEndpoint targetEndpoint, byte[] payload);

        /// <summary>
        /// Invokes a transactional parcel call from the extension to the specified target.
        /// </summary>
        /// <param name="sourceIdentifier">The source identifier of the extension that sends the parcel call.</param>
        /// <param name="targetEndpoint">The target entity of the parcel call.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The result of the parcel call.</returns>
        ParcelCallResult SendTransactionalParcelCall(string sourceIdentifier, ParcelCommEndpoint targetEndpoint, byte[] payload);

        /// <summary>
        /// Event occurs when a non-transactional parcel call is received by the extension.
        /// </summary>
        event EventHandler<NonTransactionalParcelCallReceivedEventArgs> NonTransactionalParcelCallReceivedEvent;

        /// <summary>
        /// Event occurs when a transactional parcel call is received by the extension.
        /// </summary>
        event EventHandler<TransactionalParcelCallReceivedEventArgs> TransactionalParcelCallReceivedEvent;
    }
}
