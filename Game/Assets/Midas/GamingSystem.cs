using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.SDKAssets.StandaloneDeviceConfiguration;
using Midas.Ascent;
using Midas.Ascent.Cabinet;
using Midas.Core;
using Midas.Core.StateMachine;
using Midas.Logic;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Midas
{
	/// <summary>
	/// The gaming system. Also contains the game engine thread.
	/// </summary>
	public sealed partial class GamingSystem : MonoBehaviour
	{
		private bool isShuttingDown;
		private List<IGamingSubsystem> subsystems = new List<IGamingSubsystem>();

		[SerializeField]
		private GameIdentityType preferredGlobalGameIdentity;

		private void Awake()
		{
#if UNITY_EDITOR
			void OnPlayModeChanged(UnityEditor.PlayModeStateChange change)
			{
				if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
				{
					isShuttingDown = true;
					DoShutdown();
					UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
				}
			}

			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif

			Tools.Logging.Init();
			Application.logMessageReceivedThreaded += OnUnityLogMessage;
			ErrorHandler.OnError += OnError;

			try
			{
				CreateCoreSubsystems();
				Communication.Init();
				GameServices.Init();
				InitAscent();
				GetSceneSubsystems();
				StartupManager.Init(subsystems, GetComponentInScene<ISceneLoader>(gameObject.scene));
				Communication.PresentationDispatcher.AddHandler<GameServiceUpdateMessage>(HandleUpdateGameServices);

				Log.Instance.Info("Init all done");
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Log.Instance.Warn("Init all failed", e);
				throw;
#else
				Log.Instance.Fatal("Init all failed", e);
#endif
			}
		}

		private void InitAscent()
		{
			AscentFoundation.Init(new GameLogic(), preferredGlobalGameIdentity, Application.isEditor);
			var standaloneDevices = GetComponentInScene<StandaloneDeviceConfigurator>(gameObject.scene)?.GetStandaloneCabinetInterfaces();
			var cabinet = AscentCabinet.Create(standaloneDevices?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
			subsystems.Add(cabinet.CabinetSubsystem);
			CabinetManager.Init(cabinet);
		}

		private void Start()
		{
			try
			{
				Log.Instance.Info("Starting all ...");
				AscentFoundation.Start();
				StartupManager.Start();
				Log.Instance.Info("Starting all done.");
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Log.Instance.Warn("Starting all failed", e);
				throw;
#else
				Log.Instance.Fatal("Starting all failed", e);
#endif
			}
		}

		private void Update()
		{
			if (isShuttingDown)
				return;

			if (AscentFoundation.LogicThreadException != null)
				throw new ApplicationException("Logic thread has crashed", AscentFoundation.LogicThreadException);

			try
			{
				if (GamePresentationTimings.LogData)
					Log.Instance.Info(GamePresentationTimings.AsString());

				GamePresentationTimings.OverallTime.Start();
				GamePresentationTimings.OverallGamingSystemTime.Start();
				GamePresentationTimings.OverallUpdateTime.Start();
				GamePresentationTimings.GamingSystemUpdateTime.Start();

				// Handle time in such a way as to avoid precision issues when the game is running for a few days.

				var currentTime = TimeSpan.FromSeconds(Time.timeAsDouble);
				var deltaTime = TimeSpan.FromSeconds(Time.deltaTime);
				var unscaledTime = TimeSpan.FromSeconds(Time.unscaledTimeAsDouble);

				FrameTime.SetNextFrame(currentTime, deltaTime, unscaledTime);

				GamePresentationTimings.L2PTime.Start();
				Communication.PresentationDispatcher.DispatchAll();
				GamePresentationTimings.L2PTime.Stop();

				GamePresentationTimings.CabinetLibTime.Start();
				AscentCabinet.Update();
				GamePresentationTimings.CabinetLibTime.Stop();

				GamePresentationTimings.BeforeFrameUpdateTime.Start();
				FrameUpdateService.PreUpdate.DoUpdate();
				GamePresentationTimings.BeforeFrameUpdateTime.Stop();

				UpdateStatusDatabaseAndStateMachinesUntilNothingChanges();

				GamePresentationTimings.ExpressionManagerTime.Start();
				ExpressionManager.Update();
				GamePresentationTimings.ExpressionManagerTime.Stop();

				GamePresentationTimings.ButtonServiceTime.Start();
				ButtonManager.Update();
				GamePresentationTimings.ButtonServiceTime.Stop();

				GamePresentationTimings.FrameUpdateTime.Start();
				FrameUpdateService.Update.DoUpdate();
				GamePresentationTimings.FrameUpdateTime.Stop();

				GamePresentationTimings.AfterFrameUpdateTime.Start();
				FrameUpdateService.PostUpdate.DoUpdate();
				GamePresentationTimings.AfterFrameUpdateTime.Stop();

				GamePresentationTimings.GamingSystemUpdateTime.Stop();
				GamePresentationTimings.OverallGamingSystemTime.Lap();
				GamePresentationTimings.OverallUnityTime.Start();
				GamePresentationTimings.UnityUpdateTime.Start();
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Log.Instance.Warn("Frame Update failed", e);
				throw;
#else
				Log.Instance.Fatal($"Frame Update failed", e);
#endif
			}
		}

		private void LateUpdate()
		{
			try
			{
				if (isShuttingDown)
					return;

				GamePresentationTimings.OverallGamingSystemTime.Start();
				GamePresentationTimings.OverallLateUpdateTime.Start();
				GamePresentationTimings.AnimatorAndCoRoutineUpdateTime.Stop();

				GamePresentationTimings.FrameLateUpdateTime.Start();
				FrameUpdateService.LateUpdate.DoUpdate();
				GamePresentationTimings.FrameLateUpdateTime.Stop();

				GamePresentationTimings.OverallGamingSystemTime.Stop();
				GamePresentationTimings.OverallUnityTime.Start();
				GamePresentationTimings.UnityLateUpdateTime.Start();
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Log.Instance.Warn("Frame LateUpdate failed", e);
				throw;
#else
				Log.Instance.Fatal($"Frame LateUpdate failed", e);
#endif
			}
		}

		private void OnDestroy()
		{
#if !UNITY_EDITOR
			DoShutdown();
#endif
			Application.logMessageReceivedThreaded -= OnUnityLogMessage;
			ErrorHandler.OnError -= OnError;
		}

		private static void DoShutdown()
		{
			Communication.PresentationDispatcher.RemoveHandler<GameServiceUpdateMessage>(HandleUpdateGameServices);

			AscentFoundation.Stop(ShutdownReason.ExitingPlayMode);
			StartupManager.Stop(true);
			AscentFoundation.DeInit();
			GameServices.DeInit();
			Communication.DeInit();
			Tools.Logging.DeInit();
		}

		private static void UpdateStatusDatabaseAndStateMachinesUntilNothingChanges()
		{
			GamePresentationTimings.DataBaseTime.Start();

			var loopCount = 0;

			StatusDatabase.ApplyAllModifications();
			GamePresentationTimings.DataBaseTime.Lap();

			GamePresentationTimings.StateMachineTime.Start();
			GamePresentationTimings.MaxStateMachineLoops = StateMachineService.Update();
			GamePresentationTimings.StateMachineTime.Lap();

			while (StatusDatabase.ApplyAllModifications())
			{
				GamePresentationTimings.StateMachineTime.Start();
				GamePresentationTimings.MaxStateMachineLoops = StateMachineService.Update();
				GamePresentationTimings.StateMachineTime.Lap();
				loopCount++;

				if (loopCount > 100)
				{
					//Log.Instance.Fatal("Maximum loopCounter reached");
				}
			}

			GamePresentationTimings.DataBaseTime.Stop();
			GamePresentationTimings.StateMachineTime.Stop();
			GamePresentationTimings.MaxStatusDatabaseStateMachineLoops = loopCount;
		}

		private void CreateCoreSubsystems()
		{
			subsystems.Add(new StatusDatabaseSubsystem());
			subsystems.Add(new PresentationSubsystem());
		}

		private void GetSceneSubsystems()
		{
			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				subsystems.AddRange(scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<IGamingSubsystem>()));
			}
		}

		private static T GetComponentInScene<T>(Scene scene, bool includeInactive = false)
		{
			foreach (var gameObject in scene.GetRootGameObjects())
			{
				var result = gameObject.GetComponentInChildren<T>(includeInactive);
				if (result is { })
					return result;
			}

			return default;
		}

		private static void HandleUpdateGameServices(GameServiceUpdateMessage message)
		{
			message.DeliverChanges();
		}
	}
}