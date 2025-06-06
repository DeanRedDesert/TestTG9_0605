//-----------------------------------------------------------------------
// <copyright file = "INicInformationCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The NICInformation category of messages to assist the client in determining which NIC IP to bind to. For
    /// example, Windows will sometimes enumerate the video topper NIC first, causing issues when a game uses Windows
    /// API calls directly and winds up binding to the video topper NIC (in which case communications will never work).
    /// Category: 142; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface INicInformationCategory
    {
        /// <summary>
        /// A request to retrieve the IP address of the NIC that will be used to connect to the given host address.  The
        /// client should use the reply IP address to create a listening socket to accept connections back from the
        /// host (if needed).
        /// </summary>
        /// <param name="hostIpAddress">
        /// This is the IP address of the host that we will be making a connection to.  The reply will return the IP
        /// address of the NIC that will be used to make that connection.  The application should use the reply IP
        /// address to create a listening socket to accept connections back from the host (if needed).
        /// </param>
        /// <returns>
        /// The content of the GetIPAddressForHostConnectionReply message.
        /// </returns>
        string GetIPAddressForHostConnection(string hostIpAddress);

        /// <summary>
        /// A request to retrieve the prioritized IP list with primary NIC IP address listed first.  This should be used
        /// in the case of UDP discovery in which case we don't know which interface to use.  Use the first IP in the
        /// list and then fall back to the other IPs if discovery fails.
        /// </summary>
        /// <returns>
        /// The content of the GetPrioritizedIPListReply message.
        /// </returns>
        IEnumerable<string> GetPrioritizedIPList();

    }

}

