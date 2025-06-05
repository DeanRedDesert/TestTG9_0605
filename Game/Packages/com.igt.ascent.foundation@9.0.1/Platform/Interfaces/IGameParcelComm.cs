//-----------------------------------------------------------------------
// <copyright file = "IGameParcelComm.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for a game to send and receive parcel calls.
    /// </summary>
    public interface IGameParcelComm
    {
        /// <summary>
        /// Event occurs when a non-transactional parcel call is received.
        /// </summary>
        event EventHandler<NonTransactionalParcelCallReceivedEventArgs> NonTransactionalParcelCallReceivedEvent;

        /// <summary>
        /// Event occurs when a transactional parcel call is received.
        /// </summary>
        event EventHandler<TransactionalParcelCallReceivedEventArgs> TransactionalParcelCallReceivedEvent;

        /// <summary>
        /// Sends a non transactional parcel call to the specified target.
        /// </summary>
        /// <param name="targetEndpoint">The target endpoint that the parcel call is sent to.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The acceptance status of the parcel call.</returns>
        ParcelCallStatus SendNonTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload);

        /// <summary>
        /// Sends a transactional parcel call to the specified target.
        /// </summary>
        /// <param name="targetEndpoint">The target endpoint that the parcel call is sent to.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The result of the parcel call.</returns>
        ParcelCallResult SendTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload);
    }
}
