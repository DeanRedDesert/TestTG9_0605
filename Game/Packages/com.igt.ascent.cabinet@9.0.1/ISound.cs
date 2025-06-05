//-----------------------------------------------------------------------
// <copyright file = "ISound.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Interface for sound related functionality of the cabinet.
    /// </summary>
    public interface ISound
    {
        /// <summary>
        /// Enable/disable the sound chair power.
        /// </summary>
        /// <param name="enable">Flag indicating if the chair power should be enabled.</param>
        /// <returns>True if the change was applied.</returns>
        bool EnableSoundChairPower(bool enable);

        /// <summary>
        /// Check if the sound chair is powered.
        /// </summary>
        /// <returns>True if the chair is powered.</returns>
        bool IsSoundChairPowered();

        /// <summary>
        /// Get the volume of the specified sound group.
        /// </summary>
        /// <param name="soundGroup">The sound group to get the volume for.</param>
        /// <returns>
        /// The level of sound attenuation between 0 and 10000. 0 indicates no attenuation and 10000 represent maximum attenuation (mute).
        /// </returns>
        uint GetVolume(GroupName soundGroup);

        /// <summary>
        /// Get the volume of each sound group.
        /// </summary>
        /// <returns>
        /// The level of sound attenuations for each sound group.
        /// </returns>
        IEnumerable<GroupVolumeSetting> GetVolumeAll();

        /// <summary>
        /// Check if audio is muted.
        /// </summary>
        /// <returns>The Mute all status.</returns>
        bool IsMuteAll();

        /// <summary>
        /// Gets the <see cref="VolumeSelectableInfo" /> from the Foundation.
        /// </summary>
        /// <returns>The <see cref="VolumeSelectableInfo" /> which contains Player Volume Selectable and Player Mute Selectable.</returns>
        VolumeSelectableInfo GetVolumePlayerSelectableInfo();

        /// <summary>
        /// Gets the <see cref="PlayerVolumeSettings"/> from the Foundation.
        /// </summary>
        /// <returns>The <see cref="PlayerVolumeSettings"/>.</returns>
        PlayerVolumeSettings GetPlayerVolumeSettings();

        /// <summary>
        /// Sets the <see cref="PlayerVolumeInfo"/> to Foundation.
        /// </summary>
        /// <param name="volumeInfo"> The player volume info.</param>
        /// <returns>True if set succeeded.</returns>
        bool SetPlayerVolumeInfo(PlayerVolumeInfo volumeInfo);

        /// <summary>
        /// Send a request to mute (or unmute) the sound of CSI Clients matching the supplied priority value.
        /// </summary>
        /// <param name="muteStates">
        /// Collection of priority values and their requested mute states.
        /// </param>
        /// <returns>
        /// Collection of priority values and their mute states after the foundation has processed the mute state change request.
        /// </returns>
        IEnumerable<PriorityMuteState> SetMuteStates(IEnumerable<PriorityMuteState> muteStates);

        /// <summary>
        /// Returns a list of audio endpoints present on the EGM.
        /// </summary>
        /// <returns>
        /// A list of audio endpoints present on the EGM.
        /// </returns>
        IEnumerable<string> GetAudioEndpoints();

        /// <summary>
        /// Sets the default audio endpoint for the EGM.
        /// </summary>
        /// <param name="endpoint">
        /// Name of the audio endpoint to use as the default audio endpoint.
        /// </param>
        /// <returns>
        /// True if the endpoint specified is now the default audio endpoint.
        /// </returns>
        bool SetDefaultAudioEndpoint(string endpoint);
    }
}
