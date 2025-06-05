//-----------------------------------------------------------------------
// <copyright file = "StateManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using Communication.CommunicationLib;
    using Tracing;

    /// <summary>
    /// The State Manager is the presentation component which handles Start State Events and Asynchronous Data
    /// Update Events from the Game Logic. For each state that the Game Logic sends a Start State Event the
    /// presentation must have a handler to deal with that state and to deal with data updates during that state.
    /// When the State Manager receives an event from the Game Logic it will call into the state handler for the
    /// state associated with the event.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class StateManager : IPresentationStateManager
    {
        private static readonly ServiceAccessor HistoryModeAccessor = new ServiceAccessor("HistoryMode");

        /// <summary>
        /// Object used to query information on presentation content reset.
        /// </summary>
        private readonly IPresentationContentResetQuery presentationContentResetQuery;

        /// <summary>
        /// Object used to reset presentation content to its start-up state,
        /// typically by unloading and reloading scenes.
        /// </summary>
        private readonly IPresentationContentResetter presentationContentResetter;

        /// <summary>
        /// Flag indicating if the first state has been entered or not.
        /// </summary>
        private bool firstStateHasBeenEntered;

        #region Constructors

        /// <summary>
        /// Initialize an instance of StateManager with a GameLogicServiceClient.
        /// </summary>
        /// <param name="logicServiceClient">
        /// The game logic service client to use.
        /// </param>
        /// <param name="stateHandlerLocator">
        /// A reference to the current IStateHandlerLocator instance.
        /// </param>
        /// <param name="presentationContentResetter">
        /// A reference to an <see cref="IPresentationContentResetter"/>, to be used to reset presentation content.
        /// This parameter null by default.  If null, it will not be used.
        /// If it is not null, it will be used in place of <paramref name="presentationContentResetQuery"/>.
        /// </param>
        /// <param name="presentationContentResetQuery">
        /// A reference to an <see cref="IPresentationContentResetQuery"/>, to be used to query information on
        /// presentation reset.  This parameter is null by default.  If null, it will not be used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="logicServiceClient"/> or <paramref name="stateHandlerLocator"/> is null.
        /// </exception>
        public StateManager(IGameLogic logicServiceClient,
                            IStateHandlerLocator stateHandlerLocator,
                            IPresentationContentResetter presentationContentResetter = null,
                            IPresentationContentResetQuery presentationContentResetQuery = null)
        {
            gameLogicServiceClient = logicServiceClient ?? throw new ArgumentNullException(nameof(logicServiceClient));
            this.stateHandlerLocator = stateHandlerLocator ?? throw new ArgumentNullException(nameof(stateHandlerLocator));

            // If a resetter is present, it will take place of the reset query as well.
            this.presentationContentResetter = presentationContentResetter;
            this.presentationContentResetQuery = presentationContentResetter ?? presentationContentResetQuery;

            states = new Dictionary<string, IStateHandler>();
        }

        #endregion

        #region IPresentationStateManager Interface

        /// <inheritdoc />
        /// <DevDoc>
        ///    This function fills in the state name parameter with the currently executing state (<see cref = "CurrentState" />).
        ///    There should only ever be one presentation state executing at a time, so this should not cause any
        ///    problems.
        /// </DevDoc>
        /// <exception cref="IGT.Game.Core.Presentation.States.PresentationCompleteAlreadySentException">
        /// Thrown if <see cref="isPresentationComplete"/> is true.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <see cref="CurrentState"/> is null or empty.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.States.InvalidPresentationCompleteException">
        /// Thrown if <paramref name="stateName"/> does not match CurrentState.
        /// </exception>
        public void PresentationStateComplete(string stateName,
                                              string actionRequest,
                                              Dictionary<string, object> genericData = null)
        {
            if(string.IsNullOrEmpty(CurrentState))
            {
                throw new InvalidOperationException("Presentation complete cannot be called at this time, there is no current presentation state.");
            }

            if(CurrentState != stateName)
            {
                throw new InvalidPresentationCompleteException($"The presentation state {stateName} is trying to complete but is not the current state. The current state is {CurrentState}.");
            }

            if(isPresentationComplete)
            {
                throw new PresentationCompleteAlreadySentException(CurrentState,
                                                                   "A presentation complete message was already sent for the presentation state. " +
                                                                   "Only send one presentation complete message per presentation state.");
            }

            ExecuteOnPresentationComplete();

            gameLogicServiceClient.PresentationStateComplete(CurrentState, actionRequest, genericData);

            if(presentationContentResetter != null && historyMode && actionRequest == "FirstStep")
            {
                presentationContentResetter.RequestReset();
            }

            isPresentationComplete = true;
            historyMode = false;
        }

        /// <inheritdoc />
        public ICollection<string> States => states.Keys;

        /// <inheritdoc />
        /// <devdoc>
        /// The LogicDataEditor builds lists of service accessors for each state while in edit mode;
        /// when the game is ran each of these accessors needs to be added to the request controller of the
        /// correct state handler. This function allows the handlers to be accessed for this purpose.
        /// </devdoc>
        public IStateHandler GetStateHandler(string stateName)
        {
            if(!states.ContainsKey(stateName))
            {
                throw new UnknownPresentationStateException(stateName,
                                                            "Requested state does not exist.");
            }

            return states[stateName];
        }

        /// <inheritdoc />
        public void ExitContext()
        {
            // Reset pendingStartState in case the context is exited before a start state has been committed.
            // CommitStartState can be held up by reasons like presentation content is being reset etc.
            pendingStartState = string.Empty;

            ExecuteOnExit();
        }

        /// <inheritdoc />
        public void ResetFirstStateEnteredStatus()
        {
            firstStateHasBeenEntered = false;
        }

        /// <inheritdoc />
        public event EventHandler OnFirstStateEntered;

        #endregion

        #region Public Methods

        /// <summary>
        /// Enter the state handler function for the specified state.
        /// </summary>
        /// <remarks>
        /// This function would normally be registered with the Start State Event of the Game Logic API on the Game
        /// Logic to Presentation Interface.
        /// </remarks>
        /// <DevDoc>When this method is called the current state (<see cref = "CurrentState" />) property is set.</DevDoc>
        /// <param name = "stateName">Name of the presentation state to enter.</param>
        /// <param name = "stateData">Data required during execution of the state.</param>
        /// <exception cref = "ArgumentNullException">Method parameters cannot be null.</exception>
        /// <exception cref = "ArgumentException">Thrown if <paramref name = "stateName" /> is an empty string.</exception>
        /// <exception cref = "UnknownPresentationStateException">
        /// Thrown if the <paramref name = "stateName" /> requested has not been registered with the state manager, and
        /// the state manager has been told the scene is fully loaded.
        /// </exception>
        public void StartState(string stateName, DataItems stateData)
        {
            if(stateName == null)
            {
                throw new ArgumentNullException(nameof(stateName), "State name parameter cannot be null.");
            }

            if(stateName.Length <= 0)
            {
                throw new ArgumentException("State name parameter cannot be an empty string.", nameof(stateName));
            }

            pendingStartState = stateName;
            pendingStateData = stateData ?? throw new ArgumentNullException(nameof(stateData), "State handler parameter cannot be null.");
            if(!states.ContainsKey(stateName))
            {
                throw new UnknownPresentationStateException(stateName,
                                                            "States that are not registered with the State Manager cannot be used.");
            }

            if(presentationContentResetQuery != null &&
               (presentationContentResetQuery.ShouldReset || presentationContentResetQuery.ResettingContent))
            {
                presentationContentResetQuery.ContentReset += ContentReset;

                // Call OnExit so no presentation state is executing while content is being reset.
                // If ExitContext() has already exited the presentation state, this will do nothing.
                ExecuteOnExit();

                // If this state manager instance has the resetter responsibility, reset the content as needed.
                // Do not reset a second time if both ShouldReset and ResettingContent are true.
                if(presentationContentResetter != null && !presentationContentResetQuery.ResettingContent)
                {
                    presentationContentResetter.ResetContent();
                }
            }
            else
            {
                CheckAndCommitStartState();
            }
        }

        /// <summary>
        /// Handle notification that presentation content has completed resetting,
        /// and it is now safe to enter the presentation state.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">Event args.</param>
        protected void ContentReset(object sender, EventArgs eventArgs)
        {
            presentationContentResetQuery.ContentReset -= ContentReset;
            CheckAndCommitStartState();
        }

        /// <summary>
        /// Commit the start state, after verifying that the state can start.
        /// </summary>
        protected void CheckAndCommitStartState()
        {
            if(stateHandlerLocator.TryLoadStateHandler(pendingStartState) &&
               stateHandlerLocator.CanStartState(pendingStartState))
            {
                CommitStartState();
            }
        }

        /// <summary>
        /// Process pending start state.
        /// </summary>
        public void ProcessPendingState()
        {
            if(!string.IsNullOrEmpty(pendingStartState))
            {
                if(stateHandlerLocator.CanStartState(pendingStartState))
                {
                    CommitStartState();
                }
            }
        }

        /// <summary>
        /// Indicate to a state that data has been updated.
        /// </summary>
        /// <remarks>
        /// This function would normally be registered with the Update Asynchronous Data Event on the Game
        /// Logic to Presentation Interface.
        /// </remarks>
        /// <param name = "stateName">Name of the state that the data is intended for.</param>
        /// <param name = "data">Updated data for the specified state.</param>
        /// <exception cref = "ArgumentNullException">Method parameters cannot be null.</exception>
        /// <exception cref = "ArgumentException">Thrown if <paramref name = "stateName" /> is an empty string.</exception>
        /// <exception cref = "UnknownPresentationStateException">
        /// Thrown if the <paramref name = "stateName" /> requested has not been registered with the state manager.
        /// </exception>
        public void UpdateAsynchronousData(string stateName, DataItems data)
        {
            if(stateName == null)
            {
                throw new ArgumentNullException(nameof(stateName), "State name parameter cannot be null.");
            }

            if(stateName.Length <= 0)
            {
                throw new ArgumentException("State name parameter cannot be an empty string.", nameof(stateName));
            }

            if(data == null)
            {
                throw new ArgumentNullException(nameof(data), "State handler parameter cannot be null.");
            }

            if(!states.ContainsKey(stateName))
            {
                throw new UnknownPresentationStateException(stateName,
                                                            "States that are not registered with the State Manager cannot be used.");
            }

            //This happens in the update loop so the current state cannot change between here and the OnDataUpdate.
            //If the update was intended for a state which is no longer executing, then the update should be discarded.
            if(stateName == CurrentState)
            {
                states[CurrentState].OnDataUpdate(data);
            }

            // There is a chance we could still be loading the state so if we have a valid pending state we need to merge the data
            // with the pending state data. This prevents the state from missing any possible updates.
            else if(!string.IsNullOrEmpty(pendingStartState) && pendingStartState == stateName)
            {
                MergeDataItems(pendingStateData, data);
            }
        }

        /// <summary>
        /// Registers a state handler using the given <see cref="StateHandlerRegistry"/> object.
        /// </summary>
        /// <param name="handlerRegistry">An object containing all of the required registration information.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="handlerRegistry"/> is <b>null</b>.
        /// </exception>
        public void RegisterStateHandler(StateHandlerRegistry handlerRegistry)
        {
            if(handlerRegistry == null)
            {
                throw new ArgumentNullException(nameof(handlerRegistry));
            }

            var stateHandler = stateHandlerLocator.CreateStateHandler(handlerRegistry);
            RegisterStateHandler(handlerRegistry.StateName, stateHandler);
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder().AppendLine(base.ToString())
                                             .AppendLine($"Current State: {CurrentState ?? "null"}");

            builder = states.Count == 0
                          ? builder.AppendLine("State list is empty.")
                          : states.Aggregate(builder, (sb, kvp) => sb.AppendLine($"Handlers[{kvp.Key}] = {kvp.Value}"));

            return builder.ToString();
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Get or Set the presentation state that is currently running.
        /// </summary>
        protected string CurrentState { get; set; }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the State list
        /// </summary>
        /// <remarks>
        /// This is internal because it should only be used by tests.
        /// </remarks>
        internal Dictionary<string, IStateHandler> StateList => states.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        #endregion

        #region Private Members

        /// <summary>
        /// A dictionary of state name and state handler pair.
        /// </summary>
        private readonly Dictionary<string, IStateHandler> states;

        /// <summary>
        /// The instance of game logic service.
        /// </summary>
        private readonly IGameLogic gameLogicServiceClient;

        /// <summary>
        /// The instance of <see cref="IStateHandlerLocator"/>.
        /// </summary>
        private readonly IStateHandlerLocator stateHandlerLocator;

        /// <summary>
        /// If in history mode.
        /// </summary>
        private bool historyMode;

        /// <summary>
        /// Name of pending start state request.
        /// </summary>
        private string pendingStartState;

        /// <summary>
        /// Flag to check if PresentationComplete was called on a state.
        /// </summary>
        private bool isPresentationComplete;

        /// <summary>
        /// Pending start state data that was cached in the StartState function.
        /// </summary>
        private DataItems pendingStateData;


        /// <summary>
        /// Commits a pending start state request.
        /// </summary>
        /// <exception cref="UnknownPresentationStateException">
        /// Thrown when there is no handler for the pending state
        /// </exception>
        /// <exception cref = "InvalidCastException">
        /// Thrown if the pending state is a history state, but does not support the 
        /// <see cref="IHistoryPresentationState"/> interface.
        /// </exception>
        private void CommitStartState()
        {
            if(!string.IsNullOrEmpty(pendingStartState))
            {
                if(!states.ContainsKey(pendingStartState))
                {
                    throw new UnknownPresentationStateException(pendingStartState,
                                                                "States that are not registered with the State Manager cannot be used.");
                }

                // Call OnExit before we enter a new presentation state.
                ExecuteOnExit();

                isPresentationComplete = false;

                var historyData = pendingStateData.HistoryData;
                historyMode = historyData != null && (bool)historyData[HistoryModeAccessor.Identifier];

                // Presentation state which is involved in History mode must support IHistoryPresentationState.
                if(historyMode && !(states[pendingStartState] is IHistoryPresentationState))
                {
                    throw new InvalidCastException($"{pendingStartState} is involved in History Mode. It must support IHistoryPresentationState.");
                }

                // Set the current state before it is entered. Since there cannot be any other states
                // running at this point, it is OK to consider the state running now.
                CurrentState = pendingStartState;
                ExecuteOnEnter();
                pendingStartState = string.Empty;

                if(!firstStateHasBeenEntered)
                {
                    firstStateHasBeenEntered = true;

                    OnFirstStateEntered?.Invoke(this, EventArgs.Empty);

                    GameLifeCycleTracing.Log.FirstPresentationStateEntered(CurrentState);
                }
            }
        }

        /// <summary>
        /// Execute the OnExit function for the state.
        /// </summary>
        private void ExecuteOnExit()
        {
            if(!string.IsNullOrEmpty(CurrentState))
            {
                states[CurrentState].OnExit();
                historyMode = false;
                CurrentState = null;
            }
        }

        /// <summary>
        /// Execute the OnEnter function for the state.
        /// </summary>
        private void ExecuteOnEnter()
        {
            states[pendingStartState].OnEnter(pendingStateData);
        }

        /// <summary>
        /// Execute the OnPresentationComplete function for the state.
        /// </summary>
        private void ExecuteOnPresentationComplete()
        {
            states[CurrentState].OnPresentationComplete();
        }

        /// <summary>
        /// The RegisterStateHandler function registers a state handler object for a state.
        /// When a start state or update message call is made for the state it will be directed to the
        /// registered handler object.
        /// </summary>
        /// <param name = "stateName">Name of the state to register the handler with.</param>
        /// <param name = "stateHandler">Handler object to register.</param>
        /// <exception cref = "ArgumentException">
        /// Thrown if <paramref name = "stateName" /> is <b>null</b> or an empty string.
        /// </exception>
        /// <exception cref = "DuplicatePresentationStatesException">
        /// Thrown if the <paramref name = "stateName" /> passed has already been registered with the state manager.
        /// </exception>
        private void RegisterStateHandler(string stateName, IStateHandler stateHandler)
        {
            if(string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentException("The state name parameter cannot be null or empty.");
            }

            if(states.ContainsKey(stateName))
            {
                throw new DuplicatePresentationStatesException(stateName,
                                                               "State is already defined. States must have unique names.");
            }

            stateHandler.StateManager = this;
            states.Add(stateName, stateHandler);
        }

        /// <summary>
        /// This function takes additional items and add/updates any that are in the
        /// current list.
        /// </summary>
        /// <param name = "currentItems">The current data items.</param>
        /// <param name = "additionalItems">The data items to be updated/added.</param>
        private void MergeDataItems(DataItems currentItems, DataItems additionalItems)
        {
            foreach(var providerKey in additionalItems.Keys.ToList())
            {
                //If the current items don't have this provider add it to the list.
                if(!currentItems.ContainsKey(providerKey))
                {
                    currentItems.Add(providerKey, additionalItems[providerKey]);
                }
                else
                {
                    // If current items have the same provider then override the data.
                    // The data that should be overridden should only be what members were given by
                    // additional items. 
                    foreach(var memberKey in additionalItems[providerKey].Keys.ToList())
                    {
                        if(currentItems[providerKey].ContainsKey(memberKey))
                        {
                            currentItems[providerKey][memberKey] = additionalItems[providerKey][memberKey];
                        }
                    }
                }
            }
        }

        #endregion
    }
}