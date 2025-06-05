// -----------------------------------------------------------------------
// <copyright file = "BankPlayBase.cs" company = "IGT">
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
    /// Base class for all <see cref="IBankPlay"/> implementations.
    /// </summary>
    internal abstract class BankPlayBase : IBankPlay, IContextCache<IShellContext>
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

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="BankPlayBase"/>.
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
        /// <param name="nonTransactionalEventDispatcher">
        /// Interface for processing a non-transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments except <paramref name="eventSender"/> is null.
        /// </exception>
        protected BankPlayBase(object eventSender, IEventDispatcher transactionalEventDispatcher, IEventDispatcher nonTransactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }
            if(nonTransactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(nonTransactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;

            CreateEventLookupTable();
            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
            nonTransactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
        }

        #endregion

        #region IContextCache Implementation

        /// <inheritdoc/>
        public abstract void NewContext(IShellContext shellContext);

        #endregion

        #region IBankPlay Implementation

        /// <inheritdoc/>
        public event EventHandler<MoneyChangedEventArgs> MoneyChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyBetEventArgs> MoneyBetEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyBettableTransferEventArgs> MoneyBettableTransferEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyCommittedChangedEventArgs> MoneyCommittedChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyInEventArgs> MoneyInEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyOutEventArgs> MoneyOutEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneySetEventArgs> MoneySetEvent;

        /// <inheritdoc/>
        public event EventHandler<MoneyWonEventArgs> MoneyWonEvent;

        /// <inheritdoc/>
        public event EventHandler<BankPlayPropertiesUpdateEventArgs> BankPlayPropertiesUpdateEvent;

        /// <inheritdoc/>
        public virtual MachineWideBetConstraints MachineWideBetConstraints { get; protected set; }

        /// <inheritdoc/>
        public virtual bool SupportPlayerTransfersForBettable { get; protected set; }

        /// <inheritdoc/>
        public virtual BankPlayProperties BankPlayProperties { get; protected set; }

        /// <inheritdoc/>
        public virtual GamingMeters GamingMeters { get; protected set; }

        /// <inheritdoc/>
        public abstract void RequestCashout();

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(MoneyBetEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyBetEvent);
            eventTable[typeof(MoneyBettableTransferEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyBettableTransferEvent);
            eventTable[typeof(MoneyCommittedChangedEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyCommittedChangedEvent);
            eventTable[typeof(MoneyInEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyInEvent);
            eventTable[typeof(MoneyOutEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyOutEvent);
            eventTable[typeof(MoneySetEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneySetEvent);
            eventTable[typeof(MoneyWonEventArgs)] =
                (sender, eventArgs) => ExecuteMoneyChangedHandler(sender, eventArgs, MoneyWonEvent);
            eventTable[typeof(BankPlayPropertiesUpdateEventArgs)] =
                (sender, eventArgs) => ExecutePropertiesUpdateHandler(sender, eventArgs as BankPlayPropertiesUpdateEventArgs);
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
        /// This function is a generic way to invoke a money changed event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <param name="eventHandler">The handler for the money changed event.</param>
        private void ExecuteMoneyChangedHandler<TEventArgs>(object sender,
                                                            EventArgs eventArgs,
                                                            EventHandler<TEventArgs> eventHandler)
            where TEventArgs : MoneyChangedEventArgs
        {
            var moneyChangedEvent = eventArgs as MoneyChangedEventArgs;
            if(moneyChangedEvent != null)
            {
                GamingMeters = moneyChangedEvent.GamingMeters;
            }

            var moneyChangedEventHandler = MoneyChangedEvent;
            moneyChangedEventHandler?.Invoke(sender, moneyChangedEvent);

            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// Caches the new property values and raises <see cref="BankPlayPropertiesUpdateEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void ExecutePropertiesUpdateHandler(object sender,
                                                    BankPlayPropertiesUpdateEventArgs eventArgs)
        {
            if(eventArgs != null)
            {
                BankPlayProperties = UpdateProperties(BankPlayProperties, eventArgs);
            }

            BankPlayPropertiesUpdateEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Returns a new set of property values based on the current values and the update values.
        /// </summary>
        /// <param name="current">The current property values.</param>
        /// <param name="update">The property values to update.</param>
        /// <returns>The property values after update.</returns>
        private static BankPlayProperties UpdateProperties(BankPlayProperties current, BankPlayPropertiesUpdateEventArgs update)
        {
            return new BankPlayProperties(update.CanBet ?? current.CanBet,
                                          update.CanCommitGameCycle ?? current.CanCommitGameCycle,
                                          update.CashoutOfferable ?? current.CashoutOfferable,
                                          update.PlayerBettableTransferOfferable ?? current.PlayerBettableTransferOfferable);
        }

        #endregion
    }
}