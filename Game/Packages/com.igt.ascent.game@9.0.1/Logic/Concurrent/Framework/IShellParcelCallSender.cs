// -----------------------------------------------------------------------
// <copyright file = "IShellParcelCallSender.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs to forward parcel calls of a coplayer to the shell,
    /// who will in turn send the parcel calls out to Foundation on behalf of the coplayer.
    /// </summary>
    internal interface IShellParcelCallSender
    {
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