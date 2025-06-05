//-----------------------------------------------------------------------
// <copyright file = "ReelDescription.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Class containing information about a mechanical reel.
    /// </summary>
    public class ReelDescription
    {
        /// <summary>
        /// The number of stops the reel has.
        /// </summary>
        public byte NumberOfStops { get; }

        /// <summary>
        /// The maximum time, in seconds, the motor will take to accelerate, find the desired position,
        /// and decelerate to a stop if issued a spin command with no "extra" movement.
        /// </summary>
        public byte MaximumSeekTime { get; }

        /// <summary>
        /// Construct a reel description.
        /// </summary>
        /// <param name="numberOfStops">Number of stops in the reel.</param>
        /// <param name="maximumSeekTime">Maximum time for the reel to seek.</param>
        public ReelDescription(byte numberOfStops, byte maximumSeekTime)
        {
            NumberOfStops = numberOfStops;
            MaximumSeekTime = maximumSeekTime;
        }
    }
}
