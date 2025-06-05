// -----------------------------------------------------------------------
// <copyright file = "IInnerLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// This interface defines the common functionality of an Inner Link.
    /// </summary>
    internal interface IInnerLink
    {
        /// <summary>
        /// Gets the API Manager working on the link.
        /// </summary>
        IApiManager ApiManager { get; }

        /// <summary>
        /// Gets the flag indicating whether the link is currently connected to Foundation.
        /// </summary>
        bool IsConnected { get; }
    }
}