// -----------------------------------------------------------------------
// <copyright file = "IAscribedGameApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using F2X.Schemas.Internal.AscribedGameApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the AscribedGame API.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface IAscribedGameApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the ascribed game level sent by the Foundation.
        /// </summary>
        /// <param name="ascribedGame">Identifies the ascribed game that API negotiation will support.</param>
        /// <param name="extensions">
        /// List of extensions linked to this ascribed game.  The versions are specifically
        /// the (minor) versions of the importing component requested in the registry,
        /// not what are compatible with the importing component.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessAscribedGameApiStart(AscribedGame ascribedGame, IEnumerable<Extension> extensions);

        /// <summary>
        /// Process the result of a round of ascribed game level API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessAscribedGameApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);
    }
}
