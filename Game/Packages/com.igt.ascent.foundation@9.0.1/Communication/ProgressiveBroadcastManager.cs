// -----------------------------------------------------------------------
// <copyright file = "ProgressiveBroadcastManager.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// This class manages the timing of the progressive broadcasting by the event-driven mechanism.
    /// The event listeners should broadcast their progressives timely based on the event notifications.
    /// </summary>
    /// <devdoc>
    /// This class must be public because it is used by Executable Extensions.
    /// </devdoc>
    public sealed class ProgressiveBroadcastManager : IEventSource, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The timer which is used to trigger the broadcasting periodically.
        /// </summary>
        private readonly Timer progressiveTimer;

        /// <summary>
        /// The time interval between posting the progressive
        /// broadcast event.
        /// </summary>
        private readonly uint progressiveBroadcastInterval;
        
        /// <summary>
        /// This wait handle is used to signal the progressive broadcasting event.
        /// </summary>
        private readonly ManualResetEvent broadcastPosted = new ManualResetEvent(false);

        /// <summary>
        /// The list of requesters who wanted to enable the broadcasting.
        /// </summary>
        private readonly List<string> enablingRequesters = new List<string>();

        /// <summary>
        /// This flag indicates if the broadcast timing is started.
        /// <see cref="ProgressiveBroadcastEvent"/> can be triggered only when both this flag
        /// and <see cref="broadcastingActivated"/> are true.
        /// </summary>
        private bool broadcastTimingEnabled;

        /// <summary>
        /// This flag indicates if the broadcasting is activated.
        /// <see cref="ProgressiveBroadcastEvent"/> can be triggered only when both this flag
        /// and <see cref="broadcastTimingEnabled"/> are true.
        /// </summary>
        private bool broadcastingActivated;

        #endregion

        #region Events

        /// <summary>
        /// This event is triggered to notify the progressive broadcasting periodically.
        /// </summary>
        public event EventHandler ProgressiveBroadcastEvent;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the instance with the progressive broadcast interval.
        /// </summary>
        /// <param name="progressiveBroadcastInterval">
        /// The time interval between posting the progressive broadcast event.
        /// </param>
        public ProgressiveBroadcastManager(uint progressiveBroadcastInterval)
        {
            this.progressiveBroadcastInterval = progressiveBroadcastInterval;
            progressiveTimer = new Timer(NotifyProgressiveBroadcasting, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The callback method of <see cref="progressiveTimer" />.
        /// </summary>
        /// <param name="stateInfo">The state information of the timer.</param>
        private void NotifyProgressiveBroadcasting(object stateInfo)
        {
            broadcastPosted.Set();
        }

        /// <summary>
        /// Update the broadcast timing status.
        /// </summary>
        private void UpdateBroadcastStatus()
        {
            if(broadcastTimingEnabled && broadcastingActivated)
            {
                progressiveTimer.Change(0, progressiveBroadcastInterval);
            }
            else
            {
                progressiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        /// <summary>
        /// This function can execute by being called from the Dispose
        /// method of the IDisposable interface or from the finalizer.
        /// If called from Dispose, then managed and unmanaged resources
        /// may be released. If called from a finalizer, then only
        /// unmanaged resources may be released.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(disposing)
            {
                // Dispose the progressive broadcast timer.
                progressiveTimer.Dispose();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enable or disable the broadcasting.
        /// </summary>
        /// <param name="enabling">True to enable the broadcasting; otherwise, false.</param>
        /// <param name="requester">The identifier of the caller.</param>
        public void EnableBroadcasting(bool enabling, string requester = "")
        {
            if(!enabling)
            {
                enablingRequesters.Remove(requester);
            }
            else if(!enablingRequesters.Contains(requester))
            {
                enablingRequesters.Add(requester);
            }

            broadcastTimingEnabled = enablingRequesters.Count > 0;

            UpdateBroadcastStatus();
        }

        /// <summary>
        /// Activate or deactivate the progressive broadcast manager.
        /// </summary>
        /// <param name="activating">True to activate the broadcast manager; otherwise, false.</param>
        public void Activate(bool activating)
        {
            broadcastingActivated = activating;
            UpdateBroadcastStatus();
        }

        #endregion

        #region Implementation of IEventSource

        /// <inheritDoc />
        public WaitHandle EventPosted => broadcastPosted;

        /// <inheritDoc />
        public void ProcessEvents()
        {
            if(broadcastPosted.WaitOne(0))
            {
                if(broadcastingActivated && broadcastTimingEnabled)
                {
                    ProgressiveBroadcastEvent?.Invoke(this, EventArgs.Empty);
                }

                broadcastPosted.Reset();
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <inheritDoc />
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}