// -----------------------------------------------------------------------
// <copyright file = "Gl2PMessageQueue.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Tracing;

    /// <summary>
    /// This class represents a FIFO queue of the GL2P messages.
    /// </summary>
    /// <typeparam name="T">Type of the Gl2P message that this queue stores.</typeparam>
    /// <remarks>The methods in this class are thread safe.</remarks>
    internal class Gl2PMessageQueue<T>
    {
        #region Private Fields

        /// <summary>
        /// Get the GL2P message queue.
        /// </summary>
        private readonly Queue<MessagePayload> queue = new Queue<MessagePayload>();

        /// <summary>
        /// Event signaled when a message is pending in the game logic message queue.
        /// </summary>
        private readonly ManualResetEvent messageReceivedSignal = new ManualResetEvent(false);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the signal object of the message queue.
        /// </summary>
        public WaitHandle MessageReceivedSignal => messageReceivedSignal;

        /// <summary>
        /// Gets if there is any message pending.
        /// </summary>
        public bool IsMessagePending =>
                // Since Queue<T>.Count won't have an intermediate state, the property getter is atomic. No lock is needed here.
                queue.Count > 0;

        #endregion

        #region Public Methods

        /// <summary>
        /// Enqueue a message to the end of the queue with the related info.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="info">The info of the message to enqueue.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public void EnqueueMessage(T message, MessageInfo info)
        {
            if(message == null)
            {
                throw new ArgumentNullException("message");
            }

            if(info == null)
            {
                throw new ArgumentNullException("info");
            }

            lock(queue)
            {
                Gl2PTracing.Log.Gl2PMessageEnqueued(info.Identifier);
                queue.Enqueue(new MessagePayload(message, info));
                messageReceivedSignal.Set();
            }
        }

        /// <summary>
        /// Dequeue a message from the beginning of the queue.
        /// This method returns immediately if the queue is occupied by another thread.
        /// </summary>
        /// <returns>
        /// The message from the beginning of the queue;
        /// returns null if the queue is empty, or the queue is occupied by another thread.
        /// </returns>
        public T TryDequeueMessage()
        {
            return DequeueMessage(false);
        }

        /// <summary>
        /// Dequeue a message from the beginning of the queue. This method is blocked if the queue is occupied by another thread.
        /// </summary>
        /// <returns>
        /// The message from the beginning of the queue; returns null if the queue is empty.
        /// </returns>
        public T DequeueMessage()
        {
            return DequeueMessage(true);
        }

        /// <summary>
        /// Peek the message from the beginning of the queue.
        /// This method returns immediately if the queue is occupied by another thread.
        /// </summary>
        /// <returns>
        /// The message from the beginning of the queue;
        /// returns null if the queue is empty, or the queue is occupied by another thread.
        /// </returns>
        public T TryPeekMessage()
        {
            return PeekMessage(false);
        }

        /// <summary>
        /// Peek the message from the beginning of the queue.
        /// This method is blocked if the queue is occupied by another thread.
        /// </summary>
        /// <returns>
        /// The message from the beginning of the queue; returns null if the queue is empty.
        /// </returns>
        public T PeekMessage()
        {
            return PeekMessage(true);
        }

        /// <summary>
        /// Clear the messages in queue.
        /// </summary>
        public void ClearMessages()
        {
            lock(queue)
            {
                queue.Clear();
                messageReceivedSignal.Reset();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Dequeue a message from the beginning of the queue.
        /// </summary>
        /// <param name="blocking">
        /// True to block the code if the queue cannot be acquired;
        /// otherwise, returns immediately if the queue cannot be acquired.
        /// </param>
        /// <returns>
        /// The message from the beginning of the queue; returns null if the queue is empty, or the queue cannot be acquired but 
        /// <paramref name="blocking"/> is true.
        /// </returns>
        private T DequeueMessage(bool blocking)
        {
            var result = default(T);
            if(blocking)
            {
                Monitor.Enter(queue);
            }

            if(blocking || Monitor.TryEnter(queue))
            {
                MessagePayload? messagePayload = null;
                try
                {
                    if(queue.Any())
                    {
                        // The lock surrounding should prevent a race condition.
                        // -------------------------------------------------------------------------------------
                        // Using a manual reset event here and resetting the event (which will block the thread
                        // which is waiting on this event) only when it is the final message in the queue, can
                        // prevent delayed GL2P message processing caused by state machine's blocking
                        // wait for Foundation events.
                        if(queue.Count == 1)
                        {
                            messageReceivedSignal.Reset();
                        }

                        messagePayload = queue.Dequeue();
                    }
                }
                finally
                {
                    Monitor.Exit(queue);
                }

                if(messagePayload.HasValue)
                {
                    // ReSharper disable once RedundantAssignment
                    var info = messagePayload.Value.Info;

                    Gl2PTracing.Log.Gl2PMessageDequeued(info.Identifier, (int)info.DataSize);

                    result = (T)messagePayload.Value.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Peek the message from the beginning of the queue.
        /// </summary>
        /// <param name="blocking">
        /// True to block the code if the queue cannot be acquired;
        ///  otherwise, returns immediately if the queue cannot be acquired.
        /// </param>
        /// <returns>
        /// The message from the beginning of the queue; returns null if the queue is empty, or the queue cannot be acquired but 
        /// <paramref name="blocking"/> is true.
        /// </returns>
        private T PeekMessage(bool blocking)
        {
            if(blocking)
            {
                Monitor.Enter(queue);
            }

            if(blocking || Monitor.TryEnter(queue))
            {
                try
                {
                    if(queue.Any())
                    {
                        var message = queue.Peek();
                        return (T)message.Message;
                    }
                }
                finally
                {
                    Monitor.Exit(queue);
                }
            }
            return default;
        }

        #endregion
    }
}