using System;
using DG.Tweening;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Audio;
using Midas.Presentation.Data;
using Midas.Presentation.SceneManagement;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using Midas.Presentation.Tween;
using TMPro;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffAnimations : DoTweenAnimation, ISceneInitialiser
	{
		private Vector3 initialPosition;
		private Vector3 initialScale;
		private Vector3 winRaysInitialScale;
		private TextMeshPro returnButtonTextMesh;
		private TextMeshPro winTextMesh;
		private TextMeshPro playButtonTextMesh;
		private TextMeshPro[] extraSpinTextMeshes;

		[Header("Transform Attributes")]
		[SerializeField]
		private float moveToWaitDuration = 1.96f;

		[SerializeField]
		private float extraSpinTextAppearTime = 1.16f;

		[SerializeField]
		private float playButtonAppearTime = 1.56f;

		[SerializeField]
		private float innerCircleSpriteAppearTime = 0.33f;

		[SerializeField]
		private float innerCircleSpriteAppearAlphaValue = 1f;

		[SerializeField]
		private float logoSpriteAppearTime = 0.76f; //this is also where to play the play button appear sound

		[SerializeField]
		private float zoomedUpScale = 7;

		[SerializeField]
		private Vector3 zoomedUpPosition = new Vector3(30, 115, 0);

		[SerializeField]
		private Vector3 moveOutPosition = new Vector3(30, -60, 0);

		[SerializeField]
		private float moveTime = 1.0f;

		private readonly float moveBackToActiveFadeOutTime = 0.016f;

		[SerializeField]
		private float zoomedUpPointerTime = 2.25f;

		[SerializeField]
		private float moveOutTime = 1;

		[SerializeField]
		private float dimLayerAlphaValue = 0.6f;

		[SerializeField]
		private float pointerAppearStartAngle = 180.0f;

		[SerializeField]
		private float spinningDuration = 4.0f;

		[SerializeField]
		private int fullRotations = 5;

		[SerializeField]
		private float textSpinFadeOutTime = 0.25f;

		[SerializeField]
		private float winLosObjectsFadeTime = 0.5f;

		[SerializeField]
		private float timeBeforeMoveOut = 0.5f;

		[Header("Objects")]
		[SerializeField]
		private GameObject animationRootObject;

		[SerializeField]
		private GameObject pointerRotationObject;

		[SerializeField]
		private GameObject winParticles;

		[SerializeField]
		private GameObject dialGlowSprite;

		[SerializeField]
		private GameObject innerCircleSprite;

		[SerializeField]
		private GameObject logoSprite;

		[SerializeField]
		private GameObject extraSpinText;

		[SerializeField]
		private GameObject lossGlowGameObject;

		[SerializeField]
		private GameObject playButtonImageSprite;

		[SerializeField]
		private ParticleSystem playButtonSparkles;

		[SerializeField]
		private GameObject playButtonText;

		[SerializeField]
		private GameObject pointerNormal;

		[SerializeField]
		private GameObject pointerWin;

		[SerializeField]
		private GameObject pointerLose;

		[SerializeField]
		private GameObject returnButton;

		[SerializeField]
		private GameObject returnButtonText;

		[SerializeField]
		private GameObject winText;

		[SerializeField]
		private GameObject winRaysSprite;

		[SerializeField]
		private GameObject dimLayer;

		[SerializeField]
		private GameObject playButton;

		[SerializeField]
		private GameObject startPlayoffButton;

		[SerializeField]
		private CreditPlayoffDialController dialController;

		[SerializeField]
		private SoundPlayer appearSound;

		[SerializeField]
		private float appearSoundTime = 0.76f;

		[SerializeField]
		private SoundPlayer wheelAppear;

		[SerializeField]
		private SoundPlayer wheelHide;

		[SerializeField]
		private SoundPlayer playButtonPressed;

		[SerializeField]
		private SoundPlayer spinning;

		[SerializeField]
		private SoundPlayer win;

		[SerializeField]
		private SoundPlayer loss;

		#region Public

		[Tween]
		public void EnableAtActivePosition(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			SetToAvailablePosition();
		}

		[Tween]
		public void MoveToWaitPlayPosition(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			//total time seen in TreasureIsland 1:58sec = 1.96sec
			dimLayer.SetActive(true);
			startPlayoffButton.SetActive(false);
			tweenSequence.Insert(0.0f, animationRootObject.transform.DOLocalMove(zoomedUpPosition, moveTime).SetEase(Ease.OutQuad));
			tweenSequence.Insert(0.0f, animationRootObject.transform.DOScale(new Vector3(zoomedUpScale, zoomedUpScale, zoomedUpScale), moveTime).SetEase(Ease.OutQuad));

			//starts at 1:10(_extraSpinTextAppearTime) and goes to 1:34(_playButtonAppearTime)
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				tweenSequence.Insert(extraSpinTextAppearTime,
					DOTween.ToAlpha(() => extraSpinTextMesh.color, value => extraSpinTextMesh.color = value, 1.0f, playButtonAppearTime - extraSpinTextAppearTime)
						.SetEase(Ease.InOutQuad));
			//starts at 0:20 and stops at 0:40
			tweenSequence.Insert(innerCircleSpriteAppearTime,
				innerCircleSprite.GetComponent<SpriteRenderer>().DOFade(innerCircleSpriteAppearAlphaValue, moveTime).SetEase(Ease.Linear));
			//Logo at 0:46 and stops at 1:10(_extraSpinTextAppearTime)
			tweenSequence.Insert(logoSpriteAppearTime,
				logoSprite.GetComponent<SpriteRenderer>().DOFade(1.0f, extraSpinTextAppearTime - logoSpriteAppearTime).SetEase(Ease.Linear));

			//play button appear a 1:34sec(_playButtonAppearTime) and goes to then end = 1.56sec
			tweenSequence.Insert(playButtonAppearTime, playButtonImageSprite.GetComponent<SpriteRenderer>()
				.DOFade(1.0f, moveToWaitDuration - playButtonAppearTime)
				.SetEase(Ease.Linear)
				.OnStart(() =>
				{
					returnButton.SetActive(true);
					playButton.SetActive(true);
					playButtonSparkles.Play();
				})
				.OnKill(() =>
				{
					returnButton.SetActive(true);
					playButton.SetActive(true);
					playButtonSparkles.Play();
				}));

			tweenSequence.Insert(playButtonAppearTime,
				DOTween.ToAlpha(() => playButtonTextMesh.color, value => playButtonTextMesh.color = value, 1.0f, moveToWaitDuration - playButtonAppearTime)
					.SetEase(Ease.InOutQuad));

			tweenSequence.Insert(0.0f, pointerRotationObject.transform.DOLocalRotate(Vector3.zero, zoomedUpPointerTime, RotateMode.FastBeyond360).SetEase(Ease.InOutBack));

			//return button has same values as PlayButton
			tweenSequence.Insert(playButtonAppearTime,
				returnButton.GetComponent<SpriteRenderer>().DOFade(1.0f, moveToWaitDuration - playButtonAppearTime).SetEase(Ease.Linear));
			tweenSequence.Insert(playButtonAppearTime,
				DOTween.ToAlpha(() => returnButtonTextMesh.color, value => returnButtonTextMesh.color = value, 1.0f, moveToWaitDuration - playButtonAppearTime)
					.SetEase(Ease.InOutQuad));

			tweenSequence.Insert(0.0f, dimLayer.GetComponent<SpriteRenderer>().DOFade(0.5f, moveTime).SetEase(Ease.Linear));
			tweenSequence.Insert(0.0f, pointerNormal.GetComponent<SpriteRenderer>().DOFade(1.0f, moveTime).SetEase(Ease.Linear));

			tweenSequence.InsertCallback(appearSoundTime, () => { appearSound.Play(); });
			tweenSequence.InsertCallback(0, () => { wheelAppear.Play(); });
			tweenSequence.OnComplete(() =>
			{
				appearSound.Stop();
				wheelAppear.Stop();
			});

			onInterrupt = () =>
			{
				appearSound.Stop();
				wheelAppear.Stop();
				returnButton.SetActive(false);
				playButton.SetActive(false);
				playButtonSparkles.Stop();
			};
		}

		[Tween]
		public void MoveBackToActivePosition(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			tweenSequence.Insert(0.0f, animationRootObject.transform.DOLocalMove(initialPosition, moveTime).SetEase(Ease.InOutQuad));
			tweenSequence.Insert(0.0f, animationRootObject.transform.DOScale(initialScale, moveTime).SetEase(Ease.InOutQuad));
			tweenSequence.Insert(0.0f,
				pointerRotationObject.transform.DOLocalRotate(new Vector3(0, 0, pointerAppearStartAngle), moveBackToActiveFadeOutTime, RotateMode.FastBeyond360)
					.SetEase(Ease.InOutQuad));

			tweenSequence.Insert(0.0f, dimLayer.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime));
			tweenSequence.Insert(0.0f, innerCircleSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime));
			tweenSequence.Insert(0.0f, logoSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime));
			tweenSequence.Insert(0.0f, pointerNormal.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime));
			tweenSequence.Insert(0.0f, playButtonImageSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime));
			tweenSequence.Insert(0.0f, returnButton.GetComponent<SpriteRenderer>().DOFade(0.0f, moveBackToActiveFadeOutTime)
				.OnComplete(() =>
				{
					returnButton.SetActive(false);
					playButton.SetActive(false);
					startPlayoffButton.SetActive(true);
				}));

			playButtonSparkles.Stop();
			tweenSequence.Insert(0.0f,
				DOTween.ToAlpha(() => returnButtonTextMesh.color, value => returnButtonTextMesh.color = value, 0.0f, moveBackToActiveFadeOutTime).SetEase(Ease.InOutQuad));
			tweenSequence.Insert(0.0f,
				DOTween.ToAlpha(() => playButtonTextMesh.color, value => playButtonTextMesh.color = value, 0.0f, moveBackToActiveFadeOutTime).SetEase(Ease.InOutQuad));
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				tweenSequence.Insert(0.0f,
					DOTween.ToAlpha(() => extraSpinTextMesh.color, value => extraSpinTextMesh.color = value, 0.0f, moveBackToActiveFadeOutTime).SetEase(Ease.InOutQuad));

			tweenSequence.InsertCallback(0, () => { wheelHide.Play(); });
			tweenSequence.OnComplete(() => { wheelHide.Stop(); });

			onInterrupt = () =>
			{
				wheelHide.Stop();
				returnButton.SetActive(true);
				playButton.SetActive(true);
				startPlayoffButton.SetActive(false);
				playButtonSparkles.Play();
			};
		}

		[Tween]
		public void SpinToWin(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			//mind the OnComplete - is on its own sequence (OnComplete replaces all onComplete)
			tweenSequence.InsertCallback(0, () =>
			{
				spinning.Play();
				playButtonPressed.Play();
			});
			tweenSequence.Append(CreateSpinSeq(args, true).OnComplete(() =>
			{
				pointerNormal.SetActive(false);
				pointerLose.SetActive(false);
				pointerWin.SetActive(true);
				winParticles.SetActive(true);
				spinning.Stop();
				playButtonPressed.Stop();
			})).Append(PresentWin());

			onInterrupt = () =>
			{
				returnButton.SetActive(true);
				playButton.SetActive(true);
				playButtonSparkles.Play();
				pointerNormal.SetActive(true);
				pointerLose.SetActive(false);
				pointerWin.SetActive(false);
				winParticles.SetActive(false);
				spinning.Stop();
				playButtonPressed.Stop();
				win.Stop();
			};
		}

		[Tween]
		public void SpinToLose(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			//mind the OnComplete - is on its own sequence (OnComplete replaces all onComplete)
			tweenSequence.InsertCallback(0, () =>
			{
				spinning.Play();
				playButtonPressed.Play();
			});
			tweenSequence.Append(CreateSpinSeq(args, false).OnComplete(() =>
			{
				pointerNormal.SetActive(false);
				pointerLose.SetActive(true);
				pointerWin.SetActive(false);
				lossGlowGameObject.SetActive(true);
				spinning.Stop();
				playButtonPressed.Stop();
			})).Append(PresentLose());

			onInterrupt = () =>
			{
				returnButton.SetActive(true);
				playButton.SetActive(true);
				playButtonSparkles.Play();
				pointerNormal.SetActive(true);
				pointerLose.SetActive(false);
				pointerWin.SetActive(false);
				lossGlowGameObject.SetActive(false);
				spinning.Stop();
				playButtonPressed.Stop();
				loss.Stop();
			};
		}

		[Tween]
		public void MoveOutOfScreen(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt)
		{
			tweenSequence.Insert(0.0f, dimLayer.GetComponent<SpriteRenderer>().DOFade(dimLayerAlphaValue, timeBeforeMoveOut).SetEase(Ease.Linear));
			tweenSequence.Insert(timeBeforeMoveOut, animationRootObject.transform.DOLocalMove(moveOutPosition, moveOutTime).SetEase(Ease.OutQuad));
			tweenSequence.Insert(timeBeforeMoveOut, dimLayer.GetComponent<SpriteRenderer>().DOFade(0.0f, moveOutTime));

			tweenSequence.OnComplete(() =>
			{
				loss.Stop();
				win.Stop();
			});
		}

		[Tween]
		public void ResetAnimations(Sequence tweenSequence, SequenceEventArgs args, ref Action onInterrupt) => ResetGameObjects();

		private Sequence PresentWin()
		{
			var seq = DOTween.Sequence();

			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				seq.Insert(0.0f, DOTween.ToAlpha(() => extraSpinTextMesh.color, value => extraSpinTextMesh.color = value, 0.0f, textSpinFadeOutTime).SetEase(Ease.InOutQuad));
			seq.Insert(0.0f, logoSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, winLosObjectsFadeTime));
			seq.Insert(0.0f, dialGlowSprite.GetComponent<SpriteRenderer>().DOColor(new Color(0.0f, 1.0f, 0.0f, 1.0f), winLosObjectsFadeTime));
			seq.Insert(0.0f, winText.transform.DOScale(Vector3.one, winLosObjectsFadeTime));
			seq.Insert(0.0f, DOTween.ToAlpha(() => winTextMesh.color, value => winTextMesh.color = value, 1.0f, winLosObjectsFadeTime).SetEase(Ease.InOutQuad));

			seq.Insert(0.0f, winRaysSprite.GetComponent<SpriteRenderer>().DOFade(1.0f, winLosObjectsFadeTime / 2));
			seq.Insert(0.0f, winRaysSprite.transform.DOScale(winRaysInitialScale, winLosObjectsFadeTime / 2));
			seq.Insert(winLosObjectsFadeTime / 2, winRaysSprite.GetComponent<SpriteRenderer>().DOFade(1.0f, winLosObjectsFadeTime / 2));
			seq.Insert(winLosObjectsFadeTime / 2, winRaysSprite.transform.DOScale(Vector3.zero, winLosObjectsFadeTime / 2));
			seq.InsertCallback(0, () => { win.Play(); });

			return seq;
		}

		private Sequence PresentLose()
		{
			var seq = DOTween.Sequence();

			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				seq.Insert(0.0f, DOTween.ToAlpha(() => extraSpinTextMesh.color, value => extraSpinTextMesh.color = value, 0.0f, textSpinFadeOutTime).SetEase(Ease.InOutQuad));
			seq.Insert(0.0f, logoSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, textSpinFadeOutTime));
			seq.Insert(0.0f, dialGlowSprite.GetComponent<SpriteRenderer>().DOColor(new Color(1.0f, 0.0f, 0.0f, 1.0f), textSpinFadeOutTime));
			seq.Insert(0.0f, lossGlowGameObject.GetComponent<SpriteRenderer>().DOFade(1.0f, textSpinFadeOutTime));
			seq.InsertCallback(0, () => { loss.Play(); });

			return seq;
		}

		#endregion

		#region Private

		private void Awake()
		{
			returnButtonTextMesh = returnButtonText.GetComponent<TextMeshPro>();
			winTextMesh = winText.GetComponent<TextMeshPro>();

			playButtonTextMesh = playButtonText.GetComponent<TextMeshPro>();
			extraSpinTextMeshes = extraSpinText.GetComponentsInChildren<TextMeshPro>(true);

			initialPosition = animationRootObject.transform.localPosition;
			initialScale = animationRootObject.transform.localScale;
			winRaysInitialScale = winRaysSprite.transform.localScale;
			ResetGameObjects();
		}

		private float GetStopAngle(bool winningSpin)
		{
			var creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
			var stopPosition = creditPlayoffStatus.Result / (double)creditPlayoffStatus.TotalWeight;
			return dialController.GetStopPosition(winningSpin, stopPosition) * -360.0f;
		}

		private Sequence CreateSpinSeq(SequenceEventArgs args, bool winningSpin)
		{
			dialController.ClockUpdatingLocked = true;

			var stopAngle = GetStopAngle(winningSpin);
			var rotation = fullRotations * -360.0f + stopAngle;
			var seq = DOTween.Sequence();

			seq.Insert(0.0f, pointerRotationObject.transform
				.DOLocalRotate(new Vector3(0, 0, rotation), spinningDuration, RotateMode.FastBeyond360)
				.SetEase(Ease.OutSine)
			);

			seq.Insert(0.0f, playButtonImageSprite.GetComponent<SpriteRenderer>().DOFade(0.0f, textSpinFadeOutTime));
			seq.Insert(0.0f, returnButton.GetComponent<SpriteRenderer>().DOFade(0.0f, textSpinFadeOutTime));
			playButtonSparkles.Stop();

			seq.Insert(0.0f, DOTween.ToAlpha(() => returnButtonTextMesh.color, value => returnButtonTextMesh.color = value, 0.0f, textSpinFadeOutTime).SetEase(Ease.InOutQuad));
			seq.Insert(0.0f, DOTween.ToAlpha(() => playButtonTextMesh.color, value => playButtonTextMesh.color = value, 0.0f, textSpinFadeOutTime).SetEase(Ease.InOutQuad)
				.OnComplete(() =>
				{
					returnButton.SetActive(false);
					playButton.SetActive(false);
				}));
			return seq;
		}

		public bool RemoveAfterFirstInit => false;

		public void SceneInit(Stage currentStage)
		{
			var creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
			if (!StatusDatabase.GameStatus.CurrentGameState.HasValue)
				return;

			switch (creditPlayoffStatus.State)
			{
				case CreditPlayoffState.Available:
					// The dial is visible next to the credit meter.
					SetToAvailablePosition();
					break;

				case CreditPlayoffState.Unavailable:
					ResetGameObjects();
					break;

				case CreditPlayoffState.Win when StatusDatabase.GameStatus.CurrentGameState == GameState.History:
					if (StatusDatabase.HistoryStatus.HistoryStepType == HistoryStepType.CreditPlayoff)
						SetToHistoryPosition(true);
					else
						ResetGameObjects();
					break;

				case CreditPlayoffState.Loss when StatusDatabase.GameStatus.CurrentGameState == GameState.History:
					if (StatusDatabase.HistoryStatus.HistoryStepType == HistoryStepType.CreditPlayoff)
						SetToHistoryPosition(false);
					else
						ResetGameObjects();
					break;

				default:
					// The dial is expanded to the middle of the screen and can be played.
					SetToIdlePosition();
					break;
			}
		}

		private void SetToAvailablePosition()
		{
			animationRootObject.SetActive(true);
			startPlayoffButton.SetActive(true);
			returnButtonTextMesh.SetAlpha(0);
			playButtonTextMesh.SetAlpha(0);
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				extraSpinTextMesh.SetAlpha(0);
		}

		private void SetToIdlePosition()
		{
			SetToAvailablePosition();

			startPlayoffButton.SetActive(false);
			animationRootObject.transform.localPosition = zoomedUpPosition;
			animationRootObject.transform.localScale = new Vector3(zoomedUpScale, zoomedUpScale, zoomedUpScale);
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				extraSpinTextMesh.SetAlpha(1.0f);
			innerCircleSprite.GetComponent<SpriteRenderer>().SetAlpha(innerCircleSpriteAppearAlphaValue);
			logoSprite.GetComponent<SpriteRenderer>().SetAlpha(1.0f);
			playButtonImageSprite.GetComponent<SpriteRenderer>().SetAlpha(1.0f);
			returnButton.SetActive(true);
			playButton.SetActive(true);
			playButtonSparkles.Play();
			playButtonTextMesh.SetAlpha(1.0F);
			pointerRotationObject.transform.localEulerAngles = Vector3.zero;
			returnButton.GetComponent<SpriteRenderer>().SetAlpha(1.0f);
			returnButtonTextMesh.SetAlpha(1.0f);
			dimLayer.SetActive(true);
			dimLayer.GetComponent<SpriteRenderer>().SetAlpha(0.5f);
			pointerNormal.SetActive(true);
			pointerNormal.GetComponent<SpriteRenderer>().SetAlpha(1.0f);
		}

		private void SetToHistoryPosition(bool isWin)
		{
			animationRootObject.SetActive(true);
			animationRootObject.transform.localPosition = zoomedUpPosition;
			animationRootObject.transform.localScale = new Vector3(zoomedUpScale, zoomedUpScale, zoomedUpScale);
			winParticles.SetActive(isWin);
			startPlayoffButton.SetActive(false);
			returnButton.SetActive(false);
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				extraSpinTextMesh.SetAlpha(0);
			innerCircleSprite.GetComponent<SpriteRenderer>().SetAlpha(1);

			var stopAngle = GetStopAngle(isWin);
			pointerRotationObject.transform.localEulerAngles = new Vector3(0, 0, stopAngle);
			pointerNormal.SetActive(false);
			pointerLose.SetActive(!isWin);
			pointerWin.SetActive(isWin);

			dialGlowSprite.GetComponent<SpriteRenderer>().color = isWin
				? new Color(0.0f, 1.0f, 0.0f, 1.0f)
				: new Color(1.0f, 0.0f, 0.0f, 1.0f);

			winText.SetActive(isWin);
			winTextMesh.SetAlpha(isWin ? 1 : 0);
			winText.transform.localScale = isWin ? Vector3.one : Vector3.zero;
			lossGlowGameObject.SetActive(!isWin);
			lossGlowGameObject.GetComponent<SpriteRenderer>().SetAlpha(isWin ? 0 : 1);
		}

		private void ResetGameObjects()
		{
			if (!winTextMesh)
				return;

			dialController.ClockUpdatingLocked = false;
			animationRootObject.SetActive(false);
			animationRootObject.transform.localPosition = initialPosition;
			animationRootObject.transform.localScale = initialScale;
			lossGlowGameObject.SetActive(false);
			pointerNormal.SetActive(true);
			pointerLose.SetActive(false);
			pointerWin.SetActive(false);
			playButtonSparkles.Stop();
			winParticles.SetActive(false);
			lossGlowGameObject.GetComponent<SpriteRenderer>().SetAlpha(0);
			dialGlowSprite.GetComponent<SpriteRenderer>().color = new Color(0.0f, 1.0f, 0.0f, 0.0f);
			innerCircleSprite.GetComponent<SpriteRenderer>().SetAlpha(0);
			logoSprite.GetComponent<SpriteRenderer>().SetAlpha(0);
			playButtonImageSprite.GetComponent<SpriteRenderer>().SetAlpha(0);
			pointerNormal.GetComponent<SpriteRenderer>().SetAlpha(0);
			returnButton.GetComponent<SpriteRenderer>().SetAlpha(0);
			winRaysSprite.GetComponent<SpriteRenderer>().SetAlpha(0);
			dimLayer.GetComponent<SpriteRenderer>().SetAlpha(0);
			dimLayer.SetActive(false);
			pointerRotationObject.transform.localRotation = new Quaternion(0, 0, pointerAppearStartAngle, 1);
			winText.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
			winRaysSprite.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
			returnButton.SetActive(false);
			playButton.SetActive(false);
			startPlayoffButton.SetActive(false);

			winTextMesh.SetAlpha(0);
			foreach (var extraSpinTextMesh in extraSpinTextMeshes)
				extraSpinTextMesh.SetAlpha(0);
		}

		#endregion
	}
}