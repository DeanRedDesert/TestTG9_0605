using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator.Decisions;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.Coroutine;
using Midas.Gle.Logic;
using Midas.Gle.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using UnityEngine;
using Coroutine = UnityEngine.Coroutine;

namespace Midas.Utility.ResultPicker
{
	public sealed class UtilityEditorController : MonoBehaviour, IGamingSubsystem
	{
		private GameObject resultEditorGameObject;
		private UtilityEditorHandler reHandler;
		private Coroutine startCoroutine;

		[SerializeField]
		private GameObject resultEditorPrefab;

		[SerializeField]
		private UtilitySymbolWindows[] utilitySymbolWindows;

		#region IGamingSubsystem

		public string Name => nameof(UtilityEditorController);

		public void Init()
		{
		}

		public void OnStart()
		{
		}

		public void OnBeforeLoadGame()
		{
			startCoroutine = StartCoroutine(StartupCheck());
		}

		public void OnAfterUnloadGame()
		{
			if (startCoroutine != null)
				StopCoroutine(startCoroutine);

			reHandler?.Hide();
			reHandler = null;
		}

		public void OnStop()
		{
		}

		public void DeInit()
		{
		}

		#endregion

		private IEnumerator StartupCheck()
		{
			while (!StatusDatabase.GameStatus.IsSetupPresentationDone)
				yield return null;

			if (StatusDatabase.GameStatus.GameMode != FoundationGameMode.Utility)
				yield break;

			if (StatusDatabase.UtilityStatus.IsUtilityModeEnabled)
				yield break;

			reHandler = new UtilityEditorHandler(resultEditorPrefab, utilitySymbolWindows, transform);
			Communication.ToPresentationSender.Send(new UtilityHandleMessage(reHandler));
			startCoroutine = null;
		}

		private sealed class UtilityEditorHandler : IUtilityHandler
		{
			private readonly GameObject resultEditorPrefab;
			private readonly UtilitySymbolWindows[] utilitySymbolWindows;
			private readonly Transform transform;
			private GameObject resultEditorGameObject;
			private IReadOnlyList<Decision> decisions = new List<Decision>();

			public UtilityEditorHandler(GameObject resultEditorPrefab, UtilitySymbolWindows[] utilitySymbolWindows, Transform transform)
			{
				this.resultEditorPrefab = resultEditorPrefab;
				this.utilitySymbolWindows = utilitySymbolWindows;
				this.transform = transform;
			}

			public bool IsEnable { get { return StatusDatabase.GameStatus.GameMode == FoundationGameMode.Utility; } }

			public IEnumerator<CoroutineInstruction> Run()
			{
				IDialUpResults gaffResults = null;

				var isShowing = true;
				yield return new CoroutineRun(Show(decisions, OnFinished));

				while (isShowing)
					yield return null;

				Hide();

				if (gaffResults == null)
					yield break;

				Communication.ToLogicSender.Send(new UtilityResultsMessage(gaffResults));

				void OnFinished((IDialUpResults Result, IReadOnlyList<Decision> Decisions) obj)
				{
					isShowing = false;
					gaffResults = obj.Result;
					decisions = obj.Decisions;
				}
			}

			internal void Hide()
			{
				Destroy(resultEditorGameObject);
				resultEditorGameObject = null;
			}

			private IEnumerator<CoroutineInstruction> Show(IReadOnlyList<Decision> previousDecisions, Action<(IDialUpResults Result, IReadOnlyList<Decision> Decisions)> onFinished)
			{
				GleDialUpData gleDialUpData = null;

				Communication.PresentationDispatcher.AddHandler<RequestUtilityDataResponse>(OnUtilityDataResponse);
				Communication.ToLogicSender.Send(new RequestUtilityDataMessage());
				while (gleDialUpData == null)
					yield return null;

				Communication.PresentationDispatcher.RemoveHandler<RequestUtilityDataResponse>(OnUtilityDataResponse);

				var isDual = StatusDatabase.ConfigurationStatus.ConfiguredMonitors.All(cm => cm != MonitorType.MainPortrait);

				resultEditorGameObject = Instantiate(resultEditorPrefab, transform, false);
				resultEditorGameObject.GetComponent<UtilityEditorDisplay>().Init(isDual, previousDecisions, GleGameData.Runner, gleDialUpData.Inputs, gleDialUpData.PreviousResults, utilitySymbolWindows, onFinished);

				void OnUtilityDataResponse(RequestUtilityDataResponse response) => gleDialUpData = (GleDialUpData)response.GaffData;
			}
		}
	}
}