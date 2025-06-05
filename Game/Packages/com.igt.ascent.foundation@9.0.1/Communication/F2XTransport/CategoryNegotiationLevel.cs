// -----------------------------------------------------------------------
// <copyright file = "CategoryNegotiationLevel.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// Represents when and where the F2X category can be negotiated with Foundation.
    /// </summary>
    /// <remarks> 
    /// CategoryNegotiationLevel is represented as a bit field, please maintain 
    /// enum values as ascending powers of 2.
    /// </remarks> 
    [Flags]
    public enum CategoryNegotiationLevel
    {
        /// <summary>
        /// The category can be negotiated at Link Control level.
        /// This is the most commonly used negotiation level.
        /// </summary>
        Link = 1,

        /// <summary>
        /// The category can be negotiated at System API Control level
        /// which is only available on F2E connection.
        /// </summary>
        System = 1 << 1,

        /// <summary>
        /// The category can be negotiated at Theme API Control level
        /// which is only available on F2E connection.
        /// </summary>
        Theme = 1 << 2,

        /// <summary>
        /// The category can be negotiated at TSM API Control level
        /// which is only available on F2E connection.
        /// </summary>
        Tsm = 1 << 3,

        /// <summary>
        /// The category can be negotiated at Shell API Control level
        /// which is only available on F2B connection for concurrent games.
        /// </summary>
        Shell = 1 << 4,

        /// <summary>
        /// The category can be negotiated at Coplayer API Control level
        /// which is only available on F2B connection for concurrent games.
        /// </summary>
        Coplayer = 1 << 5,

        /// <summary>
        /// The category can be negotiated at AscribedGame API Control level
        /// which is only available on F2E connection.
        /// </summary>
        AscribedGame = 1 << 6,

        /// <summary>
        /// The category can be negotiated at App API Control level
        /// which is only available on F2E connection.
        /// </summary>
        App = 1 << 7,
    }
}