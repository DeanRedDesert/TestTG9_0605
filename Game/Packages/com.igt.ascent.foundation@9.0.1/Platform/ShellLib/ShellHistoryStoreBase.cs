// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryStoreBase.cs" company = "IGT">
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
    /// Base class for all <see cref="IShellHistoryStore"/> implementations.
    /// </summary>
    internal abstract class ShellHistoryStoreBase : IShellHistoryStore, IContextCache<IShellContext>
    {
        #region Constants and Fields

        private const string StoreName = "ShellHistoryStore";

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// The object to put as the sender of all events raised by this class.
        /// </summary>
        private readonly object eventSender;

        /// <summary>
        /// The interface used for validating the accessing to key-value store of critical data.
        /// </summary>
        private readonly ICriticalDataStoreAccessValidator storeAccessValidator;

        /// <summary>
        /// Back end field for <see cref="WritePermittedCoplayers"/>.
        /// </summary>
        protected  List<int> WritePermittedList;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ShellHistoryStoreBase"/>.
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
        /// <param name="storeAccessValidator">
        /// The interface used for validating the accessing to key-value store of critical data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments except <paramref name="eventSender"/> is null.
        /// </exception>
        protected ShellHistoryStoreBase(object eventSender,
                                        IEventDispatcher transactionalEventDispatcher,
                                        ICriticalDataStoreAccessValidator storeAccessValidator)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.eventSender = eventSender ?? this;
            this.storeAccessValidator = storeAccessValidator ?? throw new ArgumentNullException(nameof(storeAccessValidator));

            WritePermittedList = new List<int>();

            CreateEventLookupTable();
            transactionalEventDispatcher.EventDispatchedEvent += HandleEventDispatchedEvent;
        }

        #endregion

        #region IShellHistoryStore Implementation

        /// <inheritdoc/>
        public event EventHandler<ShellHistoryStoreWritePermittedChangedEventArgs> ShellHistoryStoreWritePermittedChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<LogEndGameCycleEventArgs> LogEndGameCycleEvent;

        /// <inheritdoc/>
        public IReadOnlyList<int> WritePermittedCoplayers => WritePermittedList;

        /// <inheritdoc/>
        public bool IsWritePermitted(int coplayerId)
        {
            return WritePermittedList.Contains(coplayerId);
        }

        /// <inheritdoc/>
        public ICriticalDataBlock Read(int coplayerId, IList<CriticalDataName> nameList)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            if(nameList == null || nameList.Count == 0)
            {
                throw new ArgumentException("The name list can not be null or empty.");
            }

            storeAccessValidator.Validate(DataAccess.Read, StoreName);

            return DoRead(coplayerId, nameList);
        }

        /// <inheritdoc/>
        public void Remove(int coplayerId, IList<CriticalDataName> nameList)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            if(nameList == null || nameList.Count == 0)
            {
                throw new ArgumentException("The name list can not be null or empty.");
            }

            storeAccessValidator.Validate(DataAccess.Remove, StoreName);

            if(!IsWritePermitted(coplayerId))
            {
                throw new InvalidOperationException(
                    $"Coplayer {coplayerId}'s corresponding shell history store is not write permitted.");
            }

            DoRemove(coplayerId, nameList);
        }

        /// <inheritdoc/>
        public void Write(int coplayerId, ICriticalDataBlock criticalDataBlock)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            if(criticalDataBlock == null)
            {
                throw new ArgumentNullException(nameof(criticalDataBlock));
            }

            var nameList = criticalDataBlock.GetNameList();
            if(nameList == null || nameList.Count == 0)
            {
                throw new ArgumentException("Invalid critical data block: The name list can not be empty.");
            }

            storeAccessValidator.Validate(DataAccess.Write, StoreName);

            if(!IsWritePermitted(coplayerId))
            {
                throw new InvalidOperationException(
                    $"Coplayer {coplayerId}'s corresponding shell history store is not write permitted.");
            }

            DoWrite(coplayerId, criticalDataBlock);
        }

        #endregion

        #region IContextCache Implementation

        /// <inheritdoc/>
        public abstract void NewContext(IShellContext newContext);

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Executes the action of reading.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is read from.
        /// </param>
        /// <param name="nameList">
        /// List of names of the critical data to read from the store.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        protected abstract ICriticalDataBlock DoRead(int coplayerId, IList<CriticalDataName> nameList);

        /// <summary>
        /// Executes the action of removing.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is removed from.
        /// </param>
        /// <param name="nameList">
        /// List of names of the critical data to remove from the store.
        /// </param>
        protected abstract void DoRemove(int coplayerId, IList<CriticalDataName> nameList);

        /// <summary>
        /// Executes the action of writing.
        /// </summary>
        /// <param name="coplayerId">
        /// The coplayer whose corresponding shell history store is where the data is written to.
        /// </param>
        /// <param name="criticalDataBlock">
        /// A critical data block which contains all the critical data having been read.
        /// </param>
        protected abstract void DoWrite(int coplayerId, ICriticalDataBlock criticalDataBlock);

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            eventTable[typeof(ShellHistoryStoreWritePermittedChangedEventArgs)] =
                (sender, eventArgs) => ExecuteWritePermittedChangedHandler(sender, eventArgs as ShellHistoryStoreWritePermittedChangedEventArgs);

            eventTable[typeof(LogEndGameCycleEventArgs)] =
                (sender, eventArgs) => ExecuteEventHandler(sender, eventArgs, LogEndGameCycleEvent);
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
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <param name="eventHandler">The declared event handler.</param>
        private static void ExecuteEventHandler<TEventArgs>(object sender, EventArgs eventArgs,
                                                            EventHandler<TEventArgs> eventHandler)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// Updates write permitted list and raises <see cref="ShellHistoryStoreWritePermittedChangedEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void ExecuteWritePermittedChangedHandler(object sender, ShellHistoryStoreWritePermittedChangedEventArgs eventArgs)
        {
                UpdateWritePermittedList(eventArgs.CoplayerId, eventArgs.WritePermitted);

                ShellHistoryStoreWritePermittedChangedEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Updates the list tracking which coplayers are write permitted.
        /// </summary>
        /// <param name="coplayerId">
        /// The id of the coplayer.
        /// </param>
        /// <param name="writePermitted">
        /// The flag indicating if the coplayer's corresponding shell history store is write permitted.
        /// </param>
        private void UpdateWritePermittedList(int coplayerId, bool writePermitted)
        {
            if(writePermitted && !WritePermittedList.Contains(coplayerId))
            {
                WritePermittedList.Add(coplayerId);
            }
            else if(!writePermitted && WritePermittedList.Contains(coplayerId))
            {
                WritePermittedList.Remove(coplayerId);
            }
        }

        #endregion
    }
}