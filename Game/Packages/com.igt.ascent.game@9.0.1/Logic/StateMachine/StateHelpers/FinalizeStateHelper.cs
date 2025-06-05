//-----------------------------------------------------------------------
// <copyright file = "FinalizeStateHelper.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    ///   This class should be used to assist in the creation of a game's state machine to handle finalizing.
    /// </summary>
    public class FinalizeStateHelper
    {
        /// <summary>
        ///   Safe store results of Finalize Outcome Event
        /// </summary>
        /// <param name = "sender">Object sending event.</param>
        /// <param name = "eventArgs">Event args.</param>
        /// <exception cref="ArgumentNullException">Thrown if sender or eventArgs are null.</exception>
        /// <exception cref = "ArgumentException">Thrown if sender does not support IGameLib.</exception>
        public virtual void HandleFinalizeOutcomeEvent(object sender, FinalizeOutcomeEventArgs eventArgs)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }
            var gameLib = sender as IGameLib;
            if (gameLib != null)
            {
                gameLib.WriteCriticalData(CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.Finalized, true);
            }
            else
            {
                throw new ArgumentException("Sender must support IGameLib interface", "sender");
            }
        }

        ///<summary>
        ///   Tells the Foundation to finalize its outcome.
        ///</summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        ///<exception cref = "ArgumentNullException">Thrown if framework is null.</exception>
        public virtual void FinalizeOutcome(IStateMachineFramework framework)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            framework.GameLib.FinalizeOutcome();
        }

        ///<summary>
        /// Ends the current game cycle. uses the number of history steps returned by StateMachineBase.GetHistoryStepCount.
        ///</summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        ///<exception cref = "ArgumentNullException">Thrown if framework is null.</exception>
        public virtual void ProcessFinalizeOutcomeResponse(IStateMachineFramework framework)
        {
            if (framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            framework.ProcessEvents(
                () =>
                framework.GameLib.ReadCriticalData<bool>(CriticalDataScope.GameCycle,
                                                         StateHelperCriticalDataPaths.Finalized));
            framework.GameLib.RemoveCriticalData(CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.Finalized);
            framework.GameLib.EndGameCycle(StateMachineBase.GetHistoryStepCount(framework));
        }
    }
}
