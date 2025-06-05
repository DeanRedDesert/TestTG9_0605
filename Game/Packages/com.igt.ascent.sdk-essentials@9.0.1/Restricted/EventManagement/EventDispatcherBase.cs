// -----------------------------------------------------------------------
// <copyright file = "EventDispatcherBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using Interfaces;
    using System;

    /// <summary>
    /// This abstract class implements <see cref="IEventDispatcher"/>,
    /// and can be used as the base class for some common event queue implementations.
    /// </summary>
    public abstract class EventDispatcherBase : IEventDispatcher, IEventDispatchMonitor
    {
        #region Implementation of IEventDispatcher

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;

        #endregion

        #region Implementation of IEventDispatchMonitor

        /// <inheritdoc />
        public event EventHandler EventDispatchStarting;

        /// <inheritdoc />
        public event EventHandler EventDispatchEnded;

        #endregion

        #region Private Methods

        /// <summary>
        /// Dispatches an event for processing.
        /// </summary>
        /// <param name="eventArgs">
        /// The event to process.
        /// </param>
        /// <param name="checkExpectation">
        /// The delegate used to check if an expectation is met.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <returns>
        /// True if <paramref name="checkExpectation"/> is specified, and the events processed met the expectation;
        /// False otherwise.
        /// </returns>
        protected bool DispatchEventForProcessing(EventArgs eventArgs, CheckEventExpectationDelegate checkExpectation)
        {
            EventDispatchStarting?.Invoke(this, eventArgs);

            var result = false;

            var handler = EventDispatchedEvent;
            if(handler != null)
            {
                handler(this, new EventDispatchedEventArgs(eventArgs));

                if(checkExpectation != null)
                {
                    result = checkExpectation(eventArgs);
                }
            }

            EventDispatchEnded?.Invoke(this, eventArgs);

            return result;
        }

        #endregion
    }
}