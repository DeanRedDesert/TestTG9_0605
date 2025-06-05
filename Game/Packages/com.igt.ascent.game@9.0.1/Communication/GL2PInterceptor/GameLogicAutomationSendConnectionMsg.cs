//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendConnectionMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    ///<summary>
    /// This message class is used to send a msg telling the client its connected.
    ///</summary>
    [Serializable]
    public class GameLogicAutomationSendConnectionMsg : AutomationGenericMsg
    {
        ///<summary>
        /// Initialize a new instance of the class.
        ///</summary>
        public GameLogicAutomationSendConnectionMsg() {}
    }
}
