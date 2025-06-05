using UnityEngine;

namespace Midas.Presentation.Reels
{
	public sealed class SpinSettings
	{
		public float SpinSpeed { get; }
		public float WindupDistance { get; }
		public float WindupTime { get; }
		public float OvershootDistance { get; }
		public float RecoveryTime { get; }
		public int SpinFrameRate { get; }

		public SpinSettings(float spinSpeed, float windupDistance, float windupTime, float overshootDistance, float recoveryTime, int spinFrameRate)
		{
			SpinSpeed = spinSpeed;
			WindupDistance = windupDistance;
			WindupTime = windupTime;
			OvershootDistance = overshootDistance;
			RecoveryTime = recoveryTime;
			SpinFrameRate = spinFrameRate;
		}

		public float GetWindupPosition(float t)
		{
			return Mathf.Lerp(0f, WindupDistance, t);
		}

		public float GetRecoveryPosition(float t)
		{
			return Mathf.Lerp(OvershootDistance, 0f, t);
		}
	}
}