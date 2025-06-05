// -----------------------------------------------------------------------
// <copyright file = "ILinkApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System.Collections.Generic;
    using F2X.Schemas.Internal.DiscoveryContextTypes;
    using F2X.Schemas.Internal.LinkControl;
    using F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the Link level APIs.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface ILinkApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the link sent by the Foundation.
        /// </summary>
        /// <param name="jurisdiction">
        /// The jurisdiction string.
        /// </param>
        /// <param name="connectToken">
        /// The token for the executable to identify itself to the Foundation.
        /// </param>
        /// <param name="discoveryContexts">
        /// The collection of discovery contexts for the link, including executable extensions.
        /// </param>
        /// <param name="extensionImports">
        /// A collection of imported/linked extensions.
        /// </param>
        void ProcessLinkApiStart(string jurisdiction, string connectToken,
                                 ICollection<DiscoveryContext> discoveryContexts,
                                 ICollection<ExtensionImport> extensionImports);

        /// <summary>
        /// Process the result of a round of API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        void ProcessLinkApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);

        /// <summary>
        /// Process a park request from the Foundation.
        /// </summary>
        void ProcessLinkPark();
    }
}