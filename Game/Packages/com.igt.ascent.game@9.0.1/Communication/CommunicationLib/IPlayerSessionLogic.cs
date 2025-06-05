//-----------------------------------------------------------------------
// <copyright file = "IPlayerSessionLogic.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    /// <summary>
    /// This interface contains the APIs for the game presentation
    /// to communicate with the player session parameters logic.
    /// </summary>
    public interface IPlayerSessionLogic
    {
        /// <summary>
        /// Informs the logic for player session parameters that the presentation would like to request
        /// action on resetting player session parameters.
        /// </summary>
        /// <param name="actionData">
        /// The action request type.
        /// </param>
        /// <param name="actionRequest">
        /// The action data if any.
        /// </param>
        void RequestPlayerSessionParametersResetAction(PlayerSessionParametersResetActionType actionRequest,
                                                       object actionData);
    }
}
