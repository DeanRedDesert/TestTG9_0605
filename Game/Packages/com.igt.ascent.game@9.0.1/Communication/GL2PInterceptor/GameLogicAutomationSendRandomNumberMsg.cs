//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendRandomNumberMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    ///<summary>
    ///This message acknowledge it received the request for the random numbers.
    ///</summary>
    [Serializable]
    public class GameLogicAutomationSendRandomNumberMsg : AutomationGenericMsg
    {
        ///<summary>
        ///Initialize a new instance of the class.
        ///</summary>
        public GameLogicAutomationSendRandomNumberMsg() { }
    }
}
