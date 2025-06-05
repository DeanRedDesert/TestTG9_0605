//-----------------------------------------------------------------------
// <copyright file = "CombinedClientHost.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using CommunicationLib;
    using GL2PInterceptorLib;
    using Logic.CommServices;
    using Presentation.CommServices;
    using Tilts;
    using Tracing;

    /// <summary>
    /// Combined GameLogic and Presentation interface host and client.
    /// </summary>
    public class CombinedClientHost : IGameLogic, IPresentation, IPresentationNotify, IPresentationServiceHost,
                                      IGameLogicServiceHost, IPresentationTransition, IPlayerSessionLogic
    {
        #region Constructors

        /// <summary>
        /// Constructor for CombinedClientHost
        /// </summary>
        public CombinedClientHost()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructor for CombinedClientHost
        /// </summary>
        /// <param name="gameLogicInterceptorService">GameLogic Interceptor Service.</param>
        public CombinedClientHost(IGameLogicInterceptorService gameLogicInterceptorService)
            : this(gameLogicInterceptorService, null)
        {
        }

        /// <summary>
        /// Constructor for CombinedClientHost
        /// </summary>
        /// <param name="presentationInterceptorService">Presentation Interceptor Service.</param>
        public CombinedClientHost(IPresentationInterceptorService presentationInterceptorService)
            : this(null, presentationInterceptorService)
        {
        }

        /// <summary>
        /// Constructor for CombinedClientHost
        /// </summary>
        /// <param name="gameLogicInterceptorService">GameLogic Interceptor Service.</param>
        /// <param name="presentationInterceptorService">Presentation Interceptor Service.</param>
        public CombinedClientHost(IGameLogicInterceptorService gameLogicInterceptorService,
                                  IPresentationInterceptorService presentationInterceptorService)
        {
            gameLogicMessageQueue = new Gl2PMessageQueue<GameLogicGenericMsg>();
            presentationMessageQueue = new Gl2PMessageQueue<PresentationGenericMsg>();

            // Use two instances to reduce the complexity of the code by avoiding handling race conditions.
            logicDataCopier = new Gl2PDataCopier();
            presentationDataCopier = new Gl2PDataCopier();

            this.gameLogicInterceptorService = gameLogicInterceptorService;
            this.presentationInterceptorService = presentationInterceptorService;

            // Register to Game Logic and Presentation Interceptor Services, so that they can
            // insert messages for processing.
            if(this.gameLogicInterceptorService != null)
            {
                this.gameLogicInterceptorService.MessageReceivedForProcessing += ProcessGameLogicMsg;
            }
            if(this.presentationInterceptorService != null)
            {
                this.presentationInterceptorService.MessageReceivedForProcessing += ProcessPresentationMsg;
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// This object is used to copy the GL2P data from Logic to Presentation.
        /// </summary>
        /// <remarks>
        /// We use two instances of the GL2P data copier to avoid race condition handling deep inside the code.
        /// </remarks>
        private readonly Gl2PDataCopier logicDataCopier;

        /// <summary>
        /// This object is used to copy the GL2P data from Presentation to Logic.
        /// </summary>
        /// <remarks>
        /// We use two instances of the GL2P data copier to avoid race condition handling deep inside the code.
        /// </remarks>
        private readonly Gl2PDataCopier presentationDataCopier;

        /// <summary>
        /// The presentation message queue object.
        /// </summary>
        private readonly Gl2PMessageQueue<PresentationGenericMsg> presentationMessageQueue;

        /// <summary>
        /// A function used to tell if a given presentation message is a message which will trigger the presentation to reset.
        /// </summary>
        private Func<PresentationTransition, bool> presentationResetTransitionFilterFunc;

        /// <summary>
        /// A count of the number of presentation reset messages in the presentation message queue.
        /// </summary>
        private int pendingPresentationResetMsgCount;

        /// <summary>
        /// The game logic message queue object.
        /// </summary>
        private readonly Gl2PMessageQueue<GameLogicGenericMsg> gameLogicMessageQueue;

        #endregion

        #region IGameLogic Members

        /// <inheritdoc />
        public void PresentationStateComplete(string stateName, string actionRequest,
                                              Dictionary<string, object> genericData = null)
        {
            if(!gameLogicHostRunning)
            {
                return;
            }

            if(gameLogicInterceptorService != null)
            {
                gameLogicInterceptorService.PresentationStateComplete(stateName, actionRequest, genericData);
            }

            if(gameLogicInterceptorService == null || gameLogicInterceptorService.IsPassiveInterceptor)
            {
                var info = MessageInfo.NewMessageTracking();
                Gl2PTracing.Log.Gl2PMessageConstructionStart("PresentationComplete", stateName, info.Identifier);

                // When the dictionary parameter is null, we create a new empty one for the message.
                var genericDataCopy = genericData != null && genericData.Count > 0
                    ? presentationDataCopier.DeepCopy(genericData, info)
                    : new Dictionary<string, object>();
                var message = new GameLogicPresentationStateCompleteMsg(
                    stateName,
                    actionRequest,
                    genericDataCopy);
                gameLogicMessageQueue.EnqueueMessage(message, info);
            }
        }

        /// <inheritdoc/>
        public void PostPresentationTilt(string tiltKey, ITilt presentationTilt,
                                         object[] titleFormatArgs, object[] messageFormatArgs)
        {
            if(!gameLogicHostRunning)
            {
                return;
            }

            if(gameLogicInterceptorService != null)
            {
                gameLogicInterceptorService.PostPresentationTilt(tiltKey, presentationTilt,
                                                                 titleFormatArgs, messageFormatArgs);
            }

            if(gameLogicInterceptorService == null || gameLogicInterceptorService.IsPassiveInterceptor)
            {
                var info = MessageInfo.NewMessageTracking();
                Gl2PTracing.Log.Gl2PMessageConstructionStart("PostPresentationTilt", string.Empty, info.Identifier);

                var message = new GameLogicPostPresentationTiltMsg(
                    tiltKey,
                    presentationDataCopier.DeepCopy(presentationTilt, info),
                    presentationDataCopier.DeepCopy(titleFormatArgs, info),
                    presentationDataCopier.DeepCopy(messageFormatArgs, info));
                gameLogicMessageQueue.EnqueueMessage(message, info);
            }
        }

        /// <inheritdoc/>
        public void ClearPresentationTilt(string tiltKey)
        {
            if(!gameLogicHostRunning)
            {
                return;
            }

            if(gameLogicInterceptorService != null)
            {
                gameLogicInterceptorService.ClearPresentationTilt(tiltKey);
            }

            if(gameLogicInterceptorService == null || gameLogicInterceptorService.IsPassiveInterceptor)
            {
                var info = MessageInfo.NewMessageTracking();
                Gl2PTracing.Log.Gl2PMessageConstructionStart("ClearPresentationTilt", string.Empty, info.Identifier);
                var message = new GameLogicClearPresentationTiltMsg(tiltKey);
                gameLogicMessageQueue.EnqueueMessage(message, info);
            }
        }

        #endregion

        #region IPlayerSessionLogic Members

        /// <inheritdoc />
        public void RequestPlayerSessionParametersResetAction(PlayerSessionParametersResetActionType actionType,
                                                              object actionData)
        {
            if(!gameLogicHostRunning)
            {
                return;
            }

            if(gameLogicInterceptorService == null || gameLogicInterceptorService.IsPassiveInterceptor)
            {
                var info = MessageInfo.NewMessageTracking();
                Gl2PTracing.Log.Gl2PMessageConstructionStart("RequestPlayerSessionParametersReset", string.Empty, info.Identifier);
                var message = new GameLogicPlayerSessionParametersResetMsg(actionType, actionData);
                gameLogicMessageQueue.EnqueueMessage(message, info);
            }
        }

        #endregion

        #region IPresentation Members

        /// <inheritdoc />
        public void UpdateAsynchData(string stateName, DataItems data)
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("AsyncData", stateName, info.Identifier);

            if(presentationInterceptorService != null)
            {
                presentationInterceptorService.UpdateAsynchData(stateName, data);
            }

            if(presentationInterceptorService == null || presentationInterceptorService.IsPassiveInterceptor)
            {
                var message = new PresentationUpDateAsynchDataMsg(
                    stateName,
                    logicDataCopier.DeepCopy(data, info));
                EnqueuePresentationMessage(message, info);
            }

            var args = new PresentationNotificationEventArgs(stateName, data);
            UpdateAsynchDataSent?.Invoke(this, args);
        }

        /// <inheritdoc />
        public void StartState(string stateName, DataItems stateData)
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("StartState", stateName, info.Identifier);
            if(presentationInterceptorService != null)
            {
                presentationInterceptorService.StartState(stateName, stateData);
            }

            if(presentationInterceptorService == null || presentationInterceptorService.IsPassiveInterceptor)
            {
                var message = new PresentationStartStateMsg(
                    stateName,
                    logicDataCopier.DeepCopy(stateData, info));
                EnqueuePresentationMessage(message, info);
            }

            StartStateSent?.Invoke(this, new PresentationNotificationEventArgs(stateName, stateData));
        }

        #endregion

        #region IPresentationTransition Members

        /// <inheritdoc />
        public void Park()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("Park", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.Park);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void Unpark()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("Unpark", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.Unpark);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void ExitPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitPlayContext);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void ExitHistoryContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitHistoryContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitHistoryContext);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void ExitUtilityContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitUtilityContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitUtilityContext);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void EnterHistoryFromPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("EnterHistoryFromPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.EnterHistoryFromPlayContext);
            EnqueuePresentationMessage(message, info);
        }

        /// <inheritdoc />
        public void EnterUtilityFromPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("EnterUtilityFromPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.EnterUtilityFromPlayContext);
            EnqueuePresentationMessage(message, info);
        }

        #endregion

        #region IPresentationServiceHost Members

        /// <inheritdoc />
        PresentationGenericMsg IPresentationServiceHost.GetNextMessage()
        {
            var msg = presentationMessageQueue.DequeueMessage();

            if(IsPresentationResetMsg(msg))
            {
                Interlocked.Decrement(ref pendingPresentationResetMsgCount);
            }

            return msg;
        }

        /// <inheritdoc />
        PresentationGenericMsg IPresentationServiceHost.TryGetNextMessage()
        {
            var msg = presentationMessageQueue.TryDequeueMessage();

            if(IsPresentationResetMsg(msg))
            {
                Interlocked.Decrement(ref pendingPresentationResetMsgCount);
            }

            return msg;
        }

        /// <inheritdoc />
        bool IPresentationServiceHost.IsMessagePending => presentationMessageQueue.IsMessagePending;

        /// <inheritdoc />
        PresentationGenericMsg IPresentationServiceHost.PeekNextMessage()
        {
            //Since this method is usually called for each frame update,
            //do not wait for acquiring the lock to avoid performance delays.
            return presentationMessageQueue.TryPeekMessage();
        }

        /// <inheritdoc />
        void IPresentationServiceHost.SetPresentationResetTransitionFilter(Func<PresentationTransition, bool> filterFunc)
        {
            presentationResetTransitionFilterFunc = filterFunc;
        }

        /// <inheritdoc />
        int IPresentationServiceHost.PendingPresentationResetMsgCount => pendingPresentationResetMsgCount;

        #endregion

        #region IGameLogicServiceHost Members

        /// <inheritdoc/>
        public WaitHandle MessageReceivedHandle => gameLogicMessageQueue.MessageReceivedSignal;

        /// <summary>
        /// A flag which indicates whether the <see cref="IGameLogicServiceHost"/> is running, and therefore
        /// accepting new messages.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="IGameLogicServiceHost.Start"/> and <see cref="IGameLogicServiceHost.Stop"/> methods
        /// to control this flag.
        /// </remarks>
        private volatile bool gameLogicHostRunning = true;

        /// <inheritdoc />
        GameLogicGenericMsg IGameLogicServiceHost.GetNextMessage()
        {
            return gameLogicMessageQueue.DequeueMessage();
        }

        /// <inheritdoc />
        bool IGameLogicServiceHost.IsMessagePending => gameLogicMessageQueue.IsMessagePending;

        /// <inheritdoc />
        GameLogicGenericMsg IGameLogicServiceHost.PeekNextMessage()
        {
            return gameLogicMessageQueue.PeekMessage();
        }

        /// <inheritdoc/>
        void IGameLogicServiceHost.Start()
        {
            if(gameLogicHostRunning)
            {
                return;
            }

            gameLogicMessageQueue.ClearMessages();
            gameLogicHostRunning = true;
        }

        /// <inheritdoc/>
        void IGameLogicServiceHost.Stop()
        {
            gameLogicHostRunning = false;
        }

        #endregion

        #region IPresentationNotify Members

        /// <inheritdoc />
        public event EventHandler<PresentationNotificationEventArgs> StartStateSent;

        /// <inheritdoc />
        public event EventHandler<PresentationNotificationEventArgs> UpdateAsynchDataSent;

        #endregion

        #region Interceptor Services

        /// <summary>
        /// Game Logic Interceptor Service.
        /// </summary>
        private readonly IGameLogicInterceptorService gameLogicInterceptorService;

        /// <summary>
        /// Queues the Game Logic Message contained in the eventArgs for processing.
        /// </summary>
        /// <param name="sender">Origin of the event.</param>
        /// <param name="eventArgs">Contains the Game Logic Message to be processed.</param>
        private void ProcessGameLogicMsg(object sender, GameLogicMessageEventArgs eventArgs)
        {
            // This message is received from a socket stream, thus, we don't need to further isolate the
            // data by a deep copy or serialization again.
            gameLogicMessageQueue.EnqueueMessage(eventArgs.Message, MessageInfo.NewEmptyInfo());
        }

        /// <summary>
        /// Presentation Interceptor Service.
        /// </summary>
        private readonly IPresentationInterceptorService presentationInterceptorService;

        /// <summary>
        /// Queues the Presentation Message contained in the eventArgs for processing.
        /// </summary>
        /// <param name="sender">Origin of the event.</param>
        /// <param name="eventArgs">Contains the Presentation Message to be processed.</param>
        private void ProcessPresentationMsg(object sender, PresentationMessageEventArgs eventArgs)
        {
            // This message is received from a socket stream, thus, we don't need to further isolate the
            // data by a deep copy or serialization again.
            EnqueuePresentationMessage(eventArgs.Message, MessageInfo.NewEmptyInfo());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add a presentation message to the presentation message queue.
        /// Update pending presentation reset message count.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="info">The info of the message to enqueue.</param>
        private void EnqueuePresentationMessage(PresentationGenericMsg message, MessageInfo info)
        {
            if(IsPresentationResetMsg(message))
            {
                Interlocked.Increment(ref pendingPresentationResetMsgCount);
            }
            presentationMessageQueue.EnqueueMessage(message, info);
        }

        /// <summary>
        /// Tell whether the given message is a presentation reset message,
        /// using the presentation reset message filter function.
        /// </summary>
        /// <param name="message">The message to inspect.</param>
        /// <returns>
        /// Returns true if the message is a presentation reset message.
        /// Returns false if the message is null, if the presentation reset message filter
        /// function is null, or if the message is not a presentation reset message.
        /// </returns>
        private bool IsPresentationResetMsg(PresentationGenericMsg message)
        {
            var transitionMessage = message as PresentationTransitionDataMsg;
            return transitionMessage != null &&
                   presentationResetTransitionFilterFunc != null &&
                   presentationResetTransitionFilterFunc(transitionMessage.PresentationTransition);
        }

        #endregion
    }
}
