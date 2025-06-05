//-----------------------------------------------------------------------
// <copyright file = "VirtualMechanicalReels.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using MechanicalReels;

    /// <summary>
    /// Standalone implementation of the mechanical reels interface.
    /// </summary>
    public sealed class VirtualMechanicalReels : IMechanicalReels, ICabinetUpdate, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// List of reel features supported by this mechanical reels instance.
        /// </summary>
        private readonly Dictionary<string, ReelFeature> reelFeatures =
            new Dictionary<string, ReelFeature>();

        /// <summary>
        /// Flag which indicates if the simulated reels are active.
        /// </summary>
        private volatile bool running;

        /// <summary>
        /// Thread used to handle simulation of the reels.
        /// </summary>
        private readonly Thread simulationThread;

        /// <summary>
        /// Minimum command interval in milliseconds.
        /// </summary>
        private const int CheckInterval = 10;

        /// <summary>
        /// Timeout to wait for the processing thread to close after setting run to false. If it doesn't terminate in
        /// this time then it will be killed.
        /// </summary>
        private const int ThreadJoinTimeout = 1000;

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The queue of <see cref="EventArgs"/>.
        /// </summary>
        private readonly Queue<EventArgs> reelEventQueue = new Queue<EventArgs>();

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a mechanical reels instance which supports the given features.
        /// </summary>
        /// <param name="reelFeatureDescriptions">A description of the available reel devices.</param>
        public VirtualMechanicalReels(IEnumerable<ReelFeatureDescription> reelFeatureDescriptions)
        {
            if(reelFeatureDescriptions != null)
            {
                foreach(var reelFeatureDescription in reelFeatureDescriptions)
                {
                    var reelFeature = new ReelFeature(reelFeatureDescription);
                    reelFeatures[reelFeatureDescription.FeatureId] = reelFeature;
                    reelFeature.ReelStatusChangedEvent += (sender, statusEvent) => OnReelStatusChanged(statusEvent);
                    reelFeature.ReelsSpunStateChangedEvent += (sender, stateEvent) => OnReelsSpunStateChanged(stateEvent);
                }
            }

            //If there are no features then the simulation thread does not need to be ran.
            if(reelFeatures.Count > 0)
            {
                running = true;
                simulationThread = new Thread(SimulateReels);
                simulationThread.Start();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Main function for the reel simulation thread.
        /// </summary>
        private void SimulateReels()
        {
            while(running)
            {
                lock(reelFeatures)
                {
                    foreach(var reelFeature in reelFeatures)
                    {
                        reelFeature.Value.Process();
                    }
                }

                Thread.Sleep(CheckInterval);
            }
        }

        /// <summary>
        /// Verify the given feature ID.
        /// </summary>
        /// <param name="featureId">The feature ID to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if the featureId is null.</exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if the featureId references a feature which is not present.
        /// </exception>
        private void ValidateFeatureId(string featureId)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId), "The feature ID may not be null.");
            }
            if(!reelFeatures.ContainsKey(featureId))
            {
                throw new InvalidFeatureIdException(featureId);
            }
        }

        /// <summary>
        /// Invoke the reel status changed event.
        /// </summary>
        /// <param name="statusEvent">Event to raise.</param>
        private void OnReelStatusChanged(ReelStatusEventArgs statusEvent)
        {
            EnqueueEvent(statusEvent);
        }

        /// <summary>
        /// Invoke the reels spun state changed event.
        /// </summary>
        /// <param name="stateEvent">Event to raise.</param>
        private void OnReelsSpunStateChanged(ReelsSpunEventArgs stateEvent)
        {
            EnqueueEvent(stateEvent);
        }

        /// <summary>
        /// Enqueue an <see cref="EventArgs"/> event.
        /// </summary>
        /// <param name="eventArgs"></param>
        private void EnqueueEvent(EventArgs eventArgs)
        {
            lock(reelEventQueue)
            {
                reelEventQueue.Enqueue(eventArgs);
            }
        }

        #endregion

        #region IMechanicalReels Implementation

        /// <inheritdoc/>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChangedEvent;

        /// <inheritdoc/>
        public ICollection<ReelFeatureDescription> GetReelDevices()
        {
            ICollection<ReelFeatureDescription> reelDevices;

            lock(reelFeatures)
            {
                reelDevices = (from reelFeature in reelFeatures select reelFeature.Value.Description).ToList();
            }

            return reelDevices;
        }

        /// <inheritdoc/>
        public ReelCommandResult RequireDevice(string featureId)
        {
            // Do not verify the feature ID here because requiring a device that is not currently
            // connected is allowed.
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("The feature ID cannot be null or empty.", nameof(featureId));
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetOnlineStatus(string featureId, bool online)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                //This function does nothing in standalone.
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetRecoveryBehavior(string featureId, RecoveryOrder order, ReelDirection direction)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                //This function does not have an operation for standalone reels.
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetStopOrder(string featureId, ICollection<byte> reels)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].SetStopOrder(reels);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetSynchronousStops(string featureId, ICollection<ReelStop> reelStops)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].SetSynchronousStop(reelStops);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult Spin(string featureId, ICollection<SpinProfile> spinProfiles)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].Spin(spinProfiles);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult Stop(string featureId, ICollection<ReelStop> reelStops)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].Stop(reelStops);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousSpin(string featureId, ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].SynchronousSpin(speedIndex, spinProfiles);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousStop(string featureId, ICollection<byte> reels)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].SynchronousStop(reels);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult ApplyAttributes(string featureId, IDictionary<byte, SpinAttributes> attributes)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].ApplyAttributes(attributes);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult ChangeSpeed(string featureId, IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].ChangeSpeed(changeSpeedProfiles);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetToPosition(string featureId, ICollection<byte> reelStops, out bool foundationHandlesTiltWhileRecovering)
        {
            foundationHandlesTiltWhileRecovering = true;
            lock(reelFeatures)
            {
                ValidateFeatureId(featureId);
                reelFeatures[featureId].SetToPosition(reelStops);
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public void Update()
        {
            EventArgs eventToPost = null;

            lock(reelEventQueue)
            {
                if(reelEventQueue.Any())
                {
                    eventToPost = reelEventQueue.Dequeue();
                }
            }

            if(eventToPost != null)
            {
                switch(eventToPost)
                {
                    case ReelStatusEventArgs reelStatusEventArgs:
                        ReelStatusChangedEvent?.Invoke(this, reelStatusEventArgs);
                        break;
                    case ReelsSpunEventArgs reelsSpunEventArgs:
                        ReelsSpunStateChangedEvent?.Invoke(this, reelsSpunEventArgs);
                        break;
                }
            }
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Object finalizer.
        /// </summary>
        ~VirtualMechanicalReels()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);

            //The finalizer does not need to execute if the object has been disposed.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// </summary>
        /// <param name="disposing">
        /// Flag indicating if the object is being disposed. If true Dispose was called, if false the finalizer called
        /// this function. If the finalizer called the function, then only unmanaged resources should be released.
        /// </param>
        /// <remarks>Disposable implementation is being used to stop a thread.</remarks>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                running = false;
                if(simulationThread != null)
                {
                    simulationThread.Join(ThreadJoinTimeout);
                    if(simulationThread.IsAlive)
                    {
                        simulationThread.Abort();
                    }
                }

                disposed = true;
            }
        }

        #endregion
    }
}
