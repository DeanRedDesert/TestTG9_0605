//-----------------------------------------------------------------------
// <copyright file = "GameTimerStatus.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    /// <summary>
    /// Enumerations describing game timer statuses.
    /// </summary>
    public enum GameTimerStatus
    {
        /// <summary>
        /// The timer is in between actions.
        /// </summary>
        Idling,
        /// <summary>
        /// The timer has started ticking.
        /// </summary>
        Started,
        /// <summary>
        /// The timer has stopped ticking.
        /// </summary>
        Stopped,
        /// <summary>
        /// The timer is ticking.
        /// </summary>
        Ticking,
    }
}