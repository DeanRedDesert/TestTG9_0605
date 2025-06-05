// -----------------------------------------------------------------------
// <copyright file = "ITsmApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the TSM API.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface ITsmApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the TSM sent by the Foundation.
        /// </summary>
        /// <param name="tsmIdentifier">Identifies the TSM that API negotiation will support.</param>
        /// <param name="extensions">
        /// List of extensions linked to this TSM.  The versions are specifically
        /// the (minor) versions of the importing component requested in the registry,
        /// not what are compatible with the importing component.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessTsmApiStart(string tsmIdentifier, IEnumerable<Extension> extensions);

        /// <summary>
        /// Process the result of a round of TSM level API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessTsmApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);
    }
}
