using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public abstract class FeatureMusicPlayer : MonoBehaviour
	{
		protected enum WinInteractionStyle
		{
			None,
			MuteBackgroundOnly,
			UseWinTune,
			FullInteraction
		}

		public static IReadOnlyList<FeatureMusicPlayer> Instances { get; private set; }

		public static FeatureMusicPlayer GetFirstActiveInstance()
		{
			Instances ??= FindObjectsOfType<FeatureMusicPlayer>(true);
			return Instances.FirstOrDefault(fmp => fmp.IsEligibleForWinInteractivity());
		}

		[SerializeField]
		private WinInteractionStyle winInteractionStyle = WinInteractionStyle.FullInteraction;

		public bool AllowsMutingBackgroundMusicDuringWins => AllowsFullInteraction || winInteractionStyle == WinInteractionStyle.MuteBackgroundOnly;
		public bool AllowsInteractionDuringWins => AllowsFullInteraction || winInteractionStyle == WinInteractionStyle.UseWinTune;
		public bool AllowsFullInteraction => winInteractionStyle == WinInteractionStyle.FullInteraction;

		public abstract bool IsEligibleForWinInteractivity();
		public abstract void Play();
		public abstract void Stop();
		public abstract void StartWin();
		public abstract void StopWin();
		public abstract void MuteBackgroundMusic();
		public abstract void UnmuteBackgroundMusic();
		public abstract void ImmediateStop();
	}
}