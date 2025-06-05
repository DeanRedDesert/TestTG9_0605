//-----------------------------------------------------------------------
// <copyright file = "SoundVolumeChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that the sound volume has changed.
    /// </summary>
    public class SoundVolumeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The sound group volume setting.
        /// </summary>
        /// <remarks>
        /// The volume level provided by this setting is an attenuation level between 0 and 10000.
        /// Use <see cref="GetVolume"/> method to get the corresponding volume scale between 0 and 1.
        /// </remarks>
        public GroupVolumeSetting GroupVolumeSetting { get; }

        ///<summary>
        /// A value representing the maximum volume attenuation level.
        ///</summary>
        private const float MaxVolumeAttenuationLevel = 10000.0f;

        /// <summary>
        /// A value representing the maximum volume scale.
        /// </summary>
        private const float MaxVolumeScale = 1.0f;

        /// <summary>
        /// Construct an instance with the given volume setting.
        /// </summary>
        /// <param name="soundGroupVolumeSetting">The new sound group volume setting.</param>
        public SoundVolumeChangedEventArgs(GroupVolumeSetting soundGroupVolumeSetting)
        {
            GroupVolumeSetting = soundGroupVolumeSetting;
        }

        /// <summary>
        /// Gets the converted volume scale value based on
        /// the data in <see cref="GroupVolumeSetting"/>.
        /// </summary>
        /// <returns>The converted volume scale.</returns>
        public float GetVolume()
        {
            return MaxVolumeScale - GroupVolumeSetting.VolumeLevel / MaxVolumeAttenuationLevel;
        }
    }
}
