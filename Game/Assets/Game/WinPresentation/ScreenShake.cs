using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Sequencing;
using Midas.Presentation.Tween;
using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Game.WinPresentation
{
	public sealed class ScreenShake : DoTweenAnimation
	{
		#region Public

		[Tween]
		public void StartShake(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			ShakeScreen(tweenSequence);
		}

		#endregion

		#region Private

		private void ShakeScreen(Sequence tweenSequence)
		{
			foreach (var item in particlesSystemsToStartList)
			{
				item.Play();
			}

			CreateShakeSequence(tweenSequence);
		}

		private void CreateShakeSequence(Sequence tweenSequence)
		{
			foreach (var objectTransform in objectsToShakeList)
			{
				tweenSequence.Join(objectTransform.DOShakePosition(shakeTime, shakePosition, vibrato, randomness, false, false));
				tweenSequence.Join(objectTransform.DOShakeRotation(shakeTime, shakeRotation, vibrato, randomness, false));
			}

			tweenSequence.onComplete += ShakeComplete;
		}

		private void ShakeComplete()
		{
			foreach (var item in particlesSystemsToStartList)
			{
				item.Stop();
			}
		}

		[SerializeField]
		private float shakeTime = 3;

		[SerializeField]
		private Vector3 shakePosition = new Vector3(10, 10, 0);

		[SerializeField]
		private Vector3 shakeRotation = new Vector3(0, 0, 1);

		[SerializeField]
		private int vibrato = 30;

		[SerializeField]
		private float randomness = 90;

		[SerializeField]
		private List<Transform> objectsToShakeList = new List<Transform>();

		[SerializeField]
		private List<ParticleSystem> particlesSystemsToStartList = new List<ParticleSystem>();

		#endregion

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(IReadOnlyList<Transform> transforms)
		{
			objectsToShakeList = transforms.ToList();
		}

#endif

		#endregion
	}
}