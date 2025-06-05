using Midas.Presentation.Data;

namespace Midas.Presentation.StageHandling
{
	public sealed class StageStatus : StatusBlock
	{
		private StatusProperty<Stage> currentStage;
		private StatusProperty<Stage> desiredStage;

		public Stage CurrentStage => currentStage.Value;

		public Stage DesiredStage => desiredStage.Value;

		public StageStatus()
			: base(nameof(StageStatus))
		{
		}

		public bool SetDesired(Stage stage)
		{
			var modified = desiredStage.SetValue(stage);
			return modified;
		}

		public bool SetDesiredAsCurrent()
		{
			var modified = currentStage.SetValue(desiredStage.Value);
			return modified;
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			currentStage = AddProperty(nameof(CurrentStage), Stage.Undefined);
			desiredStage = AddProperty(nameof(DesiredStage), Stage.Undefined);
		}
	}
}