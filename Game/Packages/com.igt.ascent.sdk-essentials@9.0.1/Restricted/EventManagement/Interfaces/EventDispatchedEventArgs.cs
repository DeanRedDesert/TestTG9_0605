//-----------------------------------------------------------------------
// <copyright file = "EventDispatchedEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class defines the event arguments indicating that
    /// an event has been dispatched for handling.
    /// </summary>
    [Serializable]
    public class EventDispatchedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the flag indicating whether the dispatched event has been handled.
        /// This can be used by subsequent handlers to decide whether to handle the event or not.
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Gets the type of the dispatched event.
        /// </summary>
        public Type DispatchedEventType { get; private set; }

        /// <summary>
        /// Gets the dispatched event.
        /// </summary>
        public EventArgs DispatchedEvent { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="EventDispatchedEventArgs"/>.
        /// </summary>
        /// <param name="dispatchedEvent">The event to be dispatched.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dispatchedEvent"/> is null.
        /// </exception>
        public EventDispatchedEventArgs(EventArgs dispatchedEvent)
        {
            if(dispatchedEvent == null)
            {
                throw new ArgumentNullException(nameof(dispatchedEvent));
            }

            IsHandled = false;
            DispatchedEventType = dispatchedEvent.GetType();
            DispatchedEvent = dispatchedEvent;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("EventDispatchedEvent -");
            builder.AppendLine("\t IsHandled = " + IsHandled);
            builder.AppendLine("\t Dispatched Event Type = " + DispatchedEventType);

            builder.AppendLine();

            return builder.ToString();
        }

        /// <summary>
        /// Raise the dispatched event using the specified event handler.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of the event to raise.</typeparam>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="eventHandler">The event handler used for raising the event.</param>
        public void RaiseWith<TEventArgs>(object sender, EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs
        {
            if(DispatchedEvent is TEventArgs eventArgs && eventHandler != null)
            {
                eventHandler(sender, eventArgs);
                IsHandled = true;
            }
        }
    }
}
