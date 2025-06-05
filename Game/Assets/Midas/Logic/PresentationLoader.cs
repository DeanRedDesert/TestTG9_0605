using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;

namespace Midas.Logic
{
	public static class PresentationLoader
	{
		private static PresentationLoadedState presentationLoadedState = PresentationLoadedState.Unloaded;
		private static GameIdentityType gameIdentityType;

		#region Public methods

		public static void Init(GameIdentityType gameIdentity)
		{
			Log.Instance.Info("PresentationLoader init");
			Communication.LogicDispatcher.AddHandler<GameLoadDoneMessage>(OnGameLoadDoneMessage);
			Communication.LogicDispatcher.AddHandler<GameUnloadDoneMessage>(OnGameUnloadDoneMessage);
			gameIdentityType = gameIdentity;
		}

		public static void DeInit()
		{
			Communication.LogicDispatcher.RemoveHandler<GameLoadDoneMessage>(OnGameLoadDoneMessage);
			Communication.LogicDispatcher.RemoveHandler<GameUnloadDoneMessage>(OnGameUnloadDoneMessage);
		}

		public static void LoadPresentation(bool waitLoaded)
		{
			Log.Instance.InfoFormat("Loading Presentation currentLoadedState={0}", presentationLoadedState);

			switch (presentationLoadedState)
			{
				case PresentationLoadedState.Unloaded:
					SyncWithPresentation();
					SendGameLoadMessage();
					if (waitLoaded)
					{
						WaitGamePresentationLoadedCompleted();
					}

					break;

				case PresentationLoadedState.LoadSent:
					if (waitLoaded)
					{
						WaitGamePresentationLoadedCompleted();
					}

					break;

				case PresentationLoadedState.Loaded:
					break;

				default:
					Log.Instance.Fatal($"Wrong load presentation state {presentationLoadedState}");
					break;
			}

			Log.Instance.InfoFormat("Loading Presentation currentLoadedState={0} done.", presentationLoadedState);
		}

		public static void UnloadPresentation(bool waitUnloaded)
		{
			Log.Instance.InfoFormat("Unload Presentation currentLoadedState={0}", presentationLoadedState);

			switch (presentationLoadedState)
			{
				case PresentationLoadedState.UnloadedSent:
					if (waitUnloaded)
					{
						WaitPresentationUnloaded();
					}

					break;

				case PresentationLoadedState.Unloaded:
					break;

				case PresentationLoadedState.LoadSent:
					WaitGamePresentationLoadedCompleted();
					SendGameUnloadMessage();
					if (waitUnloaded)
					{
						WaitPresentationUnloaded();
					}

					break;

				case PresentationLoadedState.Loaded:
					SendGameUnloadMessage();
					if (waitUnloaded)
					{
						WaitPresentationUnloaded();
					}

					break;

				default:
					Log.Instance.FatalFormat("Wrong load presentation state {0} for Unloading Presentation", presentationLoadedState);
					break;
			}

			Log.Instance.InfoFormat("Unload Presentation currentLoadedState={0} done", presentationLoadedState);
		}

		#endregion

		#region Private methods

		private static void SyncWithPresentation()
		{
			Log.Instance.Info("Syncing with presentation");

			Communication.ToPresentationSender.Send(new SyncPresentationMessage());
			GameLogic.WaitPresentationMessagesOnly<SyncPresentationMessage>();

			Log.Instance.Info("Syncing with presentation done.");
		}

		private static void SendGameLoadMessage()
		{
			Log.Instance.Info("Sending GameLoadMessage to presentation");
			Communication.ToPresentationSender.Send(new GameLoadMessage(gameIdentityType));
			presentationLoadedState = PresentationLoadedState.LoadSent;
		}

		private static void WaitGamePresentationLoadedCompleted()
		{
			Log.Instance.InfoFormat("Wait Presentation loaded currentLoadedState={0}", presentationLoadedState);

			var message = GameLogic.WaitPresentationMessagesOnly<GameLoadDoneMessage>();
			if (message == null || presentationLoadedState != PresentationLoadedState.Loaded)
			{
				Log.Instance.FatalFormat("Waiting Presentation Loaded failed. ({0}, {1})", presentationLoadedState, message);
			}

			Log.Instance.InfoFormat("Wait Presentation loaded currentLoadedState={0} done.", presentationLoadedState);
		}

		private static void SendGameUnloadMessage()
		{
			Log.Instance.Info("Sending GameUnloadMessage to presentation");
			Communication.ToPresentationSender.Send(new GameUnloadMessage());
			presentationLoadedState = PresentationLoadedState.UnloadedSent;
		}

		private static void WaitPresentationUnloaded()
		{
			Log.Instance.InfoFormat("Wait Presentation unloaded currentLoadedState={0}", presentationLoadedState);

			var message = GameLogic.WaitPresentationMessagesOnly<GameUnloadDoneMessage>();
			if (message == null || presentationLoadedState != PresentationLoadedState.Unloaded)
			{
				Log.Instance.FatalFormat("Waiting Presentation unloaded failed. ({0}, {1})", presentationLoadedState, message);
			}

			Communication.LogicDispatcher.ForceClearMessages();

			Log.Instance.InfoFormat("Wait Presentation unloaded currentLoadedState={0} done.", presentationLoadedState);
		}

		private static void OnGameLoadDoneMessage(GameLoadDoneMessage _)
		{
			presentationLoadedState = PresentationLoadedState.Loaded;
		}

		private static void OnGameUnloadDoneMessage(GameUnloadDoneMessage _)
		{
			presentationLoadedState = PresentationLoadedState.Unloaded;
		}

		#endregion
	}
}