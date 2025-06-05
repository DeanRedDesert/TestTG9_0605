using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.SceneManagement;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.WinPresentation
{
	public sealed class WinMeterController : CompletionNotifier, ISequencePlayable, ISceneInitialiser
	{
		private WinCountController winCountingController;
		private bool registered;

		public void StartPlay()
		{
			winCountingController.StartWinCount();
		}

		public void StopPlay(bool reset)
		{
			if (reset)
				winCountingController.Reset(false);
			else
				winCountingController.Interrupt();
		}

		public bool IsPlaying()
		{
			return winCountingController.IsCounting;
		}

		private void Awake()
		{
			winCountingController = GameBase.GameInstance.GetPresentationController<WinCountController>();
			Register();
		}

		private void OnDestroy()
		{
			UnRegister();
			winCountingController = null;
		}

		private void Register()
		{
			if (!registered)
			{
				winCountingController.Complete += OnWinCountingReset;
				registered = true;
			}
		}

		private void UnRegister()
		{
			if (registered)
			{
				winCountingController.Complete -= OnWinCountingReset;
				registered = false;
			}
		}

		private void OnWinCountingReset(ICompletionNotifier _)
		{
			PostComplete();
		}

		public bool RemoveAfterFirstInit => false;

		public void SceneInit(Stage stage)
		{
			var gameState = StatusDatabase.GameStatus.CurrentGameState;
			winCountingController.Reset(!(gameState == GameState.OfferGamble || gameState == GameState.Continuing || gameState == GameState.ShowResult));
		}
	}
}