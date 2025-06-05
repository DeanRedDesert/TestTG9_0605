// -----------------------------------------------------------------------
// <copyright file = "ICoplayerApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the Coplayer API.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface ICoplayerApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the Coplayer sent by the Foundation.
        /// </summary>
        /// <param name="coplayerId">The coplayer id.</param>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <param name="g2SThemeId">The G2S theme id.</param>
        /// <param name="themeTag">The theme tag.</param>
        /// <param name="themeTagDataFile">The theme tag data file.</param>
        void ProcessCoplayerApiStart(int coplayerId,
                                     string themeIdentifier,
                                     string g2SThemeId,
                                     string themeTag,
                                     string themeTagDataFile);

        /// <summary>
        /// Process the result of a round of Coplayer level API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessCoplayerApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);
    }
}