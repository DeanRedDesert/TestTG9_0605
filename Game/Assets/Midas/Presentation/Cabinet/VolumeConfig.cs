using System;
using System.Collections.Generic;
using System.Linq;

namespace Midas.Presentation.Cabinet
{
	/// <summary>
	/// Holds volume configuration items
	/// </summary>
	public sealed class VolumeConfig : IEquatable<VolumeConfig>
	{
		private static readonly float[] defaultVolumeSteps = { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };

		private readonly float[] volumeStepValues = defaultVolumeSteps;

		/// <summary>
		///     The current level of the "Volume Player Level" option.
		///     Determines level at which a volume control should be shown in-game to the player.
		///     1.0 is the maximum volume reference, 0.5 is half loudness, 0 is lowest volume.
		/// </summary>
		public float CurrentVolume { get; } = 0.5f;

		/// <summary>
		///     Gets the current volume step. The step is calculated because of <see cref="VolumeStepValues" />
		/// </summary>
		public int CurrentVolumeStep { get; } = 2;

		/// <summary>
		///     The last setting of player selected mute.
		///     This item will be true if the last game chose to set player selectable mute.
		///     This item has higher priority than VolumePlayerLevel.
		/// </summary>
		public bool IsMuteSelected { get; } = false;

		/// <summary>
		///     If everything should be muted (can be configured from platform)
		/// </summary>
		public bool IsMuteAll { get; } = false;

		/// <summary>
		///     If the player can modify the current volume
		/// </summary>
		public bool IsPlayerVolumeControlAllowed { get; } = true;

		/// <summary>
		///     If the player can mute the volume
		/// </summary>
		public bool IsPlayerMuteControlAllowed { get; } = true;

		/// <summary>
		///     Gets the default level of the "Volume Player Level" option.
		/// </summary>
		public float DefaultVolume { get; } = 0.5f;

		/// <summary>
		///     Get the list of volume steps. The list defines the volume of each level
		/// </summary>
		public IReadOnlyList<float> VolumeStepValues => volumeStepValues;

		public VolumeConfig() { }

		/// <summary>
		///     <param name="level">The current volume level.</param>
		///     <param name="muteAll">The mute statue.</param>
		///     <param name="defaultVolume">The volume level to which to reset e.g player session reset.</param>
		/// </summary>
		public VolumeConfig(float level, bool muteSelected,
			bool isPlayerVolumeControlAllowed, bool isPlayerMuteControlAllowed,
			float defaultVolume, IReadOnlyList<float> volumeStepValues,
			bool muteAll)
		{
			CurrentVolume = level;
			IsMuteAll = muteAll;
			IsMuteSelected = muteSelected;
			IsPlayerVolumeControlAllowed = isPlayerVolumeControlAllowed;
			IsPlayerMuteControlAllowed = isPlayerMuteControlAllowed;
			DefaultVolume = defaultVolume;
			this.volumeStepValues = volumeStepValues != null ? volumeStepValues.ToArray() : defaultVolumeSteps;
			CurrentVolumeStep = CalculateCurrentLevel();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (volumeStepValues != null ? volumeStepValues.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ CurrentVolume.GetHashCode();
				hashCode = (hashCode * 397) ^ CurrentVolumeStep;
				hashCode = (hashCode * 397) ^ IsMuteSelected.GetHashCode();
				hashCode = (hashCode * 397) ^ IsMuteAll.GetHashCode();
				hashCode = (hashCode * 397) ^ IsPlayerVolumeControlAllowed.GetHashCode();
				hashCode = (hashCode * 397) ^ IsPlayerMuteControlAllowed.GetHashCode();
				hashCode = (hashCode * 397) ^ DefaultVolume.GetHashCode();
				return hashCode;
			}
		}

		public override string ToString()
		{
			return $"V:{CurrentVolume}, S:{CurrentVolumeStep}, M:{IsMuteSelected}, MA:{IsMuteAll}, " +
				$"PV:{IsPlayerVolumeControlAllowed}, PV:{IsPlayerMuteControlAllowed}, DV:{DefaultVolume}";
		}

		/// <summary>
		///     Change the volume steps of the volume config
		/// </summary>
		/// <param name="volumeStepValues"></param>
		/// <returns>A new created volume config</returns>
		public VolumeConfig WithStepValues(float[] volumeStepValues)
		{
			return new VolumeConfig(CurrentVolume, IsMuteSelected,
				IsPlayerVolumeControlAllowed, IsPlayerMuteControlAllowed,
				DefaultVolume, volumeStepValues,
				IsMuteAll);
		}

		public VolumeConfig WithStep(int step)
		{
			return WithVolumeAndMute(volumeStepValues[step], step == 0);
		}

		public VolumeConfig WithVolumeAndMute(float volume, bool muteSelected)
		{
			return new VolumeConfig(volume, muteSelected,
				IsPlayerVolumeControlAllowed, IsPlayerMuteControlAllowed,
				DefaultVolume, VolumeStepValues,
				IsMuteAll);
		}

		private int CalculateCurrentLevel()
		{
			float currentVolume = CurrentVolume;
			for (var i = 0; i < volumeStepValues.Length; i++)
			{
				if (currentVolume <= volumeStepValues[i])
				{
					return i;
				}
			}

			return volumeStepValues.Length - 1;
		}

		public bool Equals(VolumeConfig other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(volumeStepValues, other.volumeStepValues) &&
				CurrentVolume.Equals(other.CurrentVolume) &&
				CurrentVolumeStep == other.CurrentVolumeStep &&
				IsMuteSelected == other.IsMuteSelected &&
				IsMuteAll == other.IsMuteAll &&
				IsPlayerVolumeControlAllowed == other.IsPlayerVolumeControlAllowed &&
				IsPlayerMuteControlAllowed == other.IsPlayerMuteControlAllowed &&
				DefaultVolume.Equals(other.DefaultVolume);
		}

		public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is VolumeConfig other && Equals(other);
	}
}