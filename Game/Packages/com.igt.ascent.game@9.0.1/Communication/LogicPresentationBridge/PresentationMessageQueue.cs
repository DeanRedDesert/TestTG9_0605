// -----------------------------------------------------------------------
// <copyright file = "PresentationMessageQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using CommunicationLib;
    using Presentation.CommServices;
    using Tracing;

    /// <summary>
    /// Message queue for presentation events.
    /// </summary>
    public sealed class PresentationMessageQueue : IPresentationServiceHost,
                                                   IPresentation,
                                                   IPresentationTransition
    {
        #region Private Fields

        /// <summary>
        /// This object is used to copy the GL2P data from Presentation to Logic.
        /// </summary>
        private readonly Gl2PDataCopier logicDataCopier = new Gl2PDataCopier();

        /// <summary>
        /// A function used to tell if a given presentation message is a message which will trigger the presentation to reset.
        /// </summary>
        private Func<PresentationTransition, bool> presentationResetTransitionFilterFunc;

        /// <summary>
        /// A count of the number of presentation reset messages in the presentation message queue.
        /// </summary>
        private int pendingPresentationResetMsgCount;

        /// <summary>
        /// The event queue.
        /// </summary>
        private readonly ConcurrentQueue<MessagePayload> messageQueue = new ConcurrentQueue<MessagePayload>();

        #endregion

        #region IPresentationServiceHost Implementation

        /// <inheritdoc />
        public PresentationGenericMsg GetNextMessage()
        {
            // With ConcurrentQueue, the operation is the same as TryGetNextMessage.
            return TryGetNextMessage();
        }

        /// <inheritdoc />
        public PresentationGenericMsg TryGetNextMessage()
        {
            PresentationGenericMsg result = null;

            var success = messageQueue.TryDequeue(out var messagePayload);
            if(success)
            {
                Gl2PTracing.Log.Gl2PMessageDequeued(messagePayload.Info.Identifier, (int)messagePayload.Info.DataSize);

                result = messagePayload.Message as PresentationGenericMsg;
                if(IsPresentationResetMsg(result))
                {
                    _ = Interlocked.Decrement(ref pendingPresentationResetMsgCount);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public bool IsMessagePending => !messageQueue.IsEmpty;

        /// <inheritdoc />
        public PresentationGenericMsg PeekNextMessage()
        {
            var success = messageQueue.TryPeek(out var messagePayload);

            return success ? messagePayload.Message as PresentationGenericMsg : null;
        }

        /// <inheritdoc />
        public void SetPresentationResetTransitionFilter(Func<PresentationTransition, bool> filterFunc)
        {
            presentationResetTransitionFilterFunc = filterFunc;
        }

        /// <inheritdoc />
        public int PendingPresentationResetMsgCount => pendingPresentationResetMsgCount;

        #endregion

        #region IPresentation Implementation
        
        /// <inheritdoc/>
        public void UpdateAsynchData(string stateName, DataItems data)
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("AsyncData", stateName, info.Identifier);
            var message = new PresentationUpDateAsynchDataMsg(stateName,
                                                              logicDataCopier.DeepCopy(data, info));
            EnqueueMessage(message, info);
        }

        /// <inheritdoc/>
        public void StartState(string stateName, DataItems stateData)
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("StartState", stateName, info.Identifier);
            var message = new PresentationStartStateMsg(stateName,
                                                        logicDataCopier.DeepCopy(stateData, info));
            EnqueueMessage(message, info);
        }

        #endregion

        #region IPresentationTransition Implementation

        /// <inheritdoc/>
        public void Park()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("Park", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.Park);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc/>
        public void Unpark()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("Unpark", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.Unpark);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc/>
        public void ExitPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitPlayContext);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc />
        public void ExitHistoryContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitHistoryContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitHistoryContext);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc />
        public void ExitUtilityContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("ExitUtilityContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.ExitUtilityContext);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc />
        public void EnterHistoryFromPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("EnterHistoryFromPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.EnterHistoryFromPlayContext);
            EnqueueMessage(message, info);
        }

        /// <inheritdoc />
        public void EnterUtilityFromPlayContext()
        {
            var info = MessageInfo.NewMessageTracking();
            Gl2PTracing.Log.Gl2PMessageConstructionStart("EnterUtilityFromPlayContext", string.Empty, info.Identifier);
            var message = new PresentationTransitionDataMsg(PresentationTransition.EnterUtilityFromPlayContext);
            EnqueueMessage(message, info);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a message to the message queue with timing information.
        /// </summary>
        /// <param name="message">
        /// Presentation message to add to the queue.
        /// </param>
        /// <param name="info">
        /// Performance information about the message being added.
        /// </param>
        private void EnqueueMessage(PresentationGenericMsg message,
                                    MessageInfo info)
        {
            Gl2PTracing.Log.Gl2PMessageEnqueued(info.Identifier);

            if(IsPresentationResetMsg(message))
            {
                _ = Interlocked.Increment(ref pendingPresentationResetMsgCount);
            }

            messageQueue.Enqueue(new MessagePayload(message, info));
        }

        /// <summary>
        /// Tells whether the given message is a presentation reset message,
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
            return message is PresentationTransitionDataMsg transitionMessage &&
                   presentationResetTransitionFilterFunc != null &&
                   presentationResetTransitionFilterFunc(transitionMessage.PresentationTransition);
        }

        #endregion
    }
}