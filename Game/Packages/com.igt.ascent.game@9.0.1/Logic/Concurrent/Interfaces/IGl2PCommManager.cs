// -----------------------------------------------------------------------
// <copyright file = "IGl2PCommManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Game.Core.Communication.Presentation.CommServices;

    /// <summary>
    /// This interface defines APIs for the presentation thread
    /// to retrieve interfaces for GL2P communications.
    /// </summary>
    public interface IGl2PCommManager
    {
        /// <summary>
        /// Gets the presentation side interface of a GL2P comm channel
        /// identified by the given cotheme presentation key.
        /// </summary>
        /// <param name="key">
        /// The presentation key identifying the GL2P comm channel.
        /// </param>
        /// <returns>
        /// The presentation side interface of a GL2P comm channel with the specified key.
        /// Null if <paramref name="key"/> was not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        IPresentationCommServices GetPresentationCommServices(CothemePresentationKey key);
    }
}