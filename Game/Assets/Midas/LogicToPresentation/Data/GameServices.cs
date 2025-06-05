using System;
using System.Collections.Generic;
using Midas.Core.LogicServices;
using Midas.LogicToPresentation.Data.Services;
using Midas.LogicToPresentation.Messages;

namespace Midas.LogicToPresentation.Data
{
	public static class GameServices
	{
		private static readonly List<(IGameService service, object value)> pendingChanges = new List<(IGameService, object)>();
		private static bool isBatching;

		public static Configuration ConfigurationService { get; private set; }
		public static MachineState MachineStateService { get; private set; }
		public static BetService BetService { get; private set; }
		public static MetersService MetersService { get; private set; }
		public static ProgressiveService ProgressiveService { get; private set; }
		public static ExternalJackpotsService ExternalJackpotService { get; private set; }
		public static PidService PidService { get; private set; }
		public static GameFunctionStatusService GameFunctionStatusService { get; private set; }
		public static GaffService GaffService { get; private set; }
		public static PlayerSessionService PlayerSessionService { get; private set; }

		private static CompositeGameService gameServiceRoot;

		public static void Init()
		{
			gameServiceRoot = new CompositeGameService();
			gameServiceRoot.Init("Root");

			AddService(ConfigurationService = new Configuration(), nameof(ConfigurationService));
			AddService(MachineStateService = new MachineState(), nameof(MachineStateService));
			AddService(BetService = new BetService(), nameof(BetService));
			AddService(MetersService = new MetersService(), nameof(MetersService));
			AddService(ProgressiveService = new ProgressiveService(), nameof(ProgressiveService));
			AddService(ExternalJackpotService = new ExternalJackpotsService(), nameof(ExternalJackpotService));
			AddService(PidService = new PidService(), nameof(PidService));
			AddService(GameFunctionStatusService = new GameFunctionStatusService(), nameof(GameFunctionStatusService));
			AddService(GaffService = new GaffService(), nameof(GaffService));
			AddService(PlayerSessionService = new PlayerSessionService(), nameof(PlayerSessionService));
		}

		public static void DeInit()
		{
			RemoveService(ConfigurationService);
			RemoveService(MachineStateService);
			RemoveService(BetService);
			RemoveService(MetersService);
			RemoveService(ProgressiveService);
			RemoveService(ExternalJackpotService);
			RemoveService(PidService);
			RemoveService(GameFunctionStatusService);
			RemoveService(GaffService);
			RemoveService(PlayerSessionService);

			gameServiceRoot.DeInit();
			gameServiceRoot = null;
		}

		public static T GetService<T>() where T : CompositeGameService
		{
			return gameServiceRoot.GetService<T>();
		}

		internal static object GetHistoryData(HistorySnapshotType snapshotType) => ((IGameService)gameServiceRoot).GetHistoryData(snapshotType);

		internal static void RestoreHistoryData(HistorySnapshotType snapshotType, object data) => ((IGameService)gameServiceRoot).RestoreHistoryData(snapshotType, data);

		internal static void RefreshAll()
		{
			((IGameService)gameServiceRoot).Refresh();
		}

		/// <summary>
		/// Begins a batch of game service updates.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		internal static void BeginBatch()
		{
			if (isBatching)
				throw new InvalidOperationException("Cannot begin a batch, there is already one in progress.");

			isBatching = true;
		}

		/// <summary>
		/// End a batch of game service updates.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		internal static void EndBatch()
		{
			if (!isBatching)
				throw new InvalidOperationException("Cannot end batching, there is not already one in progress.");

			isBatching = false;
			if (pendingChanges.Count > 0)
			{
				var committedChanges = new List<(IGameService service, object value)>(pendingChanges);
				pendingChanges.Clear();

				Communication.ToPresentationSender.Send(new GameServiceUpdateBatchMessage(committedChanges));
			}
		}

		/// <summary>
		/// Abort the construction of a service update batch.
		/// </summary>
		internal static void AbortBatch()
		{
			isBatching = false;
			pendingChanges.Clear();
		}

		/// <summary>
		/// Queue a change to a game service.
		/// </summary>
		/// <param name="service">The service to queue.</param>
		/// <param name="value">The new service value.</param>
		internal static void ServiceChanged(IGameService service, object value)
		{
			if (!isBatching)
			{
				Communication.ToPresentationSender.Send(new GameServiceUpdateSingleMessage(service, value));
				return;
			}

			// Overwrite the value if it is already in the list of pending changes.

			for (var i = 0; i < pendingChanges.Count; i++)
			{
				if (pendingChanges[i].service == service)
				{
					pendingChanges[i] = (service, value);
					return;
				}
			}

			pendingChanges.Add((service, value));
		}

		public static void AddService(IGameService service, string serviceName)
		{
			gameServiceRoot.AddService(service, serviceName);
		}

		public static void RemoveService(IGameService service)
		{
			gameServiceRoot.RemoveService(service);
		}
	}
}