//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationRequestPayTableMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    ///<summary>
    /// This message class is used to send a msg requesting a paytable.
    ///</summary>
    [Serializable]
    public class GameLogicAutomationRequestPaytableMsg : AutomationGenericMsg
    {
        ///<summary>
        ///Initializes a new instance of the class.
        ///</summary>
        public GameLogicAutomationRequestPaytableMsg() { }
    }
}
