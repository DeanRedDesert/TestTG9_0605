//-----------------------------------------------------------------------
// <copyright file = "HistoryStateMachine.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication.CommunicationLib;
    using Services;

    /// <summary>
    /// This class provides the functionality required to load a history step and notify the presentation.
    /// </summary>
    public class HistoryStateMachine : StateMachineBase
    {
        #region Fields and Constants

        /// <summary>
        /// The name of the last presentation state that was executing.
        /// </summary>
        protected string LastKnownPresentationState;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor creates a state machine with one state to display history.
        /// </summary>
        public HistoryStateMachine()
            : base(false)  // recoveryEnabled = false for history.
        {
            RecoverySegment = new RecoverySegment(this, RecoverySegment.RecoverySegmentFunction.History);
            InitialState = RecoverySegment.InitialState;
        }

        #endregion

        #region StateMachineBase Overrides

        /// <inheritDoc />
        public override void WriteStartStateHistory(string state, DataItems data, int historyStep,
                                                    IStateMachineFramework framework)
        {
            //Do not write history when in history.
        }

        /// <inheritDoc />
        public override bool WriteUpdateHistory(string state, DataItems data, int historyStep,
                                                IStateMachineFramework framework)
        {
            //Do not write history when in history.
            return false;
        }

        /// <inheritdoc />
        protected override void UpdateAsyncData(IStateMachineFramework framework,
                                                StartFillAsynchronousRequestEventArgs asyncEventArgs)
        {
            if(!string.IsNullOrEmpty(LastKnownPresentationState))
            {
                var asyncData = GetAsyncHistoryStateData(framework, LastKnownPresentationState, asyncEventArgs);
                RecoverySegment.UpdateAsyncData(LastKnownPresentationState, framework, asyncData);
            }
        }

        /// <inheritdoc />
        public override CommonHistoryBlock ReadCommonHistoryBlock(IStateMachineFramework framework, int stepNumber)
        {
            var historyBlock = base.ReadCommonHistoryBlock(framework, stepNumber);

            LastKnownPresentationState = historyBlock.StateName;
            return historyBlock;
        }

        /// <inheritdoc />
        public override void UnregisterGameLibEvents(IGameLib gameLib)
        {
        }

        #endregion
    }
}
