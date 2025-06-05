//-----------------------------------------------------------------------
// <copyright file = "ReelShelf.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet.MechanicalReels;

    /// <summary>
    /// Specific implementation of the MechanicalReelDevice for ReelShelf devices.
    /// </summary>
    public class ReelShelf : MechanicalReelDevice
    {
        /// <summary>
        /// Constructor taking a <see cref="ReelFeatureDescription"/> argument.
        /// </summary>
        /// <param name="description">The <see cref="ReelFeatureDescription"/> of this reel device.</param>
        /// <exception cref="ArgumentNullException">Thrown if description is null.</exception>
        public ReelShelf(ReelFeatureDescription description)
            : base(description)
        {
            IsEnabled = true;
        }

        /// <summary>
        /// <code>True</code> if this reel shelf is enabled.
        /// </summary>
        public bool IsEnabled { get; internal set; }

        /// <summary>
        /// Set the order reels should stop.
        /// </summary>
        /// <param name="order">The reel stop order, as an enumeration.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetStopOrder(ReelStopOrder order)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var indices = new List<byte>();

            // The actual index stop orders are specified. For no stop order, send no indexes.
            switch(order)
            {
                case ReelStopOrder.Ascending:
                    indices = Enumerable.Range(0, ReelCount).Select(n => (byte)n).ToList();
                    break;
                case ReelStopOrder.Descending:
                    indices = Enumerable.Range(0, ReelCount).Select(n => (byte)n).Reverse().ToList();
                    break;
                case ReelStopOrder.Off:
                    break;
            }

            var result = SetStopOrder(indices);

            return result;
        }

        /// <summary>
        /// Set the order reels should stop.
        /// </summary>
        /// <param name="reels">A list of reel indexes indicating the order reels should stop.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetStopOrder(ICollection<byte> reels)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            return MechanicalReels.SetStopOrder(Description.FeatureId, reels);
        }

        /// <summary>
        /// Set the reels to where they should be in the fastest manner. The actual commands
        /// and behavior used will depend on the version of the foundation connected.
        /// </summary>
        /// <param name="reelStops">A list of reel indexes indicating the order reels should stop.</param>
        /// <param name="doesFoundationTilt">Out parameter: true if the foundation tilts during the handling of reel recovering,
        /// false if the SDK needs to spin reels and manage reel events. </param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetToPosition(ICollection<byte> reelStops, out bool doesFoundationTilt)
        {
            if(!IsEnabled)
            {
                doesFoundationTilt = false;
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            return MechanicalReels.SetToPosition(Description.FeatureId, reelStops, out doesFoundationTilt);
        }

        /// <summary>
        /// Spin all reels using individual spin profiles.
        /// </summary>
        /// <param name="spinProfiles">The spin profiles for each reel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult Spin(ICollection<SpinProfile> spinProfiles)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            return MechanicalReels.Spin(Description.FeatureId, spinProfiles);
        }

        /// <summary>
        /// Stop all reels.
        /// </summary>
        /// <param name="reelStops">Specific stop indexes for each reel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult Stop(ICollection<ReelStop> reelStops)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            return MechanicalReels.Stop(Description.FeatureId, reelStops);
        }

        /// <summary>
        /// Spin all reels synchronously. If the 'Stop' field of each reel's <see cref="SynchronousSpinProfile"/> is
        /// set to 0xFF, then the reels will spin un-synced until it receives the SetSynchronousStops command.
        /// </summary>
        /// <param name="speedIndex">The speed all reels will spin at.</param>
        /// <param name="spinProfiles">Spin profiles for each reel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult SynchronousSpin(ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var spinResult = CurrentDeviceState != ReelsSpunState.AllStopped ?
                    ReelCommandResult.SyncSpinFailedNotAllInStoppedState :
                    MechanicalReels.SynchronousSpin(Description.FeatureId, speedIndex, spinProfiles);
            return spinResult;
        }

        /// <summary>
        /// Set the stop indexes for reels previously spun synchronously without stop indexes.
        /// Once this command is sent, the reels will sync themselves and spin until the initial
        /// duration sent in the spin command expires, or the player synchronously stops the reels
        /// (aka slamming.)
        /// </summary>
        /// <param name="reelStops">The stop indexes for each reel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult SetSynchronousStops(ICollection<ReelStop> reelStops)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var result = CurrentDeviceState == ReelsSpunState.AllSpinningUp?
                    MechanicalReels.SetSynchronousStops(Description.FeatureId, reelStops):
                    ReelCommandResult.SetSyncStopsFailedReelsNotSpinning;

            return result;
        }

        /// <summary>
        /// Stop the specified reels synchronously. If reels were spun without stop indexes set, then the
        /// SetSynchronousStops command must be sent before this command is processed.
        /// </summary>
        /// <param name="reels">Reel indexes to stop.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult SynchronousStop(ICollection<byte> reels)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var spinResult =
                CurrentDeviceState == ReelsSpunState.AllCompletedSpinUp ||
                CurrentDeviceState == ReelsSpunState.AllSpinningDown?
                    MechanicalReels.SynchronousStop(Description.FeatureId, reels):
                    ReelCommandResult.SyncStopFailedShelfNotInCorrectState;
            return spinResult;
        }

        /// <summary>
        /// This sets the recover-from-tilt stop order.
        /// <param name="spunStopOrder">The <see cref="ReelStopOrder"/> stop indexes for each reel.</param>
        /// </summary>
        public virtual ReelCommandResult SetRecoveryBehavior(ReelStopOrder spunStopOrder)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var recoveryOrder = RecoveryOrder.Ascending;

            switch(spunStopOrder)
            {
                case ReelStopOrder.Ascending:
                case ReelStopOrder.Off:
                    recoveryOrder = RecoveryOrder.Ascending;
                    break;
                case ReelStopOrder.Descending:
                    recoveryOrder = RecoveryOrder.Descending;
                    break;
            }

            return MechanicalReels.SetRecoveryBehavior(Description.FeatureId, recoveryOrder, ReelDirection.Descending);
        }

        /// <summary>
        /// Apply reel spin attributes; should be done only when reels are not in a spin. If called during a reel spin, the results are
        /// undetermined.
        /// </summary>
        /// <param name="reelAttributes">The dictionary of reel index vs. <see cref="SpinAttributes"/> attributes.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult ApplyReelAttributes(IDictionary<byte, SpinAttributes> reelAttributes)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            return MechanicalReels.ApplyAttributes(Description.FeatureId, reelAttributes);
        }

        /// <summary>
        /// Clear all attributes currently set; should be done only when reels are not in a spin. If called during a reel spin,
        /// the results are undetermined.
        /// </summary>
        /// <param name="reelsToClear">The list of reel indexes to clear attributes for.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult ClearReelAttributes(List<byte> reelsToClear)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            var reelAttributes = new Dictionary<byte, SpinAttributes>();
            foreach(var reelNum in reelsToClear)
            {
                if(!reelAttributes.ContainsKey(reelNum))
                {
                    reelAttributes.Add(reelNum, new SpinAttributes());
                }
            }

            return MechanicalReels.ApplyAttributes(Description.FeatureId, reelAttributes);
        }

        /// <summary>
        /// Change the speed and/or direction of a set of spinning reels (reels must be spinning at 0 RPMS or this command is ignored.)
        /// </summary>
        /// <param name="changeSpeedProfiles">The list of reel indexes to apply speed change parameters to.</param>
        /// <devdoc> The passed collection can contain null entries which need to be removed. This can happen if the game creates a
        /// list as long as the max number of reels, with default ChangeSpeed profile objects,
        /// then sets one or more of these entries to null to indicate that certain reel(s) should not change their speed.
        /// </devdoc>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult ChangeSpeed(ICollection<ChangeSpeedProfile> changeSpeedProfiles)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            if(changeSpeedProfiles == null || !changeSpeedProfiles.Any())
            {
                return ReelCommandResult.InvalidReelCountParameter;
            }

            var setDirectionProfiles = new Dictionary<byte, ChangeSpeedProfile>();
            foreach(var profile in changeSpeedProfiles)
            {
                if(profile != null)
                {
                    setDirectionProfiles[profile.Number] = profile;
                }
            }

            return MechanicalReels.ChangeSpeed(Description.FeatureId, setDirectionProfiles);
        }

        /// <summary>
        /// Sets current game side in recovery spin mode, which includes a tilt while reels are spinning.
        /// </summary>
        /// <param name="tilt"><c>True</c> to post a tilt, false to clear.</param>
        /// <returns>
        ///     <code>ReelCommandResult.Success</code> if tilt is posted/cleared.
        ///     <code>ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled</code> if ReelShelf is disabled.
        /// </returns>
        public ReelCommandResult SetRecoveringMode(bool tilt)
        {
            if(!IsEnabled)
            {
                return ReelCommandResult.CommandIgnoredAsReelDeviceIsNotEnabled;
            }

            if(tilt)
            {
                MechanicalReelLocalizedTiltHelper.PostTilt(MechanicalReelLocalizedTiltHelper.ReelsInRecoveryModeTiltKey);
            }
            else
            {
                MechanicalReelLocalizedTiltHelper.ClearTilt(MechanicalReelLocalizedTiltHelper.ReelsInRecoveryModeTiltKey);
            }

            return ReelCommandResult.Success;
        }
    }
}
