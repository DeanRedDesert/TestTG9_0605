//-----------------------------------------------------------------------
// <copyright file = "DoubleUpOfferStateHelperSupportedActions.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    /// <summary>
    /// This class contains a list of actions supported by the DoubleUp offer State.
    /// </summary>
    public static class DoubleUpOfferStateHelperSupportedActions
    {
        /// <summary>
        /// Request entering DoubleUp.
        /// </summary>
        public const string DoubleUpPressedRequest = "DoubleUpPressed";

        /// <summary>
        /// Request disabling DoubleUp.
        /// </summary>
        public const string DoubleUpDisabledRequest = "DoubleUpDisabled";

        /// <summary>
        /// Request a playline selection. Requires generic data.
        /// </summary>
        public const string LineSelectedRequest = "LineSelected";

        /// <summary>
        /// Requests to take win the gamble.
        /// </summary>
        public const string TakeWinRequest = "TakeWinRequest";

        /// <summary>
        /// request a game start. Requires generic data.
        /// </summary>
        public const string StartGameRequest = IdleStateHelperSupportedActions.StartGameRequest;

        /// <summary>
        /// Request an AutoPlay. 
        /// </summary>
        public const string AutoPlayRequest = IdleStateHelperSupportedActions.AutoPlayChangeStateRequest;
    }
}
