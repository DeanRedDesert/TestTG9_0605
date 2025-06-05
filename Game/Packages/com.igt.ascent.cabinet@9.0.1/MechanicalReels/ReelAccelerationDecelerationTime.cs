//-----------------------------------------------------------------------
// <copyright file = "ReelAccelerationDecelerationTime.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Class which encapsulates a single profile of a reel's acceleration and deceleration times
    /// between two adjacent speed indexes.
    /// </summary>
    public class ReelAccelerationDecelerationTime
    {
        /// <summary>
        /// The time in milliseconds that a reel takes to accelerate from one speed index to the next. Note that
        /// higher speed indexes mean a slower reel spin speed.
        /// </summary>
        public ushort AccelerationTimeToNextSpeed { get; set; }

        /// <summary>
        /// The time in milliseconds that a reel takes to deceleration from one speed index to the next. Note that
        /// lower speed indexes mean a faster reel spin speed.
        /// </summary>
        public ushort DecelerationTimeToPreviousSpeed { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"Acceleration Time To Next Speed: {AccelerationTimeToNextSpeed} Deceleration Time To Previous Speed: {DecelerationTimeToPreviousSpeed}";
        }
    }
}
