using Midas.Core;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	[DefaultExecutionOrder(1000)]
	public sealed class UnityTimingsMeasurementHelper : MonoBehaviour
	{
		#region Private

		private void Update()
		{
			GamePresentationTimings.UnityUpdateTime.Stop();
			GamePresentationTimings.OverallUnityTime.Lap();
			GamePresentationTimings.OverallUpdateTime.Stop();
			GamePresentationTimings.AnimatorAndCoRoutineUpdateTime.Start();
		}

		private void LateUpdate()
		{
			GamePresentationTimings.UnityLateUpdateTime.Stop();
			GamePresentationTimings.OverallUnityTime.Stop();
			GamePresentationTimings.OverallLateUpdateTime.Stop();
			GamePresentationTimings.OverallTime.Stop();
		}

		#endregion
	}
}