// -----------------------------------------------------------------------
// <copyright file = "PlayerVolumeInfo.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The player volume level and mute.
    /// </summary>
    public struct PlayerVolumeInfo
    {
        /// <summary>
        /// The current level of the "Volume Player Level" option.
        /// Determines level at which a volume control should be shown in-game to the player.
        /// 1.0 is the maximum volume reference, 0.5 is half loudness, 0 is lowest volume.
        /// </summary>
        public float VolumePlayerLevel;

        /// <summary>
        /// The last setting of player selected mute. 
        /// This item will be true if the last game chose to set player selectable mute.
        /// This item has higher priority than VolumePlayerLevel.
        /// </summary>
        public bool PlayerMuteSelected;

        /// <summary>
        /// The constructor for the <see cref="PlayerVolumeInfo"/>.
        /// </summary>
        /// <param name="level">The volume level.</param>
        /// <param name="mute">The mute statue.</param>
        public PlayerVolumeInfo(float level, bool mute)
        {
            VolumePlayerLevel = level;
            PlayerMuteSelected = mute;
        }
    }
}