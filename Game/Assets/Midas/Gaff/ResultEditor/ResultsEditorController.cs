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

namespace Midas.Gaff.ResultEditor
{
	public sealed class ResultsEditorController : MonoBehaviour, IGamingSubsystem
	{
		private GameObject resultEditorGameObject;
		private ResultsEditorHandler reHandler;
		private Coroutine startCoroutine;

		[SerializeField]
		private GameObject resultEditorPrefab;

		#region IGamingSubsystem

		public string Name => nameof(ResultsEditorController);

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
		}

		public void OnStop()
		{
		}

		public void DeInit()
		{
			if (startCoroutine != null)
				StopCoroutine(startCoroutine);
		}

		#endregion

		private IEnumerator StartupCheck()
		{
			while (!StatusDatabase.GameStatus.IsSetupPresentationDone)
				yield return null;

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled)
			{
				Destroy(this);
				yield break;
			}

			reHandler = new ResultsEditorHandler(resultEditorPrefab, transform);
			Communication.ToPresentationSender.Send(new DemoGaffHandleMessage(reHandler, true));
			startCoroutine = null;
		}

		private sealed class ResultsEditorHandler : IGaffHandler
		{
			private readonly GameObject resultEditorPrefab;
			private readonly Transform transform;
			private GameObject resultEditorGameObject;
			private IReadOnlyList<Decision> decisions = new List<Decision>();

			public ResultsEditorHandler(GameObject resultEditorPrefab, Transform transform)
			{
				this.resultEditorPrefab = resultEditorPrefab;
				this.transform = transform;
			}

			public bool IsEnable
			{
				get
				{
					return StatusDatabase.GaffStatus is { IsDialUpActive: true, SelectedGaffIndex: null, AreGaffCyclesPending: false };
				}
			}

			public int Priority => 50;

			public IEnumerator<CoroutineInstruction> Run()
			{
				IDialUpResults gaffResults = null;
				var gaffStatus = StatusDatabase.GaffStatus;

				if (gaffStatus is { IsDialUpActive: false })
					yield break;

				var isShowing = true;
				yield return new CoroutineRun(Show(decisions, OnFinished));

				while (isShowing)
					yield return null;

				Hide();

				if (gaffResults == null)
					yield break;

				Communication.ToLogicSender.Send(new DemoGaffResultsMessage(gaffResults));

				void OnFinished((IDialUpResults Result, bool Continue, IReadOnlyList<Decision> Decisions) obj)
				{
					isShowing = false;
					gaffResults = obj.Result;
					gaffStatus.IsDialUpActive = obj.Continue;
					decisions = obj.Decisions;
				}
			}

			private void Hide()
			{
				Destroy(resultEditorGameObject);
				resultEditorGameObject = null;
			}

			private IEnumerator<CoroutineInstruction> Show(IReadOnlyList<Decision> previousDecisions, Action<(IDialUpResults Result, bool Continue, IReadOnlyList<Decision> Decisions)> obj)
			{
				GleDialUpData gleDialUpData = null;

				Communication.PresentationDispatcher.AddHandler<RequestGaffDataResponse>(OnGaffDataResponse);
				Communication.ToLogicSender.Send(new RequestGaffDataMessage());
				while (gleDialUpData == null)
					yield return null;

				Communication.PresentationDispatcher.RemoveHandler<RequestGaffDataResponse>(OnGaffDataResponse);

				var isDual = StatusDatabase.ConfigurationStatus.ConfiguredMonitors.All(cm => cm != MonitorType.MainPortrait);

				resultEditorGameObject = Instantiate(resultEditorPrefab, transform, false);
				var isStarting = StatusDatabase.GameStatus.CurrentGameState == GameState.Starting;
				resultEditorGameObject.GetComponent<ResultEditorDisplay>().Init(isStarting, isDual, previousDecisions, GleGameData.Runner, gleDialUpData.Inputs, gleDialUpData.PreviousResults, obj);

				void OnGaffDataResponse(RequestGaffDataResponse response) => gleDialUpData = (GleDialUpData)response.DialUpData;
			}
		}
	}
}