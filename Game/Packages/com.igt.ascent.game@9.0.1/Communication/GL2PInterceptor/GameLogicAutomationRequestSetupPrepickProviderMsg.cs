//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationRequestSetupPrepickProviderMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    ///<summary>
    ///This message is to request a pre-picked value provider to be setup.
    ///</summary>
    [Serializable]
    public class GameLogicAutomationRequestSetupPrepickProviderMsg : AutomationGenericMsg
    {
        ///<summary>
        ///Initializes a new instance of the class.
        ///</summary>
        public GameLogicAutomationRequestSetupPrepickProviderMsg() { }
    }
}
