using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.LogicToPresentation;
using Midas.Presentation.Audio;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Denom;
using Midas.Presentation.DevHelpers;
using Midas.Presentation.Gaff;
using Midas.Presentation.GameIdentity;
using Midas.Presentation.History;
using Midas.Presentation.Info;
using Midas.Presentation.Interruption;
using Midas.Presentation.Lights;
using Midas.Presentation.Progressives;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using Midas.Presentation.Stakes;
using Midas.Presentation.WinPresentation;
using UnityEngine;

namespace Midas.Presentation.Game
{
	public abstract class GameBase
	{
		public static GameBase GameInstance { get; private set; }

		private readonly List<IPresentationNode> nodes = new List<IPresentationNode>();
		private readonly List<IPresentationController> presentationControllers = new List<IPresentationController>();
		private readonly List<IButtonController> buttonControllers = new List<IButtonController>();
		private List<IGaffHandler> gaffHandlers = new List<IGaffHandler>();
		private IUtilityHandler utilityHandler;

		/// <summary>
		/// Override this in the derived class to change the game name.
		/// </summary>
		public abstract string GameName { get; }

		/// <summary>
		/// Override this in the derived class to change the game name style.
		/// </summary>
		public virtual GameNameStyle GameNameStyle { get; } = GameNameStyle.Automatic;

		public virtual Stage BaseGameStage { get; }

		#region General

		public void Create()
		{
			GameInstance = this;

			CreateNodes();
			CreatePresentationControllers();
			CreateButtonControllers();

			foreach (var owner in GetInterfaces<IPresentationNodeOwner>())
				nodes.AddRange(owner.PresentationNodes);
			foreach (var owner in GetInterfaces<IButtonControllerOwner>())
				buttonControllers.AddRange(owner.ButtonControllers);

			if (Application.isPlaying)
			{
				foreach (var cfg in GameInstance.GetInterfaces<IGameIdentityConfig>())
					cfg.ConfigureGameIdentity(StatusDatabase.ConfigurationStatus.GameIdentity!.Value);
			}
		}

		public void Init()
		{
			InitButtonPanelMappers();
			InitNodes();
			InitPresentationControllers();
			InitButtonControllers();
			InitMessageHandlers();
			Configure();
		}

		public void DeInit()
		{
			DeInitButtonControllers();
			DeInitPresentationControllers();
			DeInitNodes();
			DeInitButtonPanelMappers();
			DeInitMessageHandlers();
		}

		public void Destroy()
		{
			DestroyButtonControllers();
			DestroyPresentationControllers();
			DestroyNodes();

			GameInstance = null;
		}

		protected virtual void Configure()
		{
		}

		public abstract ILogicLoader GetLogicLoader(bool doLogicResetOnConfigChange);

		public virtual void CreateCustomGameServices()
		{
		}

		public virtual void DestroyCustomGameServices()
		{
		}

		public abstract Stage GetLogicStage(bool next);

		public abstract string GetStageIdFromLogicStage(Stage stage);

		public abstract Sequence GetTransitionSequence(Stage currentStage, Stage desiredStage);

		public T GetPresentationController<T>(bool throwErrorWhenNotFound = true) where T : IPresentationController
		{
			var presentationController = (T)presentationControllers.FirstOrDefault(c => c is T);
			if (throwErrorWhenNotFound && presentationController is null)
			{
				throw new Exception($"Controller of type {typeof(T).FullName} not found");
			}

			return presentationController;
		}

		public T GetInterface<T>() where T : class
		{
			var controllerInterface = (T)presentationControllers.FirstOrDefault(c => c is T) ??
				(T)buttonControllers.FirstOrDefault(c => c is T) ??
				(T)nodes.FirstOrDefault(c => c is T);

			return controllerInterface;
		}

		public IEnumerable<T> GetInterfaces<T>() where T : class
		{
			return nodes.OfType<T>()
				.Concat(presentationControllers.OfType<T>())
				.Concat(buttonControllers.OfType<T>());
		}

		public IGaffHandler GetFirstEnabledGaffHandler() => gaffHandlers.FirstOrDefault(gh => gh.IsEnable);

		public IUtilityHandler GetUtilityHandler() => utilityHandler;

		#endregion

		#region Nodes

		public IReadOnlyList<IPresentationNode> GetReadyNodes()
		{
			var result = new List<IPresentationNode>();

			foreach (var node in nodes)
				if (node.ReadyToStart)
					result.Add(node);

			return result;
		}

		public IReadOnlyList<IHistoryPresentationNode> GetReadyHistoryNodes()
		{
			var result = new List<IHistoryPresentationNode>();

			foreach (var node in nodes)
				if (node is IHistoryPresentationNode { ReadyToShowHistory: true } historyPresentationNode)
					result.Add(historyPresentationNode);

			return result;
		}

		protected void AddNode(IPresentationNode node)
		{
			nodes.Add(node);
		}

		protected virtual void CreateNodes()
		{
		}

		private void InitNodes()
		{
			foreach (var node in nodes)
				node.Init();
		}

		private void DeInitNodes()
		{
			foreach (var node in nodes)
				node.DeInit();
		}

		private void DestroyNodes()
		{
			foreach (var node in nodes)
				node.Destroy();

			nodes.Clear();
		}

		#endregion

		#region Presentation Controllers

		protected void AddPresentationController(IPresentationController controller)
		{
			presentationControllers.Add(controller);
		}

		private void CreatePresentationControllers()
		{
			AddPresentationController(new ButtonQueueController());
			AddPresentationController(new GameFlow());
			AddPresentationController(new UtilityController());
			AddPresentationController(new HistoryController());
			AddPresentationController(new InterruptController());
			AddPresentationController(CreateStageController());

			AddPresentationController(new WinPresentationController());
			AddPresentationController(new WinCountController());

			AddPresentationController(new DenomController());
			AddPresentationController(new InfoController());
			AddPresentationController(new DashboardController());
			AddPresentationController(new CashoutConfirmController());

			AddPresentationController(new StakesController());
			AddPresentationController(new SequenceFinder());
			AddPresentationController(new GaffController());
			AddPresentationController(new SpeedController());
			AddPresentationController(new GaffToggleController());
			AddPresentationController(new FeatureAutoStart());
			AddPresentationController(new LightsController());
			AddPresentationController(new ProgressiveDisplayController());
			AddPresentationController(new AutoPlayController());
			AddPresentationController(new PlayerSessionController());
			AddPresentationController(new VolumeController());
			AddPresentationController(new WinMeterResetController());

			CreateCustomPresentationControllers();
		}

		protected abstract void CreateCustomPresentationControllers();

		protected virtual StageController CreateStageController()
		{
			return new StageController(Array.Empty<Sequence>());
		}

		private void InitPresentationControllers()
		{
			for (var i = 0; i < presentationControllers.Count; i++)
			{
				var controller = presentationControllers[i];
				controller.Init();
			}
		}

		private void DeInitPresentationControllers()
		{
			for (var i = presentationControllers.Count - 1; i >= 0; i--)
			{
				var controller = presentationControllers[i];
				controller.DeInit();
			}
		}

		private void DestroyPresentationControllers()
		{
			for (var i = presentationControllers.Count - 1; i >= 0; i--)
			{
				var controller = presentationControllers[i];
				controller.Destroy();
			}

			presentationControllers.Clear();
		}

		#endregion

		#region Button Controllers

		private readonly List<IButtonPanelMapper> buttonPanelMappers = new List<IButtonPanelMapper>();

		private void AddButtonController(IButtonController buttonController)
		{
			buttonControllers.Add(buttonController);
		}

		private void CreateButtonControllers()
		{
			AddButtonController(new PlayButtonController());
			AddButtonController(new VolumeButtonController());
			AddButtonController(new CashoutButtonController());
			AddButtonController(new MoreGamesButtonController());
			AddButtonController(new ReserveButtonController());
			AddButtonController(new HistoryButtonController());
			AddButtonController(new GaffButtonController());
			AddButtonController(new DenomButtonController());
			AddButtonController(new SpeedButtonController());
			AddButtonController(new ServiceButtonController());
			AddButtonController(new AutoPlayButtonController());
			AddButtonController(new LanguageButtonController());

			CreateCustomButtonControllers(AddButtonController);
		}

		protected virtual void CreateCustomButtonControllers(Action<IButtonController> addController)
		{
		}

		private void InitButtonControllers()
		{
			foreach (var c in buttonControllers)
				c.Init();
		}

		private void DeInitButtonControllers()
		{
			foreach (var c in buttonControllers)
				c.DeInit();
		}

		private void DestroyButtonControllers()
		{
			foreach (var c in buttonControllers)
				c.Destroy();

			buttonControllers.Clear();
		}

		private void InitButtonPanelMappers()
		{
			foreach (var mapper in CreateButtonPanelMappers())
			{
				buttonPanelMappers.Add(mapper);
			}

			CabinetManager.Cabinet.AddButtonPanelMappers(buttonPanelMappers);
		}

		private void DeInitButtonPanelMappers()
		{
			CabinetManager.Cabinet.RemoveButtonPanelMappers(buttonPanelMappers);
			buttonPanelMappers.Clear();
		}

		private static IReadOnlyList<IButtonPanelMapper> CreateButtonPanelMappers()
		{
			return new List<IButtonPanelMapper>
			{
				new DefaultButtonPanelMapper()
			};
		}

		private void InitMessageHandlers()
		{
			Communication.PresentationDispatcher.AddHandler<DemoGaffHandleMessage>(OnDemoActivateGaffHandlerMessage);
			Communication.PresentationDispatcher.AddHandler<DemoGaffActionMessage>(OnDemoGaffActionMessage);
			Communication.PresentationDispatcher.AddHandler<UtilityHandleMessage>(OnUtilityHandlerMessage);
		}

		private void DeInitMessageHandlers()
		{
			Communication.PresentationDispatcher.RemoveHandler<DemoGaffHandleMessage>(OnDemoActivateGaffHandlerMessage);
			Communication.PresentationDispatcher.RemoveHandler<DemoGaffActionMessage>(OnDemoGaffActionMessage);
			Communication.PresentationDispatcher.RemoveHandler<UtilityHandleMessage>(OnUtilityHandlerMessage);
		}

		private void OnDemoActivateGaffHandlerMessage(DemoGaffHandleMessage message)
		{
			if (message.IsActive)
				gaffHandlers.Add(message.GaffHandler);
			else
				gaffHandlers.Remove(message.GaffHandler);

			gaffHandlers = gaffHandlers.OrderByDescending(gh => gh.Priority).ToList();
		}

		private void OnUtilityHandlerMessage(UtilityHandleMessage message)
		{
			utilityHandler = message.UtilityHandler;
		}

		private static void OnDemoGaffActionMessage(DemoGaffActionMessage message) => message.RunAction();

		#endregion
	}
}