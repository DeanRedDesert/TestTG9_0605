//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendPayTableMsg.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Core.Logic.Evaluator.Schemas;

    ///<summary>
    ///This message class is responsible for sending the paytable.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationSendPaytableMsg : AutomationGenericMsg
    {
        ///<summary>
        ///Empty Constructor.
        ///</summary>
        public GameLogicAutomationSendPaytableMsg() { }

        ///<summary>
        ///Constructor that stores in the Paytable.
        ///</summary>
        /// <param name="payTableName">payTable sent in message.</param>
        public GameLogicAutomationSendPaytableMsg(string payTableName)
        {
            PaytableFileName = payTableName;
            Paytable = Paytable.Load(PaytableFileName);
        }

        ///<summary>
        ///Stores the paytable to be sent.
        ///</summary>
        public string PaytableFileName { private set; get; }

        /// <summary>
        /// Stores the pay table to send.
        /// </summary>
        public Paytable Paytable { private set; get; }
    }
}
