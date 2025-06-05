using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Presentation;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Game.Stages.Common
{
	public sealed class SlideAndHide : MonoBehaviour
	{
		private enum InitialState
		{
			Visible,
			Hidden
		}

		private enum State
		{
			Visible,
			Hidden,
			Showing,
			Hiding
		}

		private Vector3? inPosition;
		private Coroutine coroutine;
		private State state;
		private float t;

		[SerializeField]
		private GameObject objectToAnimate;

		[SerializeField]
		private Vector3 outPosition;

		[SerializeField]
		private float time;

		[SerializeField]
		private InitialState initialState;

		private void OnEnable()
		{
			RefreshInPosition();
			if (isActiveAndEnabled)
				coroutine = FrameUpdateService.Update.StartCoroutine(SlideAndHideCoroutine());
		}

		private void OnDisable()
		{
			coroutine?.Stop();
			coroutine = null;
		}

		public void Hide(bool immediate)
		{
			if (state == State.Hidden)
				return;

			RefreshInPosition();

			if (immediate)
			{
				objectToAnimate.transform.localPosition = outPosition;
				objectToAnimate.SetActive(false);
				state = State.Hidden;
			}
			else if (state != State.Hiding)
			{
				objectToAnimate.transform.localPosition = inPosition!.Value;
				objectToAnimate.SetActive(true);
				t = 0;
				state = State.Hiding;
			}
		}

		public void Show(bool immediate)
		{
			if (state == State.Visible)
				return;

			RefreshInPosition();

			objectToAnimate.SetActive(true);

			if (immediate)
			{
				objectToAnimate.transform.localPosition = inPosition!.Value;
				state = State.Visible;
			}
			else if (state != State.Showing)
			{
				objectToAnimate.transform.localPosition = outPosition;
				t = 0;
				state = State.Showing;
			}
		}

		private void RefreshInPosition()
		{
			if (inPosition.HasValue)
				objectToAnimate.transform.localPosition = inPosition.Value;
			else
			{
				inPosition = objectToAnimate.transform.localPosition;
				switch (initialState)
				{
					case InitialState.Visible:
						state = State.Visible;
						break;
					case InitialState.Hidden:
						state = State.Hidden;
						objectToAnimate.transform.localPosition = outPosition;
						objectToAnimate.SetActive(false);
						break;
				}
			}
		}

		[SuppressMessage("ReSharper", "PossibleInvalidOperationException", Justification = "inPosition cannot be null if this coroutine is running")]
		[SuppressMessage("ReSharper", "IteratorNeverReturns", Justification = "This is deliberate")]
		private IEnumerator<CoroutineInstruction> SlideAndHideCoroutine()
		{
			while (true)
			{
				switch (state)
				{
					case State.Showing:
						DoSmoothLerp(outPosition, inPosition.Value);
						break;

					case State.Hiding:
						DoSmoothLerp(inPosition.Value, outPosition);
						break;
				}

				yield return null;
			}

			void DoSmoothLerp(Vector3 from, Vector3 to)
			{
				var smoothStepTime = Mathf.SmoothStep(0, 1, t / time);
				if (smoothStepTime >= 1)
				{
					objectToAnimate.transform.localPosition = to;
					if (state == State.Hiding)
					{
						objectToAnimate.SetActive(false);
						state = State.Hidden;
					}
					else
						state = State.Visible;
				}
				else
				{
					objectToAnimate.transform.localPosition = Vector3.Lerp(from, to, smoothStepTime);
					t += (float)FrameTime.DeltaTime.TotalSeconds;
				}
			}
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(GameObject o, Vector3 outPos, float animTime)
		{
			objectToAnimate = o;
			outPosition = outPos;
			time = animTime;
		}

#endif

		#endregion
	}
}