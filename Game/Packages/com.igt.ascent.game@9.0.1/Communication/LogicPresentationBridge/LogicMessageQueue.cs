// -----------------------------------------------------------------------
// <copyright file = "LogicMessageQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Ascent.Restricted.EventManagement.Interfaces;
    using CommunicationLib;
    using Logic.CommServices;
    using Tilts;
    using Tracing;

    /// <summary>
    /// Event queue for game logic messages.
    /// </summary>
    public sealed class LogicMessageQueue : IEventQueue,
                                            IEventDispatcher,
                                            IGameLogic, 
                                            IPlayerSessionLogic,
                                            ILogicHostControl,
                                            IDisposable
    {
        #region Private Fields

        /// <summary>
        /// A flag which indicates whether the <see cref="IGameLogicServiceHost"/> is running, and therefore
        /// accepting new messages.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="IGameLogicServiceHost.Start"/> and <see cref="IGameLogicServiceHost.Stop"/> methods
        /// to control this flag.
        /// </remarks>
        private volatile bool logicHostRunning = true;

        /// <summary>
        /// This object is used to copy the GL2P data from Presentation to Logic.
        /// </summary>
        private readonly Gl2PDataCopier presentationDataCopier = new Gl2PDataCopier();

        /// <summary>
        /// Object to synchronize the access to the event queue.
        /// </summary>
        private readonly object queueLocker = new object();

        /// <summary>
        /// Object to synchronize when an event is added to the queue.
        /// </summary>
        private readonly ManualResetEvent eventReceived = new ManualResetEvent(false);

        /// <summary>
        /// The event queue.
        /// </summary>
        private Queue<MessagePayload> messageQueue = new Queue<MessagePayload>();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region IEventQueue Implementation

        /// <inheritdoc/>
        public WaitHandle EventReceived => eventReceived;

        /// <inheritdoc/>
        public bool ProcessEvents(CheckEventExpectationDelegate checkExpectation)
        {
            var result = false;

            Queue<MessagePayload> pendingMessages;

            lock(queueLocker)
            {
                pendingMessages = messageQueue;
                messageQueue = new Queue<MessagePayload>();
                eventReceived.Reset();
            }

            foreach(var pendingMessage in pendingMessages)
            {
                // ReSharper disable once RedundantAssignment
                var info = pendingMessage.Info;

                Gl2PTracing.Log.Gl2PMessageDequeued(info.Identifier, (int)info.DataSize);

                var gameLogicGenericMsg = pendingMessage.Message as GameLogicGenericMsg;

                EventDispatchedEvent?.Invoke(this, new EventDispatchedEventArgs(gameLogicGenericMsg));

                if(checkExpectation != null)
                {
                    result |= checkExpectation(gameLogicGenericMsg);
                }
            }
            return result;
        }

        #endregion

        #region IEventDispatcher Implementation

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;

        #endregion

        #region IGameLogic Implementation

        /// <inheritdoc/>
        public void PresentationStateComplete(string stateName, string actionRequest,
            Dictionary<string, object> genericData = null)
        {
            if(!logicHostRunning)
            {
                return;
            }

            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("PresentationComplete", stateName, info.Identifier);

            // When the dictionary parameter is null, we create a new empty one for the message.
            var genericDataCopy = genericData != null && genericData.Count > 0
                                      ? presentationDataCopier.DeepCopy(genericData, info)
                                      : new Dictionary<string, object>();

            var message = new GameLogicPresentationStateCompleteMsg(stateName, actionRequest, genericDataCopy);

            EnqueueMessage(message, info);
        }

        /// <inheritdoc/>
        public void PostPresentationTilt(string tiltKey, ITilt presentationTilt, 
            object[] titleFormatArgs, object[] messageFormatArgs)
        {
            if(!logicHostRunning)
            {
                return;
            }

            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("PostPresentationTilt", string.Empty, info.Identifier);

            var message = new GameLogicPostPresentationTiltMsg(tiltKey,
                presentationDataCopier.DeepCopy(presentationTilt, info),
                presentationDataCopier.DeepCopy(titleFormatArgs, info),
                presentationDataCopier.DeepCopy(messageFormatArgs, info));

            EnqueueMessage(message, info);
        }

        /// <inheritdoc/>
        public void ClearPresentationTilt(string tiltKey)
        {
            if(!logicHostRunning)
            {
                return;
            }

            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ClearPresentationTilt", string.Empty, info.Identifier);

            var message = new GameLogicClearPresentationTiltMsg(tiltKey);

            EnqueueMessage(message, info);
        }

        #endregion

        #region IPlayerSessionLogic Implementation

        /// <inheritdoc/>
        public void RequestPlayerSessionParametersResetAction(PlayerSessionParametersResetActionType actionType,
            object actionData)
        {
            if(!logicHostRunning)
            {
                return;
            }

            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart(
                "RequestPlayerSessionParametersReset", string.Empty, info.Identifier);

            var message = new GameLogicPlayerSessionParametersResetMsg(actionType, actionData);

            EnqueueMessage(message, info);
        }

        #endregion

        #region ILogicHostControl Implementation

        /// <inheritdoc/>
        public void Start()
        {
            if(logicHostRunning)
            {
                return;
            }

            lock(queueLocker)
            {
                messageQueue.Clear();
                eventReceived.Reset();
            }

            logicHostRunning = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            logicHostRunning = false;
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if(disposing)
                {
                    lock(queueLocker)
                    {
                        // Auto and Manual reset events are disposable
                        (eventReceived as IDisposable).Dispose();
                    }
                }
                isDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add a message to the message queue with timing information and
        /// alert waiting threads of the new message.
        /// </summary>
        /// <param name="message">Game logic message to add to the queue.</param>
        /// <param name="info">Information about the message being added.</param>
        private void EnqueueMessage(GameLogicGenericMsg message, MessageInfo info)
        {
            Gl2PTracing.Log.Gl2PMessageEnqueued(info.Identifier);

            lock(queueLocker)
            {
                messageQueue.Enqueue(new MessagePayload(message, info));
                eventReceived.Set();
            }
        }

        #endregion
    }
}