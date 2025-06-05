using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed class MessageStrip : MonoBehaviour, IMessageHandler
	{
		private DashboardController dashboardController;
		private Coroutine messageStripCoroutine;
		private TMP_TextInfo textInfo;
		private Action localizedStringCleanup;

		[SerializeField]
		private TMP_Text textArea;

		[SerializeField]
		private LocalizedString gameOverText;

		[SerializeField]
		private LocalizedString pressTakeWinText;

		[SerializeField]
		private LocalizedString gambleAvailableText;

		private void Awake()
		{
			dashboardController = GameBase.GameInstance.GetPresentationController<DashboardController>();
			textArea.OnPreRenderText += OnPreRenderText;
			textArea.text = string.Empty;
		}

		private void OnEnable()
		{
			dashboardController.RegisterMessageHandler(this);
			textArea.OnPreRenderText += OnPreRenderText;
		}

		private void OnDisable()
		{
			if (localizedStringCleanup != null)
			{
				localizedStringCleanup();
				localizedStringCleanup = null;
			}

			textArea.OnPreRenderText -= OnPreRenderText;
			dashboardController.UnregisterMessageHandler(this);
			messageStripCoroutine?.Stop();
			messageStripCoroutine = null;
		}

		private void OnDestroy()
		{
			dashboardController = null;
		}

		private void OnPreRenderText(TMP_TextInfo info)
		{
			textInfo = info;
		}

		private IEnumerator<CoroutineInstruction> DoShowMessage(string message)
		{
			textArea.transform.SetLocalPosY(0);
			if (textArea.text != message)
			{
				textInfo = null;
				textArea.text = message;
				while (textInfo == null)
					yield return null;
			}

			yield return new CoroutineDelay(2);

			for (var i = 1; i < textInfo.lineCount; i++)
			{
				var currY = textInfo.lineInfo[0].baseline - textInfo.lineInfo[i - 1].baseline;
				var targetY = textInfo.lineInfo[0].baseline - textInfo.lineInfo[i].baseline;

				var t = 0.5;
				while (t > 0)
				{
					textArea.transform.SetLocalPosY(Mathf.Lerp(targetY, currY, (float)(t / 0.5)));
					yield return null;
					t -= FrameTime.DeltaTime.TotalSeconds;
				}

				textArea.transform.SetLocalPosY(targetY);
				yield return new CoroutineDelay(2);
			}

			MessageDisplayDone = true;
		}

		#region IMessageHandler implementation

		public bool MessageDisplayDone { get; private set; }

		public void DisplayMessage(string message)
		{
			if (localizedStringCleanup != null)
			{
				localizedStringCleanup();
				localizedStringCleanup = null;
			}

			if (string.IsNullOrEmpty(message))
			{
				textArea.gameObject.SetActive(false);
				return;
			}

			textArea.gameObject.SetActive(true);

			MessageDisplayDone = false;
			messageStripCoroutine?.Stop();
			messageStripCoroutine = FrameUpdateService.Update.StartCoroutine(DoShowMessage(message));
		}

		public void DisplayMessage(GameMessage message)
		{
			// Switch on purpose to ensure only one flag is set.

			switch (message)
			{
				case GameMessage.GameOver:
					Setup(gameOverText);
					break;
				case GameMessage.PressTakeWin:
					Setup(pressTakeWinText);
					break;
				case GameMessage.GambleAvailable:
					Setup(gambleAvailableText);
					break;

				default:
				{
					var msg = $"Exactly one {nameof(GameMessage)} flag must be set";
					Log.Instance.Error(msg);
					throw new ArgumentException(msg, nameof(message));
				}
			}

			void Setup(LocalizedString s)
			{
				DisplayMessage(s.GetLocalizedString());
				s.StringChanged += UpdateText;
				localizedStringCleanup = () => Cleanup(s);
			}

			void UpdateText(string value)
			{
				textArea.text = value;
			}

			void Cleanup(LocalizedString s)
			{
				s.StringChanged -= UpdateText;
			}
		}

		#endregion
	}
}