// -----------------------------------------------------------------------
// <copyright file = "PlayerVolumeSettings.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The player volume settings.
    /// </summary>
    public struct PlayerVolumeSettings
    {
        /// <summary>
        /// Gets the player volume info.
        /// </summary>
        public PlayerVolumeInfo PlayerVolumeInfo { get; }

        /// <summary>
        /// Gets the default level of the "Volume Player Level" option.
        /// </summary>
        public float? DefaultVolumePlayerLevel { get; }

        /// <summary>
        /// The constructor for the <see cref="PlayerVolumeSettings"/>.
        /// </summary>
        /// <param name="level">The volume level.</param>
        /// <param name="mute">The mute status.</param>
        /// <param name="defaultLevel">The default volume level.</param>
        public PlayerVolumeSettings(float level, bool mute, float? defaultLevel): this()
        {
            PlayerVolumeInfo = new PlayerVolumeInfo(level, mute);
            DefaultVolumePlayerLevel = defaultLevel;
        }
    }
}