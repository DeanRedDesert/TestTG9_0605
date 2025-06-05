// -----------------------------------------------------------------------
// <copyright file = "IConnectApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    /// <summary>
    /// This interface defines callback methods related to the Connect level APIs.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface IConnectApiCallbacks
    {
        /// <summary>
        /// Process a shut down request from the Foundation.
        /// </summary>
        void ProcessConnectShutDown();
    }
}