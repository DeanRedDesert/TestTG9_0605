//-----------------------------------------------------------------------
// <copyright file = "SoundVolumePlayerSelectableStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating that the sound volume player selectable status has changed.
    /// </summary>
    public class SoundVolumePlayerSelectableStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The sound volume player selectable status.
        /// </summary>
        public bool SoundVolumePlayerSelectable { get; }

        /// <summary>
        /// A boolean flat that indicates if the player mute option is selectable or not.
        /// </summary>
        public bool SoundVolumePlayerMuteSelectable { get; }

        /// <summary>
        /// Construct an instance with the given sound volume player selectable status.
        /// </summary>
        /// <param name="volumePlayerSelectable">The new volume player selectable status.</param>
        /// <param name="volumePlayerMuteSelectable">The volume player mute selectable status.</param>
        public SoundVolumePlayerSelectableStatusChangedEventArgs(bool volumePlayerSelectable, bool volumePlayerMuteSelectable)
        {
            SoundVolumePlayerSelectable = volumePlayerSelectable;
            SoundVolumePlayerMuteSelectable = volumePlayerMuteSelectable;
        }
    }
}
