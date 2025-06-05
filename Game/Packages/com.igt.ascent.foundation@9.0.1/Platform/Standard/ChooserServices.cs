// -----------------------------------------------------------------------
// <copyright file = "ChooserServices.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Standard
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IChooserServices"/> that uses
    /// F2X to communicate with the Foundation to support chooser services.
    /// </summary>
    /// <typeparam name="TContext">Type of the context.</typeparam>
    internal sealed class ChooserServices<TContext> : IChooserServices, IContextCache<TContext> where TContext : class
    {
        #region Private Fields

        /// <summary>
        /// The interface for the chooser services category.
        /// </summary>
        private readonly CategoryInitializer<IChooserServicesCategory> chooserServicesCategory;

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// The object to put as the sender of all events raised by this class.
        /// </summary>
        private readonly object eventSender;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ChooserServices{TContext}"/>.
        /// </summary>
        /// <param name="eventSender">
        /// The object to put as the sender of all events raised by this class.
        /// If null, this instance will be put as the sender.
        /// 
        /// This is so that the event handlers can cast sender to
        /// IShellLib if needed, e.g. writing critical data.
        /// </param>
        /// <param name="nonTransactionalEventDispatcher">
        /// Interface for processing a non-transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public ChooserServices(object eventSender, IEventDispatcher nonTransactionalEventDispatcher)
        {
            if(nonTransactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(nonTransactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;

            chooserServicesCategory = new CategoryInitializer<IChooserServicesCategory>();

            CreateEventLookupTable();
            nonTransactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="ChooserServices{TContext}"/> whose values become
        /// available after construction, e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IChooserServicesCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            chooserServicesCategory.Initialize(category);
        }

        #endregion

        #region IContextCache<in TContext> Implementation

        /// <inheritdoc />
        public void NewContext(TContext newContext)
        {
            var reply = chooserServicesCategory.Instance.GetChooserServicesProperties();
            if(!reply.OfferableSpecified)
            {
                throw new MissingFieldException("Missing Chooser Services properties");
            }

            ChooserProperties = new ChooserProperties(reply.Offerable);
        }

        #endregion

        #region IChooserServices Implementation

        /// <inheritdoc/>
        public event EventHandler<ChooserPropertiesUpdateEventArgs> ChooserPropertiesUpdateEvent;
        /// <inheritdoc/>

        public void RequestChooser()
        {
            chooserServicesCategory.Instance.RequestChooser();
        }

        /// <inheritdoc/>
        public ChooserProperties ChooserProperties { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(ChooserPropertiesUpdateEventArgs)] =
                (sender, eventArgs) => ExecutePropertiesUpdateHandler(sender, eventArgs as ChooserPropertiesUpdateEventArgs);
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
        /// Caches the new property values and raises <see cref="ChooserPropertiesUpdateEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void ExecutePropertiesUpdateHandler(object sender,  ChooserPropertiesUpdateEventArgs eventArgs) 
        {
            if(eventArgs != null)
            {
                ChooserProperties = UpdateProperties(ChooserProperties, eventArgs);
            }

            ChooserPropertiesUpdateEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Returns a new set of property values based on the current values and the update values.
        /// </summary>
        /// <param name="current">The current property values.</param>
        /// <param name="update">The property values to update.</param>
        /// <returns>The property values after update.</returns>
        private static ChooserProperties UpdateProperties(ChooserProperties current, ChooserPropertiesUpdateEventArgs update)
        {
            return new ChooserProperties(update.Offerable ?? current.Offerable);
        }

        #endregion
    }
}