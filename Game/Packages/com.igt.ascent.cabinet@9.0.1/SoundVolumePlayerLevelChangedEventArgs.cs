//-----------------------------------------------------------------------
// <copyright file = "SoundVolumePlayerLevelChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating that the sound volume player level has changed.
    /// </summary>
    public class SoundVolumePlayerLevelChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The current level of the "Volume Player Level" option.
        /// Determines the level at which a volume control should be shown in-game to the player.
        /// 1.0 is the maximum volume reference, 0.5 is half loudness, 0 is lowest volume.
        /// </summary>
        public float VolumePlayerLevel { get; }

        /// <summary>
        /// Construct an instance with the given sound volume player level.
        /// </summary>
        /// <param name="volumePlayerLevel">The player volume level.</param>
        public SoundVolumePlayerLevelChangedEventArgs(float volumePlayerLevel)
        {
            VolumePlayerLevel = volumePlayerLevel;
        }
    }
}
