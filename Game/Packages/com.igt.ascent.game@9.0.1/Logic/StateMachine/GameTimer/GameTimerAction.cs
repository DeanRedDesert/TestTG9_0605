//-----------------------------------------------------------------------
// <copyright file = "GameTimerAction.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    /// <summary>
    /// Enumerations describing game timer actions.
    /// </summary>
    public enum GameTimerAction
    {
        /// <summary>
        /// The default state.
        /// </summary>
        DoNothing,

        /// <summary>
        /// Start timer.
        /// </summary>
        Start,

        /// <summary>
        /// Stop timer.
        /// </summary>
        Stop,

        /// <summary>
        /// Reset timer.
        /// </summary>
        Reset,
    }
}
