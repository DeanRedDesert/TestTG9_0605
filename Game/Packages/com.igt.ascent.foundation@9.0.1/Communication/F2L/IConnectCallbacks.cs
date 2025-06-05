//-----------------------------------------------------------------------
// <copyright file = "IConnectCallbacks.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    /// <summary>
    /// Interface for callback methods supported by the F2L connect category.
    /// </summary>
    public interface IConnectCallbacks
    {
        /// <summary>
        /// Method which indicates that the game is to shutdown.
        /// </summary>
        void ProcessShutDown();
    }
}
