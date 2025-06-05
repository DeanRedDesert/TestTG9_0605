//-----------------------------------------------------------------------
// <copyright file = "EnrollCommitStateHelper.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    ///   This class should be used to assist in the creation of a games state machine to handle enrollment.
    /// </summary>
    public class EnrollCommitStateHelper
    {
        /// <summary>
        ///   Safe store results of the enroll response event.
        /// </summary>
        /// <param name = "sender">Object sending event.</param>
        /// <param name = "eventArgs">Event args.</param>
        /// <exception cref="ArgumentNullException">Thrown if sender or eventArgs are null.</exception>
        /// <exception cref = "ArgumentException">Thrown if sender does not support IGameLib.</exception>
        public virtual void HandleEnrollGameCycle(object sender, EnrollResponseEventArgs eventArgs)
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
                gameLib.WriteCriticalData(CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.EnrollResponse,
                                          eventArgs);
            }
            else
            {
                throw new ArgumentException("Sender must support IGameLib interface", "sender");
            }
        }

        ///<summary>
        /// Enroll a game cycle.
        ///</summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        ///<exception cref = "ArgumentNullException">Thrown if framework is null.</exception>
        public virtual void EnrollGameCycle(IStateMachineFramework framework)
        {
            if (framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            //The payload is not typically used.
            framework.GameLib.EnrollGameCycle(new byte[0]);
        }

        ///<summary>
        /// Handles an enroll response and advances the game to the playing state.  If the game cannot start playing
        /// this function will unenroll and uncommit the bet.
        ///</summary>
        /// <param name = "framework">State machine framework containing GameLib.</param>
        ///<returns>True if game successfully moved to the playing foundation state.</returns>
        ///<exception cref = "ArgumentNullException">Thrown if framework is null.</exception>
        public virtual bool ProcessEnrollGameCycleResult(IStateMachineFramework framework)
        {
            if (framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            EnrollResponseEventArgs enrollResponseEvent = null;
            framework.ProcessEvents(() =>
                                    (enrollResponseEvent =
                                     framework.GameLib.ReadCriticalData<EnrollResponseEventArgs>(
                                         CriticalDataScope.GameCycle, StateHelperCriticalDataPaths.EnrollResponse)) !=
                                    null);

            var moveToPlaying = false;
            if (enrollResponseEvent.EnrollSuccess)
            {
                framework.GameLib.PlaceStartingBet();
                moveToPlaying = framework.GameLib.StartPlaying();
            }

            //If enrollment was not successful, or the game could not be started, then return to idle.
            if (!moveToPlaying)
            {
                framework.GameLib.UncommitBet();
                framework.GameLib.UnenrollGameCycle();
            }

            return moveToPlaying;
        }
    }
}
