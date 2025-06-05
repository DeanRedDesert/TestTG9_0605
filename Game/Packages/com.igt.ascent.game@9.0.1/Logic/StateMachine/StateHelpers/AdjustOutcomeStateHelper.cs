//-----------------------------------------------------------------------
// <copyright file = "AdjustOutcomeStateHelper.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList;
    using Evaluator;
    using Evaluator.Schemas;

    /// <summary>
    ///   This class should be used in the creation of a state machine.  It is designed to handle the common adjust outcome functionality.
    ///   Use this class to take a win outcome and notify the foundation, as well as merging the response back into the outcome.
    /// </summary>
    public class AdjustOutcomeStateHelper
    {
        /// <summary>
        ///   Foundation Event handler that safe stores the results of outcome response event.
        /// </summary>
        /// <param name = "sender">Object sending event.</param>
        /// <param name = "eventArgs">Event args.</param>
        /// <exception cref="ArgumentNullException">Thrown if sender or eventArgs are null.</exception>
        /// <exception cref = "ArgumentException">Thrown if sender does not support IGameLib.</exception>
        public virtual void HandleOutcomeResponseEvent(object sender, OutcomeResponseEventArgs eventArgs)
        {
            if(sender == null)
            {
                throw new ArgumentNullException("sender");
            }

            if(eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }

            var gameLib = sender as IGameLib;
            if(gameLib != null)
            {
                gameLib.WriteCriticalData(CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.OutcomeResponse,
                                          eventArgs);
            }
            else
            {
                throw new ArgumentException("Sender must support IGameLib interface", "sender");
            }
        }

        /// <summary>
        ///   Creates feature entries from win outcomes and adjusts the outcome of the foundation.
        /// </summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        /// <param name = "winOutcomePackages">List of win outcome packages to adjust.</param>
        /// <param name = "winOutcomes">A list of modified winoutcomes made from win outcome packages.</param>
        /// <param name = "finalOutcome">Flag indicating if this is the last outcome of the game.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <exception cref = "ArgumentNullException">
        ///     Thrown if either <paramref name="framework"/> is null.
        /// </exception>
        public virtual void AdjustWinOutcome(IStateMachineFramework framework,
            IList<WinOutcomePackage> winOutcomePackages, out IList<WinOutcome> winOutcomes, bool finalOutcome,
            long denomination)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            var outcomeList = new OutcomeList(WinOutcomeUtilities.CreateFeatureEntries(winOutcomePackages, out winOutcomes, denomination));
            framework.GameLib.AdjustOutcome(outcomeList, finalOutcome);
        }

        /// <summary>
        /// Creates a feature entry from the given win outcome and adjusts the outcome of the foundation.
        /// </summary>
        /// <param name="framework">State machine framework containing GameLib.</param>
        /// <param name="winOutcomePackage">Win outcome package to adjust.</param>
        /// <param name="winOutcome">Modified winoutcome that should be used for merging the wins.</param>
        /// <param name="finalOutcome">Flag indicating if this is the last outcome of the game.</param>
        /// <param name="denomination">The denomination of the game.</param>
        ///         /// <exception cref = "ArgumentNullException">
        ///     Thrown if either <paramref name="framework"/>is null.
        /// </exception>
        public virtual void AdjustWinOutcome(IStateMachineFramework framework,
            WinOutcomePackage winOutcomePackage, out WinOutcome winOutcome, bool finalOutcome, long denomination)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            var outcomeList = new OutcomeList(new[] { WinOutcomeUtilities.CreateFeatureEntry(winOutcomePackage, out winOutcome, denomination) });
            framework.GameLib.AdjustOutcome(outcomeList, finalOutcome);
        }

        /// <summary>
        ///   Merges the outcome response with the passed in win outcome.  
        ///   This should be the same win outcome that was passed to the AdjustWinOutcome function.
        /// </summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        /// <param name = "winOutcome">
        ///   Win outcome that should have foundation outcome response merged into.
        ///   This should be the same as the WinOutcome passed into AdjustWinOutcome.
        /// </param>
        /// <exception cref = "ArgumentNullException">
        ///     Thrown if framework or winOutcome are null.
        /// </exception>
        /// <returns>Returns true if the system accepts that the passed in outcome is the last outcome of the game.</returns>
        public virtual bool ProcessOutcomeResponse(IStateMachineFramework framework, WinOutcome winOutcome)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }

            return ProcessOutcomeResponse(framework, new List<WinOutcome> { winOutcome });
        }

        /// <summary>
        ///   Merges the outcome response with the passed in list of win outcomes.
        ///   This should be the same list of win outcomes that were passed to the AdjustWinOutcome function.
        /// </summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        /// <param name = "winOutcomes">
        ///   Win outcomes that should have foundation outcome response merged into.
        ///   This should be the same as the win outcomes passed into AdjustWinOutcome.
        /// </param>
        /// <exception cref = "ArgumentNullException">
        ///     Thrown if framework or winoutcomes are null.
        /// </exception>
        /// <returns>Returns true if the system accepts that the passed in outcome is the last outcome of the game.</returns>
        public virtual bool ProcessOutcomeResponse(IStateMachineFramework framework, IList<WinOutcome> winOutcomes)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            if(winOutcomes == null)
            {
                throw new ArgumentNullException("winOutcomes");
            }

            OutcomeResponseEventArgs outcomeResponseEvent = null;

            framework.ProcessEvents(() =>
                                    (outcomeResponseEvent =
                                     framework.GameLib.ReadCriticalData<OutcomeResponseEventArgs>(
                                         CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.OutcomeResponse)) !=
                                    null);

            framework.GameLib.RemoveCriticalData(CriticalDataScope.GameCycle,
                                                 StateHelperCriticalDataPaths.OutcomeResponse);

            WinOutcomeUtilities.SafeMergeOutcomes(winOutcomes, outcomeResponseEvent.AdjustedOutcome);

            return outcomeResponseEvent.IsFinalOutcome;
        }
    }
}
