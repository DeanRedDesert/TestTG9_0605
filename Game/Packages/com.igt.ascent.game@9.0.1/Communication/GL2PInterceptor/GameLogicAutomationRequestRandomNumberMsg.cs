//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationRequestRandomNumberMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;

    ///<summary>
    ///This message request random numbers to be placed in the RandomNumberProxy.
    ///</summary>
    [Serializable]
    public class GameLogicAutomationRequestRandomNumberMsg : AutomationGenericMsg
    {
        ///<summary>
        ///Initializes a new instance of the class.
        ///</summary>
        public GameLogicAutomationRequestRandomNumberMsg() {}

        ///<summary>
        ///Initializes a new instance of the class with the IEnumberable object.
        ///</summary>
        ///<param name="randomNumberList"> Enumberable of random numbers</param>
        public GameLogicAutomationRequestRandomNumberMsg(IEnumerable<int> randomNumberList)
        {
            RandomNumberList = randomNumberList;
        }

        ///<summary>
        /// Stores the RandomNumberList sent in the message.
        ///</summary>
        public IEnumerable<int> RandomNumberList { private set; get; }
    }
}
