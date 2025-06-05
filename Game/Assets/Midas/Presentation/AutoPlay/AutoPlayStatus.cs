using Midas.Presentation.Data;

namespace Midas.Presentation.AutoPlay
{
	public sealed class AutoPlayStatus : StatusBlock
	{
		private StatusProperty<AutoPlayState> state;
		private StatusProperty<bool> shouldStopAutoplayInFeature;

		public AutoPlayState State
		{
			get => state.Value;
			set => state.Value = value;
		}

		public bool ShouldStopAutoplayInFeature
		{
			get => shouldStopAutoplayInFeature.Value;
			set => shouldStopAutoplayInFeature.Value = value;
		}

		public AutoPlayStatus() : base(nameof(AutoPlayStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			state = AddProperty(nameof(State), AutoPlayState.Idle);
			shouldStopAutoplayInFeature = AddProperty(nameof(ShouldStopAutoplayInFeature), true);
		}
	}
}