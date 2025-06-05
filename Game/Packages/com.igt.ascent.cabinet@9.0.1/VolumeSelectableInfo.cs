// -----------------------------------------------------------------------
// <copyright file = "VolumeSelectableInfo.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The Player Selectable status for volume and mute.
    /// </summary>
    public struct VolumeSelectableInfo
    {
        /// <summary>
        /// The current status of the "Volume Player Selectable" option.
        /// Determines whether or not a volume control should be shown in-game to the player.
        /// </summary>
        public bool PlayerVolumeSelectable;

        /// <summary>
        /// The current status of the "Mute Player Selectable" option.
        /// Determines whether or not a volume control should allow muting.
        /// Foundation restricts that this item can not be true if "PlayerVolumeSelectable" is false.
        /// </summary>
        public bool PlayerMuteSelectable;

        /// <summary>
        /// The constructor for the <see cref="VolumeSelectableInfo"/>.
        /// </summary>
        /// <param name="volumeSelectable">Volume selectable status.</param>
        /// <param name="muteSelectable">Mutable status.</param>
        public VolumeSelectableInfo(bool volumeSelectable, bool muteSelectable)
        {
            PlayerVolumeSelectable = volumeSelectable;
            PlayerMuteSelectable = muteSelectable;
        }
    }
}