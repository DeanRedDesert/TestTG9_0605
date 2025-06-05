using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Game
{
	public sealed class StageSwitcher
	{
		#region Public

		public void SwitchStage(Stage desiredStage)
		{
			if (desiredStage != null)
			{
				StageController.SwitchTo(desiredStage);
			}
		}

		public bool IsStageTransitioning()
		{
			return StageController.IsTransitioning();
		}

		#endregion

		#region Private

		private StageController StageController => stageController ??= GameBase.GameInstance.GetPresentationController<StageController>();
		private StageController stageController;

		#endregion
	}
}