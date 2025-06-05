// -----------------------------------------------------------------------
// <copyright file = "IShellApiCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.ShellApiControl;
    using Game.Core.Communication.Foundation.F2XTransport;

    /// <summary>
    /// This interface defines callback methods related to the Shell API.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    internal interface IShellApiCallbacks
    {
        /// <summary>
        /// Process the environment arguments for the Shell sent by the Foundation.
        /// </summary>
        /// <param name="shellTag">
        /// The shell tag.
        /// </param>
        /// <param name="shellTagDataFile">
        /// The shell tag data file.
        /// </param>
        /// <param name="linkedExtensions">
        /// The extensions linked to the shell application.
        /// </param>
        void ProcessShellApiStart(string shellTag, string shellTagDataFile, ICollection<LinkedExtension> linkedExtensions);

        /// <summary>
        /// Process the result of a round of Shell level API category negotiation started by the Foundation.
        /// </summary>
        /// <param name="installedHandlers">
        /// The category handlers that have been installed as the result of the negotiation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        void ProcessShellApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers);
    }
}