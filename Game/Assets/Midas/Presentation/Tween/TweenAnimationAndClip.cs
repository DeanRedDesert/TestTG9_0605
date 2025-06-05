using System;
using UnityEngine;

namespace Midas.Presentation.Tween
{
	[Serializable]
	public struct TweenAnimationAndClip
	{
		#region Public

		public TweenAnimation TweenAnimation
		{
			get => tweenAnimation;
			set => tweenAnimation = value;
		}

		public string ClipName
		{
			get => clipName;
			set => clipName = value;
		}

		#endregion

		#region Private

		[SerializeField]
		private TweenAnimation tweenAnimation;

		[SerializeField]
		private string clipName;

		#endregion
	}
}