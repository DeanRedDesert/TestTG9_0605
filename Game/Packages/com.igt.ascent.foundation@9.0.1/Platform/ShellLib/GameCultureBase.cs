// -----------------------------------------------------------------------
// <copyright file = "GameCultureBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Base class for all <see cref="IGameCulture"/> implementations.
    /// </summary>
    /// <devdoc>
    /// In the future if ExtensionLib etc. are moved into Platform namespace,
    /// we can refactor the base class to be CultureRead and CultureWrite etc.
    /// which can be referenced by GameCulture/Base classes.
    /// </devdoc>
    internal abstract class GameCultureBase : IGameCulture, IContextCache<IShellContext>
    {
        #region Private Fields

        /// <summary>
        /// The object to put as the sender of all events raised by this class.
        /// </summary>
        private readonly object eventSender;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GameCultureBase"/>.
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
        protected GameCultureBase(object eventSender, IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;

            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
        }

        #endregion

        #region IGameCulture Implementation

        /// <inheritdoc/>
        public string Culture { get; protected set; }

        /// <inheritdoc/>
        public IReadOnlyList<string> AvailableCultures { get; protected set; }

        /// <inheritdoc/>
        public event EventHandler<CultureChangedEventArgs> CultureChangedEvent;

        #endregion

        #region IContextCache<in IShellContext> Implementation

        /// <inheritdoc/>
        public abstract void NewContext(IShellContext newContext);

        #endregion

        #region Private Methods

        /// <summary>
        /// Actions performed when a Foundation event needs processing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatchedEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            if(eventArgs.DispatchedEventType == typeof(CultureChangedEventArgs))
            {
                var handler = CultureChangedEvent;

                // We only care about culture changed events for Game context.
                if(handler != null &&
                   eventArgs.DispatchedEvent is CultureChangedEventArgs cultureChanged &&
                   cultureChanged.AffectedContext == CultureContext.Game)
                {
                    // Cache the new culture value.
                    Culture = cultureChanged.Culture;

                    handler(eventSender, cultureChanged);

                    eventArgs.IsHandled = true;
                }
            }
        }

        #endregion
    }
}