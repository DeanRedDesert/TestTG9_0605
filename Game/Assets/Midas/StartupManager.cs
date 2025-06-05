using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas
{
	public static class StartupManager
	{
		private static ISceneLoader sceneLoader;
		private static IReadOnlyList<IGamingSubsystem> subsystems;
		private static bool onLoadGameCalled;
		private static bool stopDone;

		public static void Init(IReadOnlyList<IGamingSubsystem> subsystems, ISceneLoader sceneLoader)
		{
			Log.Instance.Info("Starting Up");

			StartupManager.subsystems = subsystems;
			StartupManager.sceneLoader = sceneLoader;
			Communication.PresentationDispatcher.AddHandler<SyncPresentationMessage>(OnSyncPresentationMessage);
			Communication.PresentationDispatcher.AddHandler<GameLoadMessage>(OnGameLoadMessage);
			Communication.PresentationDispatcher.AddHandler<GameUnloadMessage>(OnGameUnloadMessage);
			Communication.PresentationDispatcher.AddHandler<GameSetupPresentationMessage>(OnGameSetupPresentation);
			Communication.PresentationDispatcher.AddHandler<GameSetupPresentationDoneMessage>(OnGameSetupPresentationDone);
			Communication.PresentationDispatcher.AddHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);

			foreach (var subsystem in subsystems)
			{
				Log.Instance.InfoFormat("Initialising Subsystem {0}", subsystem.Name);
				subsystem.Init();
			}
		}

		public static void Start()
		{
			CheckSubSystems();
			foreach (var subsystem in subsystems)
			{
				Log.Instance.InfoFormat("Starting Subsystem {0}", subsystem.Name);
				subsystem.OnStart();
			}

			stopDone = false;
		}

		private static void OnBeforeLoadGame()
		{
			CheckSubSystems();
			for (var i = 0; i < subsystems.Count; i++)
			{
				var subsystem = subsystems[i];
				Log.Instance.InfoFormat("OnBeforeLoadGame {0}", subsystem.Name);
				subsystem.OnBeforeLoadGame();
			}

			onLoadGameCalled = true;
		}

		private static void OnAfterUnloadGame()
		{
			CheckSubSystems();
			for (var i = subsystems.Count - 1; i >= 0; i--)
			{
				var subsystem = subsystems[i];
				Log.Instance.InfoFormat("OnAfterUnloadGame {0}", subsystem.Name);
				subsystem.OnAfterUnloadGame();
			}
		}

		public static void Stop(bool fullShutdown)
		{
			Log.Instance.Info("Stopping");

			CheckSubSystems();
			Communication.PresentationDispatcher.RemoveHandler<SyncPresentationMessage>(OnSyncPresentationMessage);

			if (onLoadGameCalled)
			{
				StatusDatabase.GameStatus.IsSetupPresentationDone = false;
				sceneLoader?.UnloadScenes();
				OnAfterUnloadGame();
				onLoadGameCalled = false;
			}

			if (!stopDone)
			{
				foreach (var subsystem in subsystems)
				{
					Log.Instance.InfoFormat("Stopping Subsystem {0}", subsystem.Name);
					subsystem.OnStop();
				}

				stopDone = true;
			}

			if (fullShutdown)
			{
				Communication.PresentationDispatcher.RemoveHandler<SyncPresentationMessage>(OnSyncPresentationMessage);
				Communication.PresentationDispatcher.RemoveHandler<GameLoadMessage>(OnGameLoadMessage);
				Communication.PresentationDispatcher.RemoveHandler<GameUnloadMessage>(OnGameUnloadMessage);
				Communication.PresentationDispatcher.RemoveHandler<GameSetupPresentationMessage>(OnGameSetupPresentation);
				Communication.PresentationDispatcher.RemoveHandler<GameSetupPresentationDoneMessage>(OnGameSetupPresentationDone);
				Communication.PresentationDispatcher.RemoveHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);

				for (var i = subsystems.Count - 1; i >= 0; i--)
				{
					var subsystem = subsystems[i];
					Log.Instance.InfoFormat("DeInit {0}", subsystem.Name);
					subsystem.DeInit();
				}

				GameServiceConsumerExtensions.CheckForHangingChangeListeners();
			}
		}

		private static void OnSyncPresentationMessage(SyncPresentationMessage message)
		{
			Communication.ToLogicSender.Send(message);
		}

		private static void OnGameLoadMessage(GameLoadMessage message)
		{
			if (!StatusDatabase.ConfigurationStatus.GameIdentity.HasValue)
				StatusDatabase.ConfigurationStatus.GameIdentity = message.GameIdentity;

			OnBeforeLoadGame();

			onLoadGameCalled = true;

			if (sceneLoader == null)
				Communication.ToLogicSender.Send(new GameLoadDoneMessage());
			else
			{
				sceneLoader.AllInitialScenesLoaded += OnSceneLoaderOnAllInitialScenesLoaded;
				StatusDatabase.GameStatus.IsSetupPresentationDone = false;
				sceneLoader.LoadInitialScenesAsync();
			}
		}

		private static void OnGameUnloadMessage(GameUnloadMessage message)
		{
			if (sceneLoader == null)
			{
				OnAfterUnloadGame();
				onLoadGameCalled = false;
				Communication.PresentationDispatcher.ForceClearMessages();
				Communication.ToLogicSender.Send(new GameUnloadDoneMessage());
			}
			else
			{
				sceneLoader.AllScenesUnloaded += OnSceneLoaderOnAllScenesUnloaded;
				StatusDatabase.GameStatus.IsSetupPresentationDone = false;
				sceneLoader.UnloadScenesAsync();
			}
		}

		private static void OnGameSetupPresentation(GameSetupPresentationMessage _)
		{
			Log.Instance.Info("GameSetupPresentationMessage received");
			StatusDatabase.GameStatus.IsSetupPresentationDone = true;
			Communication.ToLogicSender.Send(new GameSetupPresentationDoneMessage());
		}

		private static void OnGameSetupPresentationDone(GameSetupPresentationDoneMessage _)
		{
			Log.Instance.Info("GameSetupPresentationDoneMessage received");
		}

		private static void OnGameStopGameEngine(GameStopGameEngineMessage _)
		{
			Log.Instance.Info("GameStopGameEngineMessage received");

			CabinetManager.Cabinet.SetReadyState(false);
		}

		private static void OnSceneLoaderOnAllInitialScenesLoaded()
		{
			Communication.ToLogicSender.Send(new GameLoadDoneMessage());

			sceneLoader.AllInitialScenesLoaded -= OnSceneLoaderOnAllInitialScenesLoaded;
		}

		private static void OnSceneLoaderOnAllScenesUnloaded()
		{
			OnAfterUnloadGame();

			sceneLoader.AllScenesUnloaded -= OnSceneLoaderOnAllScenesUnloaded;
			onLoadGameCalled = false;

			Communication.PresentationDispatcher.ForceClearMessages();
			Communication.ToLogicSender.Send(new GameUnloadDoneMessage());
		}

		private static void CheckSubSystems()
		{
			subsystems = subsystems.Where(ss => !(ss is MonoBehaviour mb && mb == null)).ToList();
		}
	}
}