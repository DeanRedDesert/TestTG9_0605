//-----------------------------------------------------------------------
// <copyright file = "ReelFeature.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas;
    using MechanicalReels;
    using Timing;

    /// <summary>
    /// Description of a reel feature and its state.
    /// </summary>
    internal sealed class ReelFeature
    {
        #region Nested Classes

        /// <summary>
        /// Class which is used to schedule a service for a reel state.
        /// </summary>
        private class ReelService
        {
            /// <summary>
            /// The time the service was scheduled.
            /// </summary>
            public TimeSpan Scheduled;

            /// <summary>
            /// The delay for the service.
            /// </summary>
            public int Delay;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Dictionary containing reel states that need to be serviced and the time until they are to be serviced.
        /// </summary>
        private readonly Dictionary<ReelState, ReelService> servicePending =
            new Dictionary<ReelState, ReelService>();

        /// <summary>
        /// List of reel states for the feature.
        /// </summary>
        private readonly List<ReelState> reelStates = new List<ReelState>();

        /// <summary>
        /// The order that reels will stop in by reel index.
        /// </summary>
        private List<byte> reelStopOrder;

        /// <summary>
        /// Object used for locking access to the class fields.
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// List of events registered for this device.
        /// </summary>
        // TODO: private member never used
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<ReelStatusEventArgs> registeredEvents = new List<ReelStatusEventArgs>();

        /// <summary>
        /// Time, in milliseconds, to increase reel delays by to enforce stop orders.
        /// </summary>
        private const int StopOrderEnforcementTime = 10;

        /// <summary>
        /// The max reels used - should be equal to the reels on the device.
        /// </summary>
        private readonly int maxReelsInUse;

        /// <summary>
        /// The actual number of reels spun.
        /// </summary>
        private int reelsSpunCount;

        /// <summary>
        /// Counters for internal reel events.
        /// </summary>
        private int accelCount;
        private int constantCount;
        private int decelCount;
        private int stoppedCount;

        /// <summary>
        /// Keeps track of the current state of a set of spinning reels.
        /// </summary>
        private ReelsSpunState currentReelsSpunState;

        /// <summary>
        /// Keeps track of the previous state of a set of spinning reels.
        /// </summary>
        private ReelsSpunState previousReelState = ReelsSpunState.AllStopped;

        #endregion

        #region Public Properties

        /// <summary>
        /// Description of the reel feature.
        /// </summary>
        public ReelFeatureDescription Description { get; }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets/sets the count of reels in use for this spin - may be less than the reels
        /// available on the device or what the game normally uses.
        /// </summary>
        private int ReelsSpunCount
        {
            get => reelsSpunCount;
            set
            {
                reelsSpunCount = value <= maxReelsInUse ? value : maxReelsInUse;
                accelCount = 0;
                constantCount = 0;
                decelCount = 0;
                stoppedCount = 0;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// EventHandler for <see cref="ReelStatusEventArgs"/> individual reel events.
        /// </summary>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChangedEvent;

        /// <summary>
        /// EventHandler for <see cref="ReelsSpunEventArgs"/> device-level events.
        /// </summary>
        public event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChangedEvent;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a reel feature based on the given description.
        /// </summary>
        /// <param name="description">A description of the reel feature.</param>
        public ReelFeature(ReelFeatureDescription description)
        {
            Description = description;
            maxReelsInUse = description.ReelDescriptions.Count();

            for(byte reelIndex = 0; reelIndex < maxReelsInUse; reelIndex++)
            {
                var reelState = new ReelState(description.FeatureId, reelIndex);
                reelStates.Add(reelState);
                reelState.ReelStatusChanged += OnReelStatusChanged;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>           
        /// Process the device and handle any pending actions.
        /// </summary>
        public void Process()
        {
            lock(locker)
            {
                var serviceTime = TimeSpanWatch.Now;

                var servicePendingCopy = new Dictionary<ReelState, ReelService>(servicePending);

                foreach(var reelService in servicePendingCopy)
                {
                    if(reelService.Value.Delay < (serviceTime - reelService.Value.Scheduled).TotalMilliseconds)
                    {
                        var delay = reelService.Key.Process();

                        if(!reelService.Key.Active)
                        {
                            servicePending.Remove(reelService.Key);
                        }
                        else
                        {
                            servicePending[reelService.Key] = new ReelService { Delay = delay, Scheduled = serviceTime };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Used to force the stop order of the reels. This ensures that the reels will stop in the correct order even
        /// if the spin times are not close enough to guarantee it. The function takes a list of reel indexes and the
        /// order of these indexes will be the desired stop order.
        /// </summary>
        /// <param name="reels">
        /// Collection containing reel numbers. The order of the collection indicates the order the reels should stop.
        /// If the collection is empty, then the stop order is cleared.
        /// </param>
        /// <remarks>
        /// Stop order applies to all the reels. Reels which are excluded from the list are reset to be based on
        /// time even if they were previously set with a stop order command.
        /// </remarks>
        public void SetStopOrder(ICollection<byte> reels)
        {
            ReelCommandVerifier.VerifySetStopOrder(Description, reels);
            lock(locker)
            {
                reelStopOrder = new List<byte>(reels);
            }
        }

        /// <summary>
        /// Apply attributes.
        /// </summary>
        /// <param name="attributes">
        /// Collection containing reel numbers vs. a <see cref="SpinAttributes"/> to apply to that reel.
        /// </param>
        public void ApplyAttributes(IDictionary<byte, SpinAttributes> attributes)
        {
            ReelCommandVerifier.VerifyReelIndexes(Description, attributes.Keys.ToList());
            // This command does nothing with virtual reels.
        }

        /// <summary>
        /// Change speed.
        /// </summary>
        /// <param name="changeSpeedProfiles"></param>The collection containing reel numbers vs. a 
        /// <see cref="ChangeSpeedProfile"/> to apply to that reel.
        public void ChangeSpeed(IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            ReelCommandVerifier.VerifyChangeSpeedProfiles(Description, changeSpeedProfiles);
            // This command does nothing with virtual reels.
        }

        /// <summary>
        /// Move all reels to the designated reel stops.
        /// </summary>
        /// <param name="reelStops">The collection of reel stops to move the reels to.</param>
        public void SetToPosition(ICollection<byte> reelStops)
        {
            ReelCommandVerifier.VerifyStops(Description, reelStops);

            lock(locker)
            {
                var reelSpins = reelStops.Select((reelStop, reelIndex) => new SpinProfile
                {
                    Attributes = null,
                    Deceleration = 0,
                    ReelNumber = (byte)reelIndex,
                    Direction = ReelDirection.Shortest,
                    Duration = 0,
                    Speed = 0,
                    Stop = reelStop
                }).ToList();

                Spin(reelSpins);
            }
        }

        /// <summary>
        /// Set the given synchronous stops.
        /// </summary>
        /// <param name="reelStops">The reel stops to set.</param>
        public void SetSynchronousStop(ICollection<ReelStop> reelStops)
        {
            ReelCommandVerifier.VerifyStops(Description, reelStops);

            lock(locker)
            {
                foreach(var reelStop in reelStops)
                {
                    var reelState = reelStates[reelStop.ReelNumber];
                    reelState.SetSynchronousStop(reelStop);
                }
            }
        }

        /// <summary>
        /// Spin the reels specified by the given profiles with the characteristics specified in the profiles.
        /// </summary>
        /// <param name="spinProfiles">List of spin profiles used to spin the reels.</param>
        public void Spin(ICollection<SpinProfile> spinProfiles)
        {
            ReelCommandVerifier.VerifySpin(Description, spinProfiles);
            ReelsSpunCount = spinProfiles.Count;

            lock(locker)
            {
                if(reelStopOrder != null && reelStopOrder.Count != 0)
                {
                    var lastSpinTime = 0;

                    //Reset the stop order overrides.
                    foreach(var reelState in reelStates)
                    {
                        reelState.SetOverrideDuration(0);
                    }

                    //Configure the overridden stop order.
                    foreach(var reel in reelStopOrder)
                    {
                        var reelState = reelStates[reel];

                        //If the spin time for any reel is less than or equal to the spin time of the reel to stop
                        //before it, then the spin time of that reel should be increased to be larger than the spin
                        //time of the previous reel.
                        if(reelState.Duration <= lastSpinTime)
                        {
                            reelState.SetOverrideDuration((ushort)(lastSpinTime + reelState.TimingDelay + StopOrderEnforcementTime));
                        }

                        lastSpinTime = reelState.Duration;
                    }
                }

                foreach(var spinProfile in spinProfiles)
                {
                    var reelState = reelStates[spinProfile.ReelNumber];
                    var delay = reelState.Spin(spinProfile);
                    ScheduleService(delay, reelState);
                }
            }
        }

        /// <summary>
        /// Stop the specified reels at the specified stops.
        /// </summary>
        /// <param name="reelStops">The reels and their stops.</param>
        public void Stop(ICollection<ReelStop> reelStops)
        {
            ReelCommandVerifier.VerifyStops(Description, reelStops);

            lock(locker)
            {
                foreach(var reelStop in reelStops)
                {
                    var reelState = reelStates[reelStop.ReelNumber];
                    var delay = reelState.Stop(reelStop);
                    ScheduleService(delay, reelState);
                }
            }
        }

        /// <summary>
        /// Synchronously spin the reels with the given profiles.
        /// </summary>
        /// <param name="spinProfiles">Synchronous spin profiles to use for spinning the reels.</param>
        /// <param name="speedIndex">Index which indicates which of the supported spin speeds to use.</param>
        public void SynchronousSpin(ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles)
        {
            ReelCommandVerifier.VerifySynchronousSpin(Description, speedIndex, spinProfiles);
            ReelsSpunCount = spinProfiles.Count;

            lock(locker)
            {
                foreach(var spinProfile in spinProfiles)
                {
                    var reelState = reelStates[spinProfile.ReelNumber];
                    var delay = reelState.SynchronousSpin(spinProfile);
                    ScheduleService(delay, reelState);
                }
            }
        }

        /// <summary>
        /// Stop the given reels synchronously.
        /// </summary>
        /// <param name="reels">The reels to stop.</param>
        public void SynchronousStop(ICollection<byte> reels)
        {
            ReelCommandVerifier.VerifySynchronousStop(Description, reels);

            lock(locker)
            {
                foreach(var reel in reels)
                {
                    var reelState = reelStates[reel];
                    var delay = reelState.SynchronousStop();
                    ScheduleService(delay, reelState);
                }
            }
        }

        /// <summary>
        /// Clear all the event registrations for this device.
        /// </summary>
        public void ClearRegisteredEvents()
        {
            lock(registeredEvents)
            {
                registeredEvents.Clear();
            }
        }

        /// <summary>
        /// Populate the list of events to post.
        /// </summary>
        /// <param name="internallyRegisteredEvents">The list of <see cref="ReelStatus"/> events to post.</param>
        public void CreateEventsToPostList(List<ReelStatus> internallyRegisteredEvents)
        {
            lock(registeredEvents)
            {
                registeredEvents.Clear();
                var featureId = Description.FeatureId;

                foreach(var reelStatus in internallyRegisteredEvents)
                {
                    for(byte reelIndex = 0; reelIndex < Description.ReelDescriptions.Count(); reelIndex++)
                    {
                        registeredEvents.Add(new ReelStatusEventArgs
                        {
                            FeatureId = featureId,
                            ReelNumber = reelIndex,
                            Status = reelStatus
                        });
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// If the reel state is active schedule a service after the given delay.
        /// </summary>
        /// <param name="delay">Delay at which to service the state.</param>
        /// <param name="reelState">The reel state to schedule the service for.</param>
        private void ScheduleService(int delay, ReelState reelState)
        {
            if(reelState.Active)
            {
                //Schedule a process callback.
                servicePending[reelState] = new ReelService
                {
                    Delay = delay,
                    Scheduled = TimeSpanWatch.Now
                };
            }
        }

        /// <summary>
        /// Handle reel status changed events from the reel states.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="reelStatusEventArgs">Arguments for the event.</param>
        private void OnReelStatusChanged(object sender, ReelStatusEventArgs reelStatusEventArgs)
        {
            lock(registeredEvents)
            {
                OnReelStatusChanged(reelStatusEventArgs);
            }
        }

        /// <summary>
        /// Invoke the reel status changed event.
        /// </summary>
        /// <param name="statusEvent">Event to raise.</param>
        private void OnReelStatusChanged(ReelStatusEventArgs statusEvent)
        {
            SyncAndPostReelStates(statusEvent);
        }

        /// <summary>
        /// Synchronizes reel states based on reel events. Posts events up the chain.
        /// </summary>
        /// <param name="reelStatusEvent">Event args of type <see cref="ReelStatusEventArgs"/>.</param>
        private void SyncAndPostReelStates(ReelStatusEventArgs reelStatusEvent)
        {
            var stateChange = UpdateStateWithNewEvent(reelStatusEvent);

            // Send the original event up the chain.
            PostReelStatusChanged(new ReelStatusEventArgs
            {
                FeatureId = reelStatusEvent.FeatureId,
                ReelNumber = reelStatusEvent.ReelNumber,
                Status = reelStatusEvent.Status
            });

            // Send a state change up the chain.
            if(stateChange)
            {
                PostReelsSpunStateChanged(new ReelsSpunEventArgs
                {
                    FeatureId = reelStatusEvent.FeatureId,
                    ReelsSpunState = currentReelsSpunState
                });
            }
        }

        /// <summary>
        /// Maintains the reels spun state by counting individual reel events.
        /// </summary>
        /// <param name="reelStatusEvent">A reel event of <see cref="ReelStatusEventArgs"/>.</param>
        /// <returns>Flag indicating whether a reels spun state change has occurred.</returns>
        private bool UpdateStateWithNewEvent(ReelStatusEventArgs reelStatusEvent)
        {
            bool stateChange;

            lock(locker)
            {
                switch(reelStatusEvent.Status)
                {
                    case ReelStatus.Accelerating:
                        if(++accelCount >= ReelsSpunCount)
                        {
                            currentReelsSpunState = ReelsSpunState.AllSpinningUp;
                        }

                        break;

                    case ReelStatus.ConstantSpeed:
                        if(++constantCount >= ReelsSpunCount)
                        {
                            currentReelsSpunState = ReelsSpunState.AllCompletedSpinUp;
                        }

                        break;

                    case ReelStatus.Decelerating:
                        if(++decelCount >= ReelsSpunCount)
                        {
                            currentReelsSpunState = ReelsSpunState.AllSpinningDown;
                        }

                        break;

                    case ReelStatus.Stopped:
                        if(++stoppedCount >= ReelsSpunCount)
                        {
                            currentReelsSpunState = ReelsSpunState.AllStopped;
                        }
                        break;
                }

                stateChange = currentReelsSpunState != previousReelState;
                previousReelState = currentReelsSpunState;
            }

            return stateChange;
        }

        /// <summary>
        /// Invoke the reel status changed event.
        /// </summary>
        /// <param name="statusEvent">Event to raise.</param>
        private void PostReelStatusChanged(ReelStatusEventArgs statusEvent)
        {
            ReelStatusChangedEvent?.Invoke(this, statusEvent);
        }

        /// <summary>
        /// Invoke the reels spun state changed event.
        /// </summary>
        /// <param name="stateEvent">Event to raise.</param>
        private void PostReelsSpunStateChanged(ReelsSpunEventArgs stateEvent)
        {
            ReelsSpunStateChangedEvent?.Invoke(this, stateEvent);
        }

        #endregion
    }
}
