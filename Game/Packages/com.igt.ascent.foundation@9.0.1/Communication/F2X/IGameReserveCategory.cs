//-----------------------------------------------------------------------
// <copyright file = "IGameReserveCategory.cs" company = "IGT">
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
    using Schemas.Internal.GameReserve;

    /// <summary>
    /// Game Reserve category of messages for getting Reserve configuration info, and for clients to post when they are
    /// in a Reserve State. While a client is in a Reserve state all user input should be ignored, unless it has the
    /// ability to cancel Reserve. It is expected of the client to not send any state change requests while in a
    /// Reserve state. As a fallback, the Foundation will also deny any state change requests made by a client that is
    /// in a reserve state. If the Game or Foundation does any form of state change that causes reserve to be canceled,
    /// then its the responsibility of the state changer to cancel Reserve.
    /// Category: 1036; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IGameReserveCategory
    {
        /// <summary>
        /// Get the configured Reserve parameters.
        /// </summary>
        /// <returns>
        /// The content of the GetReserveParametersReply message.
        /// </returns>
        ReserveParameters GetReserveParameters();

        /// <summary>
        /// Client requesting to change the state of Game Reserve. Client is responsible for starting the Reserve timer,
        /// and sending a "false" Reserve request once the timer has expired, or when the user has manually ended
        /// Reserve. If a duplicate Reserve state is sent (requesting "true" while Reserve is active, or "false" when
        /// Reserve is not active) the Foundation will not accept the request.
        /// </summary>
        /// <param name="reserveActive">
        /// The Reserve State that the game is requesting to be in.
        /// </param>
        /// <returns>
        /// The content of the RequestReserveActivationReply message.
        /// </returns>
        bool RequestReserveActivation(bool reserveActive);

    }

}

