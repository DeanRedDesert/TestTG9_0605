//-----------------------------------------------------------------------
// <copyright file = "ReelsSpunStateManager.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas;

    #region Internal Enums

    /// <summary>
    /// Defines the states that a single reel affected by a spin command can be in.
    /// </summary>
    enum ReelStateInternal
    {
        /// <summary>
        /// Reel is stopped or moving irregularly, available for a new command.
        /// </summary>
        WaitingForCommand,

        /// <summary>
        /// Reel has started spinning.
        /// </summary>
        Accelerating,

        /// <summary>
        /// Reel has reached constant speed.
        /// </summary>
        ConstantSpeed,

        /// <summary>
        /// Reel has started decelerating.
        /// </summary>
        Decelerating,

        /// <summary>
        /// Reel has stopped after a spin.
        /// </summary>
        Stopped,

        /// <summary>
        /// Reel has started moving irregularly.
        /// </summary>
        MovingIrregularly
    }

    #endregion

    /// <summary>
    /// A class that describes and manages the overall state of currently moving reels.
    /// </summary>
    public class ReelsSpunStateManager
    {
        /// <summary>
        /// A dictionary keyed with reel index and holding an IDictionary as its value; this IDictionary is keyed off of
        /// each possible <see cref="ReelStateInternal"/> that a reel can go through, with a bool value indicating
        /// if the reel has gone through that state yet.
        /// </summary>
        private readonly Dictionary<byte, IDictionary<ReelStateInternal, bool>> currentReelStatuses =
            new Dictionary<byte, IDictionary<ReelStateInternal, bool>>();

        /// <summary>
        /// Gets/private sets the previous overall <see cref="ReelsSpunState"/> of the device.
        /// </summary>
        public ReelsSpunState PreviousReelsSpunState
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/private sets the current overall <see cref="ReelsSpunState"/> of the device.
        /// </summary>
        public ReelsSpunState CurrentReelsSpunState
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/sets the reels present on the device.
        /// </summary>
        private int MaxReelCount
        {
            get;
        }

        /// <summary>
        /// Gets/sets the count of reels in use for this spin state cycle - may be less than the reels
        /// available on the device or what the game normally uses. Setting this property resets all reel
        /// statuses to Stopped.
        /// </summary>
        private int ReelsSpunCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the count of reels that were spun with spin attributes (for example, 'hover') for this spin state cycle.
        /// These reels need special handling as they are treated differently when determining device level events.
        /// </summary>
        public int ReelsSpunWithAttributesCount
        {
            get;
            set;
        }

        /// <summary>
        /// The device level Acceleration event has been posted for this spin cycle.
        /// </summary>
        private bool AccelDeviceEventPosted { get; set; }

        /// <summary>
        /// The device level Deceleration event has been posted for this spin cycle.
        /// </summary>
        private bool DecelDeviceEventPosted { get; set; }

        /// <summary>
        /// The device level Stopped event has been posted for this spin cycle.
        /// </summary>
        private bool StoppedDeviceEventPosted { get; set; }

        /// <summary>
        /// The device level Spinning Irregularly event has been posted for this spin cycle.
        /// </summary>
        private bool IrregDeviceEventPosted { get; set; }

        /// <summary>
        /// The device level Constant Speed event has been posted for this spin cycle.
        /// </summary>
        private bool ConstantDeviceEventPosted { get; set; }

        /// <summary>
        /// Resets the internal synchronization structures used for reel spinning and stopping.
        /// Called before a reel spin command, and when a reel device goes offline, then back online.
        /// </summary>
        /// <param name="reelsSpunCount">The number of reels spun for this spin-cycle.</param>
        public void SetInitialReelStates(int reelsSpunCount)
        {
            if(reelsSpunCount < 1)
            {
                throw new ArgumentException("The number of reels being managed cannot be less than 1.", nameof(reelsSpunCount));
            }

            // Reset the individual reel statuses. The device level events should be reset only in
            // 'UpdateStateWithNewEvent', when the correct set of events and states have occurred.
            ResetReelStatuses();
            ReelsSpunCount = reelsSpunCount;
            CurrentReelsSpunState = ReelsSpunState.AllWaitingForCommand;
        }

        /// <summary>
        /// Updates the current reel device state based on a new <see cref="ReelStatusEventArgs"/>.
        /// </summary>
        /// <param name="reelStatusEvent">A new <see cref="ReelStatusEventArgs"/> to be processed.</param>
        /// <returns>Flag indicating a state change occurred.</returns>
        public bool UpdateStateWithNewEvent(ReelStatusEventArgs reelStatusEvent)
        {
            // Check if a reel index is valid.
            currentReelStatuses.TryGetValue(reelStatusEvent.ReelNumber, out var reelState);

            if(reelState == null)
            {
                return false;
            }

            reelState[MapCsiReelStatusToReelSpunState(reelStatusEvent.Status)] = true;

            var accelCount = currentReelStatuses.Values.Aggregate(0, (currenState, currentReelStatus) =>
                currentReelStatus[ReelStateInternal.Accelerating] ? ++currenState : currenState);
            var constantCount = currentReelStatuses.Values.Aggregate(0, (current, currentReelStatus) =>
                currentReelStatus[ReelStateInternal.ConstantSpeed] ? ++current : current);
            var decelCount = currentReelStatuses.Values.Aggregate(0, (current, currentReelStatus) =>
                currentReelStatus[ReelStateInternal.Decelerating] ? ++current : current);
            var stoppedCount = currentReelStatuses.Values.Aggregate(0, (current, currentReelStatus) =>
                currentReelStatus[ReelStateInternal.Stopped] ? ++current : current);
            var irregularCount = currentReelStatuses.Values.Aggregate(0, (current, currentReelStatus) =>
                currentReelStatus[ReelStateInternal.MovingIrregularly] ? ++current : current);

            // Set device states based on reel state counts.
            // All reels have started moving irregularly.
            if (!IrregDeviceEventPosted && irregularCount > 0 && irregularCount == ReelsSpunWithAttributesCount)
            {
                CurrentReelsSpunState = ReelsSpunState.AllMovingIrregularly;
                IrregDeviceEventPosted = true;
            }
            // All reels have reached constant speed.
            if(!ConstantDeviceEventPosted && constantCount == ReelsSpunCount)
            {
                CurrentReelsSpunState = ReelsSpunState.AllCompletedSpinUp;
                ConstantDeviceEventPosted = true;
            }
            // All reels have started spinning or reached constant speed.
            if(!AccelDeviceEventPosted && accelCount == ReelsSpunCount)
            {
                CurrentReelsSpunState = ReelsSpunState.AllSpinningUp;
                AccelDeviceEventPosted = true;
            }
            // All reels are beginning to stop or have stopped.
            if(!DecelDeviceEventPosted && decelCount == ReelsSpunCount)
            {
                CurrentReelsSpunState = ReelsSpunState.AllSpinningDown;
                DecelDeviceEventPosted = true;
            }
            // Check to see if all reels that were spun without an attribute have stopped.
            if(!StoppedDeviceEventPosted && stoppedCount > 0 && stoppedCount == ReelsSpunCount)
            {
                CurrentReelsSpunState = ReelsSpunState.AllStopped;
                StoppedDeviceEventPosted = true;
                ResetDeviceStateChange();
            }

            var stateChange = CurrentReelsSpunState != PreviousReelsSpunState;
            PreviousReelsSpunState = CurrentReelsSpunState;

            return stateChange;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ReelsSpunStateManager"/>.
        /// </summary>
        /// <param name="maxReelCount">The Maximum number of reels to be managed. This is usually equal to
        /// the number of reels on the device and the number of reels being spun, but not always.</param>
        public ReelsSpunStateManager(int maxReelCount)
        {
            MaxReelCount = maxReelCount;
            ResetReelStatuses();
            ReelsSpunWithAttributesCount = 0;
            ResetDeviceStateChange();
        }

        #region Private Methods

        /// <summary>
        /// Resets flags indicating that the reel device has gone through a complete spin-cycle state change.
        /// </summary>
        private void ResetDeviceStateChange()
        {
            AccelDeviceEventPosted = false;
            DecelDeviceEventPosted = false;
            StoppedDeviceEventPosted = false;
            IrregDeviceEventPosted = false;
            ConstantDeviceEventPosted = false;
        }

        /// <summary>
        /// Resets the reel statuses for all reels present on the reel device.
        /// </summary>
        private void ResetReelStatuses()
        {
            currentReelStatuses.Clear();

            for(byte reel = 0; reel < MaxReelCount; reel++)
            {
                currentReelStatuses.Add(reel, new Dictionary<ReelStateInternal, bool>
                {
                    {ReelStateInternal.WaitingForCommand, true},
                    {ReelStateInternal.Accelerating, false},
                    {ReelStateInternal.ConstantSpeed, false},
                    {ReelStateInternal.Decelerating, false},
                    {ReelStateInternal.Stopped, false},
                    {ReelStateInternal.MovingIrregularly, false}
                });
            }
        }

        /// <summary>
        /// Maps an incoming CSI event type of <see cref="ReelStatus"/> to a <see cref="ReelStateInternal"/>.
        /// </summary>
        /// <param name="reelStatus">The CSI reel status event.</param>
        /// <returns>The mapped value.</returns>
        private ReelStateInternal MapCsiReelStatusToReelSpunState(ReelStatus reelStatus)
        {
            var result = ReelStateInternal.WaitingForCommand;

            switch(reelStatus)
            {
                case ReelStatus.Accelerating:
                    result = ReelStateInternal.Accelerating;
                    break;
                case ReelStatus.ConstantSpeed:
                    result = ReelStateInternal.ConstantSpeed;
                    break;
                case ReelStatus.Decelerating:
                    result = ReelStateInternal.Decelerating;
                    break;
                case ReelStatus.MovingIrregularly:
                    result = ReelStateInternal.MovingIrregularly;
                    break;
                case ReelStatus.Stopped:
                    result = ReelStateInternal.Stopped;
                    break;
            }

            return result;
        }

        #endregion
    }
}
