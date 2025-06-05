// -----------------------------------------------------------------------
// <copyright file = "ISystemApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the System API.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface ISystemApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the system extension sent by the Foundation.
        /// </summary>
        /// <param name="extensions">
        /// List of system executable extensions supported by this extension application.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessSystemApiStart(IEnumerable<Extension> extensions);

        /// <summary>
        /// Process the result of a round of System level API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessSystemApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);
    }
}
