//-----------------------------------------------------------------------
// <copyright file = "GameLogicMessageType.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;

    /// <summary>
    /// This enumeration indicates the type of a game logic generic message.
    /// </summary>
    [Serializable]
    public enum GameLogicMessageType
    {
        /// <summary>
        /// The message to inform the game logic that
        /// a presentation state has completed.
        /// </summary>
        PresentationStateComplete,

        /// <summary>
        /// The message to post a presentation tilt.
        /// </summary>
        PostPresentationTilt,

        /// <summary>
        /// The message to clear a presentation tilt.
        /// </summary>
        ClearPresentationTilt,

        /// <summary>
        /// The message to reset the player session parameters.
        /// </summary>
        ResetPlayerSessionParameters,
    }
}
