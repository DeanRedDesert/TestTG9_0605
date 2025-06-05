// -----------------------------------------------------------------------
// <copyright file = "GamePlayStatusBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Base class for <see cref="IGamePlayStatus"/> implementations.
    /// </summary>
    internal abstract class GamePlayStatusBase : IGamePlayStatus,
        IContextCache<IShellContext>
    {
        #region Private Fields

        /// <summary>
        /// The object to put as the sender of all events raised by this class.
        /// </summary>
        private readonly object eventSender;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        #endregion

        #region IGamePlayStatus Implementation

        /// <inheritdoc/>
        public event EventHandler<GameInProgressChangedEventArgs> GameInProgressChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<GameFocusChangedEventArgs> GameFocusChangedEvent;

        /// <inheritdoc/>
        public virtual bool GameInProgress { get; protected set; }

        /// <inheritdoc/>
        public virtual GameFocus GameFocus { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GamePlayStatusBase"/>.
        /// </summary>
        /// <param name="eventSender">
        /// The object to put as the sender of all events raised by this class.
        /// If null, this instance will be put as the sender.
        /// 
        /// This is so that the event handlers can cast sender to
        /// IShellLib if needed, e.g. writing critical data.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments except <paramref name="eventSender"/> is null.
        /// </exception>
        protected GamePlayStatusBase(object eventSender, IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;

            CreateEventLookupTable();
            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
        }

        #endregion

        #region IContextCache Implementation

        /// <inheritdoc/>
        public abstract void NewContext(IShellContext shellContext);

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(GameInProgressChangedEventArgs)] =
                (sender, eventArgs) => ExecuteGamePlayInProgressChanged(sender, eventArgs as GameInProgressChangedEventArgs);

            eventTable[typeof(GameFocusChangedEventArgs)] =
                (sender, eventArgs) => ExecuteGameFocusChanged(sender, eventArgs as GameFocusChangedEventArgs);
        }

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
        /// Called when the game progress is updated.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void ExecuteGamePlayInProgressChanged(object sender,
                                                      GameInProgressChangedEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                GameInProgress = eventArgs.GameInProgress;
            }
            GameInProgressChangedEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Called when the game focus changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void ExecuteGameFocusChanged(object sender,
                                             GameFocusChangedEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                GameFocus = eventArgs.NewGameFocus;
            }
            GameFocusChangedEvent?.Invoke(sender, eventArgs);
        }

        #endregion
    }
}