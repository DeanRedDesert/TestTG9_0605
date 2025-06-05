//-----------------------------------------------------------------------
// <copyright file = "GameLogicAutomationSendCellPopulationOutcomeMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Core.Logic.Evaluator.Schemas;

    /// <summary>
    /// Message object that is comprised of the parameters of the IGameLogicAutomationService
    /// interface function SendCellPopulationOutcome.
    /// </summary>
    [Serializable]
    public class GameLogicAutomationSendCellPopulationOutcomeMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private GameLogicAutomationSendCellPopulationOutcomeMsg()
        {
        }

        /// <summary>
        /// Constructor for creating GameLogicAutomationSendCellPopulationOutcomeMsg.
        /// </summary>
        /// <param name="cellPopulationOutcome">Generated CellPopulationOutcome.</param>
        /// <param name="state">State where cellPopulationOutcome was generated.</param>
        /// <param name="description">Description of the cellPopulationOutcome e.g. Free Spin Symbol Window.</param>
        public GameLogicAutomationSendCellPopulationOutcomeMsg(CellPopulationOutcome cellPopulationOutcome,
                                                               string state,
                                                               string description)
        {
            State = state;
            Description = description;
            CellPopulationOutcome = cellPopulationOutcome;
        }

        /// <summary>
        /// State where CellPopulationOutcome was generated.
        /// </summary>
        public string State { private set; get; }

        /// <summary>
        /// Description of the CellPopulationOutcome e.g. Free Spin Symbol Window.
        /// </summary>
        public string Description { private set; get; }

        /// <summary>
        /// Generated CellPopulationOutcome.
        /// </summary>
        public CellPopulationOutcome CellPopulationOutcome { private set; get; }
    }
}
