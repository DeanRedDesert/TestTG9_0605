// -----------------------------------------------------------------------
// <copyright file = "RunnerBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Communication.Platform;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.Logic.CommServices;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Base class for runners to implement some common functionality.
    /// The runner is in charge of running a thread, and responsible for
    /// creating and destroying the state machine framework at appropriate times.
    /// </summary>
    /// <remarks>
    /// This class does implement some interface APIs.  But as the interfaces implemented
    /// are runner specific, this base class does not declare to implement any interface.
    /// The interface declaration is done by the derived classes.
    /// </remarks>
    internal abstract class RunnerBase : IDisposable
    {
        #region Private Fields

        private readonly List<IEventQueue> customEventQueues = new List<IEventQueue>();

        #endregion

        #region Protected Fields

        // Disposable handling
        protected readonly DisposableCollection DisposableCollection = new DisposableCollection();
        protected bool IsDisposed;

        // Fields assigned by child classes.
        protected List<WaitHandle> ReturnHandles;
        protected IEventQueueManager EventQueueManager;
        protected IList<IEventQueue> TransEventQueueList;

        #endregion

        #region IFrameworkRunner Implementation

        /// <remarks/>
        public bool WaitForCondition(Func<bool, bool> conditionChecker)
        {
            return ProcessEvents(eventArgs => conditionChecker(eventArgs is PlatformEventArgs platformEventArgs &&
                                                               platformEventArgs.IsTransactional),
                                 null);
        }

        /// <remarks/>
        public bool WaitForLightweightTransactionalCondition(Func<bool> conditionChecker)
        {
            // There are events that are raised on trans event queue but not transactional,
            // such as the park event.
            return ProcessEvents(eventArgs => eventArgs is PlatformEventArgs platformEventArgs &&
                                              platformEventArgs.IsTransactional &&
                                              conditionChecker(),
                                 TransEventQueueList);
        }

        /// <remarks/>
        public bool WaitForNonTransactionalCondition(Func<bool> conditionChecker)
        {
            // Non transactional condition checker can run on all event queues, including trans event queue.
            return ProcessEvents(eventArgs => conditionChecker(),
                                 null);
        }

        /// <remarks/>
        public void RegisterEventQueue(IEventQueue eventQueue)
        {
            customEventQueues.Add(eventQueue);
            EventQueueManager.RegisterEventQueue(eventQueue);

            // In case the event queue dispatches events that trigger any async game service update.
            if(eventQueue is IEventDispatchMonitor dispatchMonitor)
            {
                dispatchMonitor.EventDispatchEnded += HandlePostProcessingEvent;
            }
        }

        /// <remarks/>
        public void UnregisterEventQueue(IEventQueue eventQueue)
        {
            customEventQueues.Remove(eventQueue);
            EventQueueManager.UnregisterEventQueue(eventQueue);

            if(eventQueue is IEventDispatchMonitor dispatchMonitor)
            {
                dispatchMonitor.EventDispatchEnded -= HandlePostProcessingEvent;
            }
        }

        /// <remarks/>
        public event EventHandler<GameLogicGenericMsg> PresentationEventReceived;

        /// <remarks/>
        public event EventHandler EventQueuePostProcessing;

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(IsDisposed)
            {
                return;
            }

            if(disposing)
            {
                DisposableCollection.Dispose();
            }

            IsDisposed = true;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Processes events by calling into Event Queue Manager.
        /// </summary>
        /// <param name="checkExpectation">
        /// The delegate used to check if an expectation is met.
        /// </param>
        /// <param name="expectedQueues">
        /// The event queues for which <paramref name="checkExpectation"/> is to run.
        /// </param>
        /// <returns>
        /// True of the <paramref name="checkExpectation"/> returns true;
        /// False if the waiting is interrupted by a higher priority signal.
        /// </returns>
        protected bool ProcessEvents(CheckEventExpectationDelegate checkExpectation, IList<IEventQueue> expectedQueues)
        {
            var signaledReturnHandle = EventQueueManager.ProcessEvents(ReturnHandles,
                                                                       checkExpectation,
                                                                       expectedQueues);

            return signaledReturnHandle == null;
        }

        /// <summary>
        /// Tries to leverage the next available heavyweight transactional event,
        /// including the Action Response Event, to execute an action within it.
        /// </summary>
        /// <param name="eventArgs">
        /// The event being processed.
        /// </param>
        /// <param name="action">
        /// The action to execute inside the heavyweight event handler.
        /// </param>
        /// <returns>
        /// True if <paramref name="eventArgs"/> is heavyweight and <paramref name="action"/> has run;
        /// False otherwise.
        /// </returns>
        protected static bool LeverageHeavyweightEvent(EventArgs eventArgs, Action action)
        {
            var result = false;

            if(eventArgs is PlatformEventArgs platformEventArgs && platformEventArgs.IsHeavyweight)
            {
                action();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Handles a presentation event.
        /// </summary>
        /// <param name="sender">
        /// The event sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected void HandlePresentationEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            // Forward to state machine framework for handling.
            PresentationEventReceived?.Invoke(this, eventArgs.DispatchedEvent as GameLogicGenericMsg);
        }

        /// <summary>
        /// Handles notification from an event queue that an event has been dispatched.
        /// </summary>
        /// <param name="sender">
        /// The event sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected void HandlePostProcessingEvent(object sender, EventArgs eventArgs)
        {
            // Forward to state machine framework for handling.
            EventQueuePostProcessing?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Clears all event subscriptions exposed to the state machine framework.
        /// Unregisters all custom event queues.
        /// </summary>
        protected virtual void ClearEventHandlers()
        {
            PresentationEventReceived = null;
            EventQueuePostProcessing = null;

            foreach(var customEventQueue in customEventQueues)
            {
                EventQueueManager.UnregisterEventQueue(customEventQueue);
            }
            customEventQueues.Clear();
        }

        #endregion
    }
}