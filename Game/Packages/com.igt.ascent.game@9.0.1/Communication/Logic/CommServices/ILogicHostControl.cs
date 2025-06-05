// -----------------------------------------------------------------------
// <copyright file = "ILogicHostControl.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    /// <summary>
    /// This interface defines APIs to start and stop a logic comm host.
    /// </summary>
    public interface ILogicHostControl
    {
        /// <summary>
        /// Clears any queued messages and allows new messages to be queued.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops any new messages from being queued until <see cref="Start"/> is called.
        /// </summary>
        void Stop();
    }
}