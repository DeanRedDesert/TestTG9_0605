// -----------------------------------------------------------------------
// <copyright file = "IServiceRequestDataManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines APIs for retrieving the <see cref="ServiceRequestData"/> needed by a presentation.
    /// </summary>
    public interface IServiceRequestDataManager
    {
        /// <summary>
        /// Loads the service request data for a given theme presentation.
        /// </summary>
        /// <remarks>
        /// This interface will be used on multiple threads.
        /// The implementation of this interface must be thread-safe.
        /// </remarks>
        /// <param name="g2SThemeId">
        /// The G2SThemeId defined in the theme registry which identifies a unique game theme.
        /// Use CothemePresentationKey.ShellG2S to get the data for a Shell presentation.
        /// </param>
        /// <returns>
        /// The service request data needed by all states in the specified presentation.
        /// The returned data is keyed by the state names.
        /// </returns>
        IDictionary<string, ServiceRequestData> LoadServiceRequestData(string g2SThemeId);
    }
}