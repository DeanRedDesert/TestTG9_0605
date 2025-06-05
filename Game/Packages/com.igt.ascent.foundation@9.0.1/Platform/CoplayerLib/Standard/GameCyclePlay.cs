//-----------------------------------------------------------------------
// <copyright file = "GameCyclePlay.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using OutcomeList.Interfaces;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;
    using F2XOutcomeList = Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay.OutcomeList;
    using F2XWagerCatOutcomes = Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay.EvalOutcomeRequestSendWagerCatOutcomes;

    /// <summary>
    /// Implementation of the <see cref="IGameCyclePlay"/> interface that is backed by the F2X.
    /// </summary>
    internal class GameCyclePlay : IGameCyclePlay, IGameCyclePlayRestricted, IContextCache<ICoplayerContext>
    {
        #region Private Members

        /// <summary>
        /// The object to put as the sender of all events raised by this class.
        /// </summary>
        private readonly object eventSender;

        /// <summary>
        /// The cached interface of the GameCyclePlay category.
        /// </summary>
        private readonly CategoryInitializer<IGameCyclePlayCategory> gameCyclePlayCategory;

        /// <summary>
        /// Back end field for the GameCycleState property.
        /// </summary>
        private GameCycleState gameCycleState;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="GameCyclePlay"/>.
        /// </summary>
        /// <param name="eventSender">
        /// The object to put as the sender of all events raised by this class.
        /// If null, this instance will be put as the sender.
        /// 
        /// This is so that the event handlers can cast sender to
        /// ICoplayerLib if needed, e.g. writing critical data.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <param name="nonTransactionalEventDispatcher">
        /// Interface for processing a non-transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments except <paramref name="eventSender"/> is null.
        /// </exception>
        public GameCyclePlay(object eventSender,
                             IEventDispatcher transactionalEventDispatcher,
                             IEventDispatcher nonTransactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            if(nonTransactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(nonTransactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;

            gameCyclePlayCategory = new CategoryInitializer<IGameCyclePlayCategory>();

            CreateEventLookupTable();
            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
            nonTransactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;

            OutcomeResponseReadyEvent += HandleOutcomeResponseReadyEvent;
            EnrollResponseReadyEvent += HandleEnrollResponseReadyEvent;
            FinalizeOutcomeEvent += HandleFinalizeOutcomeEvent;
            AbortCompleteEvent += HandleAbortCompleteEvent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The interface of the game cycle play category.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IGameCyclePlayCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            gameCyclePlayCategory.Initialize(category);
        }

        #endregion

        #region IGameCyclePlay Implementation

        /// <inheritdoc />
        public event EventHandler<AbortCompleteEventArgs> AbortCompleteEvent;

        /// <inheritdoc />
        public event EventHandler<EnrollResponseReadyEventArgs> EnrollResponseReadyEvent;

        /// <inheritdoc />
        public event EventHandler<OutcomeResponseReadyEventArgs> OutcomeResponseReadyEvent;

        /// <inheritdoc />
        public event EventHandler<FinalizeOutcomeEventArgs> FinalizeOutcomeEvent;

        /// <inheritdoc />
        /// <devdoc>
        /// All game cycle transitions are associated with either a function call into the foundation or with an event
        /// from the foundation. This allows for the value of the current game cycle state to be managed by the game.
        /// When the game calls a function which transitions the state, it checks for success and then updates its
        /// local state. When a message, indicating a state transition, is sent from the foundation, then it updates
        /// the state correspondingly. This technically means that the transitions are behind the foundation
        /// transitions, but given the state machine and transaction model this should not create any issues.
        /// The state needs to be updated before control is passed back to any game specific code.
        /// </devdoc>
        public GameCycleState GameCycleState
        {
            get => gameCycleState;
            private set
            {
                if(gameCycleState != value)
                {
                    var fromState = gameCycleState;
                    gameCycleState = value;

                    // Raise the event AFTER the property has been updated.  This is more consistent with the "transitioned" naming.
                    GameCycleStateTransitioned?.Invoke(this, new GameCycleStateTransitionedEventArgs(fromState, gameCycleState));
                }
            }
        }

        /// <inheritdoc />
        public bool CommitGameCycle()
        {
            // TODO ETG: Add transaction verification!!

            var success = gameCyclePlayCategory.Instance.CommitGameCycle();
            if(success)
            {
                GameCycleState = GameCycleState.Committed;
            }
            return success;
        }

        /// <inheritdoc />
        public void UncommitGameCycle()
        {
            // TODO ETG: Add transaction verification!!

            gameCyclePlayCategory.Instance.UncommitGameCycle();
            GameCycleState = GameCycleState.Idle;
        }

        /// <inheritdoc />
        public void EnrollGameCycle(byte[] enrollmentData = null)
        {
            // TODO ETG: Add transaction verification!!

            gameCyclePlayCategory.Instance.EnrollGameCycle(enrollmentData ?? new byte[0]);
            GameCycleState = GameCycleState.EnrollPending;
        }

        /// <inheritdoc />
        public void UnenrollGameCycle()
        {
            // TODO ETG: Add transaction verification!!

            gameCyclePlayCategory.Instance.UnenrollGameCycle();
            GameCycleState = GameCycleState.Idle;
        }

        /// <inheritdoc />
        public void StartPlaying()
        {
            gameCyclePlayCategory.Instance.StartPlaying();
            GameCycleState = GameCycleState.Playing;
        }

        /// <inheritdoc />
        public void AdjustOutcome(IOutcomeList outcomeList)
        {
            if(outcomeList == null)
            {
                throw new ArgumentNullException(nameof(outcomeList));
            }

            // TODO ETG: Add transaction verification!!

            GameCycleState = AdjustOutcomeForState(outcomeList, GameCycleState, false, null);
        }

        /// <inheritdoc />
        public void AdjustLastOutcome(IOutcomeList outcomeList, IList<WagerCategoryOutcome> wagerCatOutcomeList)
        {
            if(outcomeList == null)
            {
                throw new ArgumentNullException(nameof(outcomeList));
            }

            if(wagerCatOutcomeList?.Any() != true)
            {
                throw new ArgumentException("Wager category outcome list may not be null or empty.", nameof(wagerCatOutcomeList));
            }

            // TODO ETG: Add transaction verification!!

            GameCycleState = AdjustOutcomeForState(outcomeList, GameCycleState, true, wagerCatOutcomeList);
        }

        /// <inheritdoc />
        public void FinalizeOutcome()
        {
            // TODO ETG: Add transaction verification!!

            gameCyclePlayCategory.Instance.FinalizeAwardRequest();
            GameCycleState = GameCycleState.FinalizeAwardPending;
        }

        /// <inheritdoc />
        public bool AbortGameCycle()
        {
            // TODO ETG: Add transaction verification!!

            var result = gameCyclePlayCategory.Instance.AbortGameCycle();
            // If the abort request is not accepted, the current game cycle state remains unchanged.
            if (result)
            {
                GameCycleState = GameCycleState.AbortPending;
            }
            return result;
        }

        /// <inheritdoc />
        public void EndGameCycle(int numberOfSteps)
        {
            // TODO ETG: Add transaction verification!!

            gameCyclePlayCategory.Instance.EndGameCycle(Convert.ToUInt32(numberOfSteps));
            GameCycleState = GameCycleState.Idle;
        }

        /// <inheritdoc />
        public EnrollResponseData GetEnrollResponseData()
        {
            // TODO ETG: Add transaction verification!!

            var responseData = gameCyclePlayCategory.Instance.EnrollResponseData();

            var result = new EnrollResponseData(responseData.IsReady,
                                                responseData.EnrollSuccessSpecified ? (bool?)responseData.EnrollSuccess : null,
                                                responseData.HostEnrollmentData);

            if(result.IsReady && GameCycleState == GameCycleState.EnrollPending)
            {
                GameCycleState = GameCycleState.EnrollComplete;
            }

            return result;
        }

        /// <inheritdoc />
        public OutcomeResponseData GetOutcomeResponseData()
        {
            // TODO ETG: Add transaction verification!!

            var responseData = gameCyclePlayCategory.Instance.EvalOutcomeResponseData();

            var result = new OutcomeResponseData(responseData.IsReady,
                                                 responseData.IsLastOutcome,
                                                 responseData.OutcomeList);

            if(result.IsReady && GameCycleState == GameCycleState.EvaluatePending)
            {
                GameCycleState = result.IsLastOutcome
                                     ? GameCycleState.MainPlayComplete
                                     : GameCycleState.Playing;
            }

            return result;
        }

        #endregion

        #region IGameCyclePlayRestricted Implementation

        /// <inheritdoc />
        public event EventHandler<GameCycleStateTransitionedEventArgs> GameCycleStateTransitioned;

        #endregion

        #region IContextCache

        /// <inheritdoc />
        /// <remarks>
        /// Query the game cycle state from the Foundation to initialize the cached game cycle state.
        /// </remarks>
        public void NewContext(ICoplayerContext coplayerContext)
        {
            GameCycleState = (GameCycleState)gameCyclePlayCategory.Instance.QueryGameCycleState();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Actions performed when a Foundation event needs processing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatchedEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            if(eventTable.ContainsKey(eventArgs.DispatchedEventType))
            {
                var handler = eventTable[eventArgs.DispatchedEventType];
                if(handler != null)
                {
                    handler(eventSender, eventArgs.DispatchedEvent);

                    eventArgs.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Create the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(AbortCompleteEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, AbortCompleteEvent);
            eventTable[typeof(EnrollResponseReadyEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, EnrollResponseReadyEvent);
            eventTable[typeof(OutcomeResponseReadyEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, OutcomeResponseReadyEvent);
            eventTable[typeof(FinalizeOutcomeEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, FinalizeOutcomeEvent);
        }

        /// <summary>
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The object posting the event.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="eventHandler">The handler for the event.</param>
        private static void ExecuteHandler<TEventArgs>(object sender, EventArgs eventArgs,
                                                       EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// Handle the abort complete event from the foundation. Used for managing game cycle state caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="abortCompleteEventArgs">The data associated with the event.</param>
        private void HandleAbortCompleteEvent(object sender, AbortCompleteEventArgs abortCompleteEventArgs)
        {
            GameCycleState = GameCycleState.Finalized;
        }

        /// <summary>
        /// Handle the finalize outcome event from the foundation. Used for managing game cycle state caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="finalizeOutcomeEventArgs">The data associated with the event.</param>
        private void HandleFinalizeOutcomeEvent(object sender, FinalizeOutcomeEventArgs finalizeOutcomeEventArgs)
        {
            GameCycleState = GameCycleState.Finalized;
        }

        /// <summary>
        /// Handle the enroll response event. Used for managing the game cycle state caching as well as recovery
        /// for the committed bet caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="enrollResponseEventArgs">The data associated with the event.</param>
        private void HandleEnrollResponseReadyEvent(object sender, EnrollResponseReadyEventArgs enrollResponseEventArgs)
        {
            if(GameCycleState == GameCycleState.EnrollPending)
            {
                GameCycleState = GameCycleState.EnrollComplete;
            }
        }

        /// <summary>
        /// Handle the outcome response event from the foundation. Used for managing game cycle state caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="outcomeResponseReadyEventArgs">
        /// Outcome response information. Indicates if this is the final outcome.
        /// </param>
        private void HandleOutcomeResponseReadyEvent(object sender, OutcomeResponseReadyEventArgs outcomeResponseReadyEventArgs)
        {
            if(GameCycleState == GameCycleState.EvaluatePending)
            {
                GameCycleState = outcomeResponseReadyEventArgs.IsLastOutcome
                                     ? GameCycleState.MainPlayComplete
                                     : GameCycleState.Playing;
            }

            // In the case it was none of these states, then the game is likely recovering from a power hit event.
            // In this case the state requested from the foundation could be either the pre or post transition state.
        }

        private GameCycleState AdjustOutcomeForState(IOutcomeList outcomeList,
                                                     GameCycleState currentState,
                                                     bool isLastOutcome,
                                                     IList<WagerCategoryOutcome> wagerCatOutcomeList)
        {
            // Caller is responsible for validating arguments.

            GameCycleState result;

            switch(currentState)
            {
                case GameCycleState.Playing:
                {
                    gameCyclePlayCategory.Instance.EvalOutcomeRequest(new F2XOutcomeList(outcomeList),
                                                                      isLastOutcome,
                                                                      isLastOutcome
                                                                          ? new F2XWagerCatOutcomes { WagerCatOutcome = wagerCatOutcomeList.Select(item => item.ToInternal()).ToList() }
                                                                          : null);
                    result = GameCycleState.EvaluatePending;
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Outcome list is not allowed to be adjusted in the current state [{currentState}].");
                }
            }

            return result;
        }

        #endregion
    }
}