//-----------------------------------------------------------------------
// <copyright file = "IStateMachineFramework.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication;
    using Communication.CommunicationLib;
    using Communication.GL2PInterceptorLib;
    using Communication.Logic.CommServices;
    using RandomNumbers;
    using Services;

    /// <summary>
    /// Interface which the state machine framework passes to
    /// state stage callbacks so that they may use framework
    /// functionality.
    /// </summary>
    public interface IStateMachineFramework
    {
        #region Properties

        /// <summary>
        /// Gets the mode the game runs in, such as whether it is running
        /// with a Foundation, or whether it is fast play etc..
        /// </summary>
        RunMode RunMode { get; }

        /// <summary>
        /// The GameLib used by this state machine framework.
        /// </summary>
        IGameLib GameLib
        {
            get;
        }

        /// <summary>
        /// The presentation associated with this framework.
        /// </summary>
        IPresentation Presentation
        {
            get;
        }

        /// <summary>
        /// The game service controller associated with this framework.
        /// </summary>
        /// <remarks>
        /// Adding/updating/removing providers in the game service controller by multiple threads is not guaranteed
        /// thread-safe; updating the services by multiple threads is not thread-safe either.
        /// It's the caller's obligation to implement the synchronization in multi-thread situations.
        /// </remarks>
        ServiceController GameServiceController
        {
            get;
        }

        /// <summary>
        /// The name of the state which is currently being executed.
        /// </summary>
        /// <remarks>
        /// As a service this property is useful for debugging. It should not be used for flow control within the
        /// presentation.
        /// </remarks>
        string CurrentState
        {
            get;
        }

        /// <summary>
        /// The Game Logic Automation Service associated with this framework.
        /// </summary>
        IGameLogicAutomationService GameLogicAutomationService
        { 
            get;
        }

        /// <summary>
        /// A factory used to create random number proxies. The game logic should make all of its random number 
        /// requests through one of these proxies.
        /// </summary>
        IRandomNumberProxyFactory RandomNumberProxyFactory { get; }

        #endregion

        /// <summary>
        /// The setNextState functions allows the game to specify the next
        /// state which will be executed. The function must be called during
        /// the execution of either the processing or committed callback.
        /// When the committed callback is completed the state specified
        /// by setNextState will become the current state, and that state will
        /// begin to execute. If setNextState is called multiple times, then the
        /// value of the last call will become the next state.
        /// </summary>
        /// <param name="nextState">
        /// The name of the state to execute after the current state completes.
        /// </param>
        /// <exception cref="InvalidStateException">
        /// If the specified state is not configured, then this exception will
        /// be thrown.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the nextState parameter is null, then this exception will be thrown.
        /// </exception>
        void SetNextState(string nextState);

        /// <summary>
        /// Process both the transactional and non transactional events from the Foundation. In contrast to
        /// <see cref="ProcessEvents"/>, this function returns as soon as <paramref name="processedCheck"/> is
        /// verified true without blocking for the arrival of a transactional event.
        /// </summary>
        /// <param name="processedCheck">
        /// The callback to invoke after any event is arrived to determine if the function should return.
        /// This function will be invoked without a transaction, so please avoid calling transactional APIs in it.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processedCheck"/> is null.</exception>
        /// <exception cref="InvalidStateStageException">
        /// Thrown if this function is called during a processing stage.
        /// </exception>
        /// <remarks>
        /// Please be cautioned to write the payload of the non-transactional events to the safe storage,
        /// because the transaction may roll back and re-execute due to corruptions like power-hit,
        /// but the non-transactional events won't. We recommend you achieving the recovery based on a
        /// "power-hit tolerant" message flow design. An example would be that the game always asks the
        /// Foundation for the latest non-transactional information once it starts up.
        /// </remarks>
        void WaitForNonTransactionalEvents(Func<bool> processedCheck);

        /// <summary>
        /// Process events from the foundation. Block until the passed function returns true.
        /// </summary>
        /// <param name="processedCheck">
        /// Function to execute after receiving an event to determine if the function should return.
        /// This function will be invoked within a separate game transaction.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if the processCheck argument is null.</exception>
        /// <exception cref="InvalidStateStageException">
        /// Thrown if this function is called during a processing stage.
        /// </exception>
        void ProcessEvents(Func<bool> processedCheck);

        /// <summary>
        /// Processes events and waits for a piece of critical data in the game cycle scope to become available.
        /// </summary>
        /// <typeparam name="TData">The data type of the data to read from critical data.</typeparam>
        /// <param name="criticalDataPath">The critical data path to read.</param>
        /// <returns>The object read from critical data.</returns>
        TData WaitForCriticalData<TData>(string criticalDataPath) where TData : class;

        /// <summary>
        /// Attempt to get the specified event(s) from the presentation.
        /// </summary>
        /// <param name="firstType">
        /// First in a list of events to get. This is a separate parameter to
        /// enforce that at least 1 parameter is passed to the function.
        /// </param>
        /// <param name="otherTypes">
        /// The rest of the events to get. In the case that only a single event
        /// is being waited for, additional parameters may be omitted.
        /// </param>
        /// <returns>The received event.</returns>
        /// <exception cref="InvalidStateStageException">
        /// This function may only be called during the committed stage of a state.
        /// If it is called during processing, then this exception will be thrown. 
        /// </exception>
        /// <exception cref="StateMachineForcedExitException">
        /// This exception is thrown when execution of the state machine is stopped
        /// while processing events. The state machine should normally be exited during an
        /// idle state designed to handle state machine termination.
        /// </exception>
        /// <example><code>
        /// <![CDATA[
        /// public void PlayStateCommitted(IStateMachineFramework framework)
        /// {
        ///     GameLogicGenericMsg message = framework.GetPresentationEvent(typeof (GameLogicNegotiateDataMsg),
        ///                                                                  typeof (GameLogicPresentationStateCompleteMsg));
        ///     if(message.GetType() == typeof(GameLogicNegotiateMsg))
        ///     {
        ///         var negotiateMessage = message as GameLogicNegotiateDataMsg;
        ///         var filledRequest = framework.GameServiceController.FillRequest(negotiateMessage.RequiredData);
        ///         framework.Presentation.StartState(PlayStateName, filledRequest);
        ///     }
        /// }
        /// ]]>
        /// </code></example>
        GameLogicGenericMsg GetPresentationEvent(Type firstType, params Type[] otherTypes);

        /// <summary>
        /// Attempt to get the specified event(s) from the presentation with a timeout.
        /// </summary>
        /// <param name="timeout">
        /// The timeout (in ms) to wait for an event. A negative timeout indicates to wait forever.
        /// </param>
        /// <param name="firstType">
        /// First in a list of events to get. This is a separate parameter to
        /// enforce that at least 1 parameter is passed to the function.
        /// </param>
        /// <param name="otherTypes">
        /// The rest of the events to get. In the case that only a single event
        /// is being waited for, additional parameters may be omitted.
        /// </param>
        /// <returns>
        /// One of the requested events, or null if the timeout elapsed without receiving a requested event.
        /// </returns>
        /// <remarks>
        /// Elapsed time may not match the timeout exactly. The elapsed time will be at least 5ms and in intervals of
        /// approximately 5 ms. Game Lib events will be processed while waiting for a presentation event.
        /// This method allows for a state to react to both presentation and game lib events.
        /// </remarks>
        /// <example><code>
        /// <![CDATA[
        /// public void PlayStateCommitted(IStateMachineFramework framework)
        /// {
        ///     
        ///     GameLogicGenericMsg message null;
        ///     while (message == null)
        ///     {
        ///         message= framework.GetPresentationEvent(100, typeof (GameLogicNegotiateDataMsg),
        ///                                                      typeof (GameLogicPresentationStateCompleteMsg));
        /// 
        ///         if(message != null message.GetType() == typeof(GameLogicNegotiateMsg))
        ///         {
        ///             var negotiateMessage = message as GameLogicNegotiateDataMsg;
        ///             var filledRequest = framework.GameServiceController.FillRequest(negotiateMessage.RequiredData);
        ///             framework.Presentation.StartState(PlayStateName, filledRequest);
        ///         }
        ///         if (cashoutReceived)
        ///         {
        ///             //The cashoutReceived variable could be updated by a game lib event handler which
        ///             //executed during the GetPresentationEvent call.
        ///         {
        ///     }
        /// }
        /// ]]>
        /// </code></example>
        GameLogicGenericMsg GetPresentationEvent(int timeout, Type firstType, params Type[] otherTypes);

        /// <summary>
        /// Attempt to get an event of the specified type from the presentation.
        /// </summary>
        /// <typeparam name="TGameLogicGenericMsg">The type of presentation event to get.</typeparam>
        /// <returns>An instance of the specified event.</returns>
        /// <example><code>
        /// <![CDATA[
        /// public void PlayStateCommitted(IStateMachineFramework framework)
        /// {
        ///     framework.Presentation.RequestDataNegotiation("IdleState");
        ///     var negotiateDataMsg = framework.GetPresentationEvent<GameLogicNegotiateDataMsg>();
        ///     var filledRequest = framework.GameServiceController.FillRequest(negotiateDataMsg.RequiredData);
        ///     framework.Presentation.StartState(PlayStateName, filledRequest);
        /// }
        /// ]]>
        /// </code></example>
        TGameLogicGenericMsg GetPresentationEvent<TGameLogicGenericMsg>()
            where TGameLogicGenericMsg : GameLogicGenericMsg;

        /// <summary>
        /// Indicate to the framework that the state machine is making an asynchronous update. This is used to record
        /// history if the state machine supports asynchronous history snapshots. Only transactional updates can
        /// be stored in history.
        /// </summary>
        /// <param name="data">The data being sent to the presentation.</param>
        /// <param name="transactional">Flag which indicates if the update is transactional.</param>
        void NotifyUpdateAsyncData(DataItems data, bool transactional);

        /// <summary>
        /// If a transaction is open, close it, execute the function then open a new transaction.
        /// If a transaction is not open just execute the function.
        /// </summary>
        /// <param name="function">Function to execute.</param>
        /// <remarks>
        /// Care should be taken when using this function.  It can introduce power-hit issues, such as multiple
        /// increments or changing data twice .
        /// </remarks>
        /// <exception cref="InvalidStateStageException">
        /// Thrown when function is invoked from the processing stage of the state.
        /// </exception>
        void InterruptTransactionAndInvoke(Action function);
    }
}
