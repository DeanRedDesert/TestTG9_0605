//-----------------------------------------------------------------------
// <copyright file = "IAutoPlayCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    /// <summary>
    /// Interface to accept callbacks from the auto play category.
    /// </summary>
    public interface IAutoPlayCategoryCallbacks
    {
        /// <summary>
        /// Method called when the foundation initiates a request to turn on the auto play.
        /// </summary>
        /// <returns>True if the game allows to turn on the auto play.</returns>
        bool ProcessAutoPlayOnRequest();

        /// <summary>
        /// Method called when the foundation notifies the game 
        /// that the auto play has been turned off.
        /// </summary>
        void ProcessAutoPlayOff();
    }
}
