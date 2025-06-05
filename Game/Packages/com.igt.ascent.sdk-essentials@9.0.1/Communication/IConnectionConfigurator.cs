// -----------------------------------------------------------------------
// <copyright file = "IConnectionConfigurator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    /// <summary>
    /// This interfaces defines the information that are needed
    /// for establishing the connection with Foundation.
    /// </summary>
    public interface IConnectionConfigurator
    {
        /// <summary>
        /// Gets or sets the configuration of the transport used to communicating with Foundation.
        /// </summary>
        TransportConfiguration TransportConfiguration { get; }

        /// <summary>
        /// Gets or sets the Foundation version the game targets to run with.
        /// </summary>
        FoundationTarget FoundationTarget { get; }
    }
}