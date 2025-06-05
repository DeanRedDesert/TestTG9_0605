// -----------------------------------------------------------------------
// <copyright file = "InnerLibBase.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System;
    using System.Collections.Generic;
    using F2XLinks;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Base class of all <see cref="IInnerLib"/> implementations that are backed by F2X communications.
    /// </summary>
    internal abstract class InnerLibBase : IInnerLib
    {
        #region Fields and Properties

        /// <summary>
        /// The inner link used by the inner lib.
        /// </summary>
        private readonly IInnerLink innerLink;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        protected readonly Dictionary<Type, EventHandler> EventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Gets the flag indicating whether inner lib has established communication with Foundation.
        /// </summary>
        protected bool IsConnected => innerLink.IsConnected;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="InnerLibBase"/>.
        /// </summary>
        /// <param name="innerLink">The inner link used by the inner lib.</param>
        /// <param name="transactionalEventDispatcher">The dispatcher of transactional evens.</param>
        /// <param name="nonTransactionalEventDispatcher">The dispatcher of non transactional evens.</param>
        protected InnerLibBase(IInnerLink innerLink,
                               IEventDispatcher transactionalEventDispatcher,
                               IEventDispatcher nonTransactionalEventDispatcher)
        {
            this.innerLink = innerLink;

            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatched;
            nonTransactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatched;
        }

        #endregion

        #region IInnerLib Implementation

        /// <inheritdoc />
        public bool IsContextActive { get; protected set; }

        /// <inheritdoc />
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            return IsConnected
                       ? innerLink.ApiManager.GetInterface<TExtendedInterface>()
                       : null;
        }

        #endregion

        #region Protected and Private Methods

        #region Implementing Event Table

        /// <summary>
        /// Raises an event on its declared event handler found in the event table.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// <returns>
        /// True if the event handler is found and the event has been handled; False otherwise.
        /// </returns>
        private bool RaiseEvent(object sender, Type eventType, EventArgs eventArgs)
        {
            var result = false;

            if(EventTable.TryGetValue(eventType, out var handler) &&
               handler != null)
            {
                handler(sender, eventArgs);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        /// <param name="eventHandler">The declared event handler.</param>
        protected static void ExecuteEventHandler<TEventArgs>(object sender,
                                                              EventArgs eventArgs,
                                                              EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// This function is a generic way to handle an event before raising it to external parties.
        /// The event's "pre-handler" usually replaces the original event with a new instance,
        /// which is then exposed to the subscribed event handlers.
        /// </summary>
        /// <param name="sender">
        /// The event sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        /// <param name="preHandleFunc">
        /// The function that handles <paramref name="eventArgs"/>, and replace it with a new instance if needed.
        /// </param>
        /// <param name="eventHandler">
        /// The declared event handler.
        /// </param>
        protected static void HandleBeforeExecute<TEventArgs>(object sender,
                                                              EventArgs eventArgs,
                                                              Func<EventArgs, TEventArgs> preHandleFunc,
                                                              EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            var newEventArgs = preHandleFunc(eventArgs);
            eventHandler?.Invoke(sender, newEventArgs);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles an event dispatched from the trans and non-trans event queues.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatched(object sender, EventDispatchedEventArgs eventArgs)
        {
            // When raise an event, the sender should be this object instead of the event queue.
            eventArgs.IsHandled = RaiseEvent(this, eventArgs.DispatchedEventType, eventArgs.DispatchedEvent);
        }

        #endregion

        #endregion
    }
}