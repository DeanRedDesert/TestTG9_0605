// -----------------------------------------------------------------------
// <copyright file = "StandardAppLibBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Standard
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This class implements functionalities, mainly event queue implementations, that are
    /// common for many application lib classes (e.g. ShellLib and CoplayerLib etc.),
    /// be it the standard implementation.
    /// </summary>
    public abstract class StandardAppLibBase : ILayeredContextActivationEventsDependency, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The flag indicating if this instance has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Protected Fields

        /// <summary>
        /// The transactional event queue.
        /// </summary>
        protected readonly TransactionalEventQueue TransEventQueue;

        /// <summary>
        /// The non-transactional event queue.
        /// </summary>
        protected readonly NonTransactionalEventQueue NonTransEventQueue;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        protected readonly Dictionary<Type, EventHandler> EventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Collection of critical data store caches owned by this instance.
        /// </summary>
        protected readonly CachedCriticalDataStoreCollection CachedStoreCollection = new CachedCriticalDataStoreCollection();

        /// <summary>
        /// Collection of disposable objects created by this instance.
        /// </summary>
        protected readonly DisposableCollection DisposableCollection = new DisposableCollection();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the transactional event queue.
        /// </summary>
        public IEventQueue TransactionalEventQueue => TransEventQueue;

        /// <summary>
        /// Gets the non-transactional event queue.
        /// </summary>
        public IEventQueue NonTransactionalEventQueue => NonTransEventQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StandardAppLibBase"/>.
        /// </summary>
        protected StandardAppLibBase()
        {
            TransEventQueue = new TransactionalEventQueue();
            DisposableCollection.Add(TransEventQueue);

            TransEventQueue.EventDispatchedEvent += HandleTransEventDispatched;

            NonTransEventQueue = new NonTransactionalEventQueue();
            DisposableCollection.Add(NonTransEventQueue);

            NonTransEventQueue.EventDispatchedEvent += HandleNonTransEventDispatched;
        }

        #endregion

        #region ILayeredContextActivationEventsDependency Implementation

        // Interface extensions usually subscribe to the events in their constructors,
        // which are invoked on a pool thread rather than the main logic thread.

        private static readonly object ActivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> ActivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.ActivateLayeredContextEvent
        {
            add
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField -= value;
                }
            }
        }

        private static readonly object InactivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> InactivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.InactivateLayeredContextEvent
        {
            add
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField -= value;
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if(disposing)
                {
                    DisposableCollection.Dispose();

                    isDisposed = true;
                }

                isDisposed = true;
            }
        }

        #endregion

        #region Protected Methods

        #region Implementing Event Table

        /// <summary>
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// <param name="eventHandler">The declared event handler.</param>
        protected static void ExecuteEventHandler<TEventArgs>(object sender, EventArgs eventArgs,
                                                            EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// Raises an event on its declared event handler found in the event table.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// <returns>
        /// True if the event handler is found and the event has been handled; False otherwise.
        /// </returns>
        protected bool RaiseEvent(object sender, Type eventType, EventArgs eventArgs)
        {
            var result = false;

            if(EventTable.ContainsKey(eventType))
            {
                var handler = EventTable[eventType];
                if(handler != null)
                {
                    handler(sender, eventArgs);

                    result = true;
                }
            }

            return result;
        }

        #endregion

        #region Implementing Layered Context Activation

        /// <summary>
        /// Notifies interface extensions about the layered context activation by firing an event.
        /// </summary>
        /// <param name="contextLayer">The context layer being activated.</param>
        protected void ActivateLayeredContext(ContextLayer contextLayer)
        {
            var eventArgs = new LayeredContextActivationEventArgs(contextLayer);
            ActivateLayeredContextEventField?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Notifies interface extensions about the layered context inactivation by firing an event.
        /// </summary>
        /// <param name="contextLayer">The context layer being inactivated.</param>
        protected void InactivateLayeredContext(ContextLayer contextLayer)
        {
            var eventArgs = new LayeredContextActivationEventArgs(contextLayer);
            InactivateLayeredContextEventField?.Invoke(this, eventArgs);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles an event dispatched from the trans event queue.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleTransEventDispatched(object sender, EventDispatchedEventArgs eventArgs)
        {
            // When raise an event, the sender should be this object instead of the event queue.
            eventArgs.IsHandled = RaiseEvent(this, eventArgs.DispatchedEventType, eventArgs.DispatchedEvent);

            // After processing the event, commit all pending critical data writes.
            if(eventArgs.DispatchedEvent is PlatformEventArgs platformEvent && platformEvent.IsTransactional)
            {
                CachedStoreCollection.CommitAllPendingWrites(platformEvent.IsHeavyweight);
            }
        }

        /// <summary>
        /// Handles an event dispatched from the non trans event queue.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleNonTransEventDispatched(object sender, EventDispatchedEventArgs eventArgs)
        {
            // When raise an event, the sender should be this object instead of the event queue.
            eventArgs.IsHandled = RaiseEvent(this, eventArgs.DispatchedEventType, eventArgs.DispatchedEvent);
        }

        #endregion
    }
}