//-----------------------------------------------------------------------
// <copyright file = "IProgressiveControllerRestricted.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    /// <summary>
    /// This interface contains restricted progressive controller functionality
    /// that is used by the game entry thread only, and should not be used by
    /// game state machine code in most cases.
    /// </summary>
    public interface IProgressiveControllerRestricted
    {
        /// <summary>
        /// Clear all the event subscriptions to release the references to the
        /// event listening objects.
        /// </summary>
        /// <remarks>
        /// This is needed by Game Progressive Controllers, which will be existent
        /// across the creation and termination of multiple state machines due to
        /// paytable sliding or game mode switching.
        /// </remarks>
        void ClearEventSubscriptions();
    }
}
