using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using UnityEngine;

namespace Midas.Presentation.Game
{
	public abstract class GameManagerBase : MonoBehaviour, IGamingSubsystem
	{
		private GameBase game;

		[SerializeField]
		private bool doLogicResetOnConfigChange;

		public virtual void Init()
		{
			Game.CreateCustomGameServices();
			Communication.PresentationDispatcher.AddHandler<RequestLogicLoaderMessage>(OnRequestGameLogicConfigurationMessage);
		}

		public virtual void DeInit()
		{
			Communication.PresentationDispatcher.RemoveHandler<RequestLogicLoaderMessage>(OnRequestGameLogicConfigurationMessage);
			Game.DestroyCustomGameServices();
		}

		public virtual void OnStart()
		{
		}

		public virtual void OnBeforeLoadGame()
		{
			Game.Create();
			Game.Init();
		}

		public virtual void OnAfterUnloadGame()
		{
			Game = null;
		}

		public virtual void OnStop()
		{
		}

		public string Name => "GameManager";

		protected abstract GameBase InstantiateGame();

		private void OnRequestGameLogicConfigurationMessage(RequestLogicLoaderMessage _) => Communication.ToLogicSender.Send(new RequestLogicLoaderResponse(Game.GetLogicLoader(doLogicResetOnConfigChange)));

		private GameBase Game
		{
			get => game ??= InstantiateGame();
			set
			{
				if (game != null)
				{
					game.DeInit();
					game.Destroy();
				}

				game = value;
			}
		}
	}
}