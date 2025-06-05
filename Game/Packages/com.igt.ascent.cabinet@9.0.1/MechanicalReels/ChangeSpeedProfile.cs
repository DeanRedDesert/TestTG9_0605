//-----------------------------------------------------------------------
// <copyright file = "ChangeSpeedProfile.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Class which contains change speed/direction profile meta data.
    /// </summary>
    [Serializable]
    public class ChangeSpeedProfile
    {
        /// <summary>
        /// The 0-based reel index to apply the speed change profile information to.
        /// </summary>
        public byte Number { get; set; }

        /// <summary>
        /// The speed index to change to. This value must be one of the speed indexes returned from querying the reel device during
        /// initialization; usually these indexes are numbered from 0-N, depending on the device, and the lower indexes are the faster speeds.
        /// </summary>
        public ushort SpeedIndex { get; set; }

        /// <summary>
        /// The requested amount of time the reel should take to change to the target speed, in milliseconds.
        /// </summary>
        public ushort Period { get; set; }

        /// <summary>
        /// The direction to set. Note that "Shortest" is not allowed.
        /// </summary> 
        public ReelDirection Direction { get; set; }

        /// <summary>
        /// A flag indicating whether this command should be executed immediately.
        /// </summary> 
        public bool Immediate { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"Number ({Number}) / Speed ({SpeedIndex}) / Period ({Period}) / Direction ({Direction}) / Immediate ({Immediate})";
        }
    }
}
