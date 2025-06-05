// -----------------------------------------------------------------------
// <copyright file = "EventProcessingMonitor.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Threading;
    using Communication.Foundation;

    /// <summary>
    /// This class is used to momentarily registered to <see cref="IEventProcessing.EventProcessed" /> until
    /// the waiting loop quits in <see cref="StateMachineFramework.WaitForNonTransactionalEvents"/>.
    /// </summary>
    internal class EventProcessingMonitor : IDisposable
    {
        /// <summary>
        /// This object broadcasts the event processed events.
        /// </summary>
        private readonly IEventProcessing eventProcessor;

        /// <summary>
        /// The predicate used to reset <see cref="waitHandle"/> when it verifies true.
        /// </summary>
        private readonly Func<bool> predicate;

        /// <summary>
        /// The wait handle used to notify <see cref="EventCoordinator.ProcessEvents"/> to return.
        /// </summary>
        private readonly ManualResetEvent waitHandle = new ManualResetEvent(false);

        /// <summary>
        /// This wait handle indicates the passed in preciate already verifies true when signalled.
        /// </summary>
        public WaitHandle EventProcessedWaitHandle
        {
            get { return waitHandle; }
        }

        /// <summary>
        /// Construct the monitor with the event processing processor and the custom predicate delegate.
        /// </summary>
        /// <param name="eventProcessor">The event processor.</param>
        /// <param name="predicate">The custom predicate delegate.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public EventProcessingMonitor(IEventProcessing eventProcessor, Func<bool> predicate)
        {
            if(eventProcessor == null)
            {
                throw new ArgumentNullException("eventProcessor");
            }

            if(predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            this.eventProcessor = eventProcessor;
            this.predicate = predicate;
            this.eventProcessor.EventProcessed += HandleEventProcessed;
        }

        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            eventProcessor.EventProcessed -= HandleEventProcessed;
            waitHandle.Close();
        }

        #endregion

        /// <summary>
        /// Check the predicate state and reset <see cref="EventProcessedWaitHandle"/>.
        /// </summary>
        /// <returns>The state of the passed in predicate.</returns>
        public bool CheckPredicate()
        {
            waitHandle.Reset();
            return predicate();
        }

        /// <summary>
        /// Handle the event processed events from  the event processor.
        /// </summary>
        /// <param name="sender">The event processor.</param>
        /// <param name="eventArgs">The payload of the event.</param>
        private void HandleEventProcessed(object sender, EventArgs eventArgs)
        {
            waitHandle.Set();
        }
    }
}