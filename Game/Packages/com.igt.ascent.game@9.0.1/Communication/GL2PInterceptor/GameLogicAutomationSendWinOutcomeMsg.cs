//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendWinOutcomeMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Core.Logic.Evaluator.Schemas;

    /// <summary>
    /// Message object that is comprised of the parameters of the IGameLogicAutomationService
    /// interface function SendWinOutcome.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationSendWinOutcomeMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private GameLogicAutomationSendWinOutcomeMsg()
        {
        }

        /// <summary>
        /// Constructor for creating GameLogicAutomationSendWinOutcomeMsg.
        /// </summary>
        /// <param name="winOutcome">Results of an evaluation.</param>
        /// <param name="state">State where winOutcome was generated.</param>
        /// <param name="description">Description of the winOutcome e.g. Free Spin Evaluation.</param>
        public GameLogicAutomationSendWinOutcomeMsg(WinOutcome winOutcome, string state, string description)
        {
            State = state;
            Description = description;
            WinOutcome = winOutcome;
        }

        /// <summary>
        /// State where WinOutcome was generated.
        /// </summary>
        public string State { private set; get; }

        /// <summary>
        /// Description of the WinOutcome e.g. Free Spin Evaluation.
        /// </summary>
        public string Description { private set; get; }

        /// <summary>
        /// Results of an evaluation.
        /// </summary>
        public WinOutcome WinOutcome { private set; get; }
    }
}
