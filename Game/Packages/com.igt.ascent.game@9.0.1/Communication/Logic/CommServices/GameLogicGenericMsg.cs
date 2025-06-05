//-----------------------------------------------------------------------
// <copyright file = "GameLogicGenericMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System;

    /// <summary>
    /// Base Class for all messages that will be put into the
    /// Game Logic Host Message Queue.
    /// </summary>
    [Serializable]
    public abstract class GameLogicGenericMsg : EventArgs
    {
        /// <summary>
        /// Get the type of the message.
        /// </summary>
        public abstract GameLogicMessageType MessageType { get; }
    }
}
