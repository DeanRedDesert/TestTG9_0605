//-----------------------------------------------------------------------
// <copyright file = "StateMachineSegment.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using Communication.CommunicationLib;
    using Evaluator.Schemas;

    /// <summary>
    /// Base class for game feature state machines. Multiple StateMachineSegments are intended to be joined together
    /// to form an overall game state machine.
    /// </summary>
    public abstract class StateMachineSegment
    {
        #region Public Properties

        /// <summary>
        /// Reference to the primary state machine for the game.
        /// </summary>
        public StateMachineBase GameStateMachine { protected set; get; }

        /// <summary>
        /// Upon completing the states within this segment this state will be entered.
        /// </summary>
        public string ReturnState { set; get; }

        /// <summary>
        /// The currently active paytable.
        /// </summary>
        public Paytable Paytable => GameStateMachine.Paytable;

        /// <summary>
        /// Initial state within this segment.
        /// </summary>
        private string initialState;

        /// <summary>
        /// Initial state to enter in this segment. The game state machine should use this as the entry point for the
        /// segment.
        /// </summary>
        public string InitialState
        {
            set
            {
                if (!GameStateMachine.States.Contains(value))
                {
                    throw new InvalidStateException(value, "Initial state must be a configured state.");
                }

                initialState = value;
            }

            get => initialState;
        }

        #endregion

        /// <summary>
        /// Base constructor for state machine segments.
        /// </summary>
        /// <param name="gameStateMachine">Main game state machine.</param>
        protected StateMachineSegment(StateMachineBase gameStateMachine)
        {
            GameStateMachine = gameStateMachine;
        }

        /// <summary>
        /// Exclude the current history step from the history.
        /// This can only be called within StateStage.Committed.
        /// 
        /// This will typically be used when the state machine exits a state and then
        /// re-enters that same state due to a non-game flow change, e.g. a language change.
        /// </summary>
        protected void ExcludeCurrentHistoryStep()
        {
            GameStateMachine.ExcludeCurrentHistoryStep();
        }

        #region Public Methods

        /// <summary>
        /// Initialize the StateMachine. Providers should be created and initialized in this function.
        /// </summary>
        /// <param name="framework">State machine framework which hosts a service controller.</param>
        public virtual void Initialize(IStateMachineFramework framework)
        {
        }

        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="saveHistory">
        /// Flag which indicates that this state is to be saved for history.
        /// </param>
        public void CreateState(string stateName, StateStageHandler processing, StateStageHandler committed,
                                    bool saveHistory)
        {
            GameStateMachine.CreateState(stateName, processing, committed, saveHistory);
        }


        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="historySettings">
        /// History related settings.
        /// </param>
        public void CreateState(string stateName, StateStageHandler processing, StateStageHandler committed,
                            HistorySettings historySettings)
        {
            GameStateMachine.CreateState(stateName, processing, committed, historySettings);
        }

        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="writeStartStateHistoryHandler">
        /// A delegate which handles writing of start state message data for history.
        /// If the argument is null, then standard serialization will be used for StartState messages.
        /// </param>
        /// <param name="writeUpdateDataHistoryHandler">
        /// A delegate which handles writing of asynchronous update messages for history.
        /// If the argument is null, then no history will be written for updates.
        /// </param>
        public void CreateState(string stateName, StateStageHandler processing, StateStageHandler committed,
                                    WriteStartStateHistoryHandler writeStartStateHistoryHandler,
                                    WriteUpdateDataHistoryHandler writeUpdateDataHistoryHandler)
        {
            GameStateMachine.CreateState(stateName, processing, committed, writeStartStateHistoryHandler,
                                           writeUpdateDataHistoryHandler);
        }

        /// <summary>
        /// Get the data required for a presentation state.
        /// </summary>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <param name="stateName">The name of the state to negotiate data for.</param>
        /// <returns>The data required for the specified presentation state.</returns>
        /// TODO: does this API need updated? stateName is unused
        public DataItems GetStateData(IStateMachineFramework framework, string stateName)
        {
            return GameStateMachine.GetStateData(framework);
        }

        /// <summary>
        /// Convenience function for starting a presentation state. Sends a start state message for
        /// the currently executing state.
        /// </summary>
        /// <param name="framework">The state machine framework executing the state machine.</param>
        public virtual void StartPresentationState(IStateMachineFramework framework)
        {
            GameStateMachine.StartPresentationState(framework);
        }

        #endregion
    }

}
