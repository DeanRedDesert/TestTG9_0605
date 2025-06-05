//-----------------------------------------------------------------------
// <copyright file = "IGameTimerControllerInternal.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    /// <summary>
    /// An interface used for testing or special use cases where the default timer intervals need to be overridden.
    /// </summary>
    internal interface IGameTimerControllerInternal
    {
        #region Methods

        /// <summary>
        /// Set override values for timer defaults. This should be used only by test or internal code to speed up actual timer
        /// behavior.
        /// </summary>
        /// <param name="timerTickScaleFactor">A value indicating how many 100 ms interval heartbeat timer ticks should comprise
        /// one game timer tick event. The default value is ten, which results in one second events. Lower values will speed up the timer
        /// tick intervals. </param>
        void SetTimerOverrides(ushort timerTickScaleFactor);

        #endregion
    }
}
