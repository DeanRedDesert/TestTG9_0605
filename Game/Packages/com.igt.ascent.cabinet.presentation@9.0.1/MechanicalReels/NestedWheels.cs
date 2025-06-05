//-----------------------------------------------------------------------
// <copyright file = "NestedWheels.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet.MechanicalReels;

    /// <summary>
    /// Specific mechanical reel implementation for the triple nested wheel device.
    /// </summary>
    public class NestedWheels : MechanicalReelDevice
    {
        /// <summary>
        /// Constructor taking a <see cref="ReelFeatureDescription"/> argument.
        /// </summary>
        /// <param name="description">The <see cref="ReelFeatureDescription"/> of this reel device.</param>
        public NestedWheels(ReelFeatureDescription description) : base(description)
        {
        }

        /// <summary>
        /// Set the order wheels should stop.
        /// </summary>
        /// <param name="order">The wheels stop order, as an enumeration.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetStopOrder(ReelStopOrder order)
        {
            var indices = new List<byte>();

            // The actual index stop orders are specified. For no stop order, send no indexes.
            switch(order)
            {
                case ReelStopOrder.Ascending:
                    indices = Enumerable.Range(0, ReelCount).Select(reelIndex => (byte)reelIndex).ToList();
                    break;
                case ReelStopOrder.Descending:
                    indices = Enumerable.Range(0, ReelCount).Select(reelIndex => (byte)reelIndex).Reverse().ToList();
                    break;
                case ReelStopOrder.Off:
                    break;
            }

            var result = SetStopOrder(indices);

            return result;
        }

        /// <summary>
        /// Set the order wheels should stop.
        /// </summary>
        /// <param name="wheels">A list of wheels indexes indicating the order wheels should stop.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetStopOrder(ICollection<byte> wheels)
        {
            return MechanicalReels.SetStopOrder(Description.FeatureId, wheels);
        }

        /// <summary>
        /// Spin all wheels using individual spin profiles.
        /// </summary>
        /// <param name="spinProfiles">The spin profiles for each wheel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult Spin(ICollection<SpinProfile> spinProfiles)
        {
            return MechanicalReels.Spin(Description.FeatureId, spinProfiles);
        }

        /// <summary>
        /// Stop all wheels.
        /// </summary>
        /// <param name="wheelStops">Specific stop indexes for each wheel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult Stop(ICollection<ReelStop> wheelStops)
        {
            return MechanicalReels.Stop(Description.FeatureId, wheelStops);
        }

        /// <summary>
        /// This sets the recover-from-tilt stop order.
        /// </summary>
        /// <param name="spunStopOrder">The <see cref="ReelStopOrder"/> stop indexes for each wheel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public virtual ReelCommandResult SetRecoveryBehavior(ReelStopOrder spunStopOrder)
        {
            RecoveryOrder recoveryOrder;

            switch(spunStopOrder)
            {
                case ReelStopOrder.Ascending:
                case ReelStopOrder.Off:
                    recoveryOrder = RecoveryOrder.Ascending;
                    break;
                case ReelStopOrder.Descending:
                    recoveryOrder = RecoveryOrder.Descending;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spunStopOrder), spunStopOrder, null);
            }

            return MechanicalReels.SetRecoveryBehavior(Description.FeatureId, recoveryOrder, ReelDirection.Descending);
        }
    }
}
