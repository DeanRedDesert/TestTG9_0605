using Midas.Presentation.Data;
using Midas.Presentation.StageHandling;

namespace Game.GameMessages
{
	public sealed class GameMessageStatus : StatusBlock
	{
		private StatusProperty<bool> isGameInfoVisible;
		private StatusProperty<bool> isWinInfoVisible;
		private StatusProperty<int> activeGameInfoMessageIndex;

		public bool IsGameInfoVisible
		{
			get => isGameInfoVisible.Value;
			set => isGameInfoVisible.Value = value;
		}

		public bool IsWinInfoVisible
		{
			get => isWinInfoVisible.Value;
			set => isWinInfoVisible.Value = value;
		}

		public int ActiveGameInfoMessageIndex
		{
			get => activeGameInfoMessageIndex.Value;
			set => activeGameInfoMessageIndex.Value = value;
		}

		public GameMessageStatus(Stage stage) : base($"{stage.Name}{nameof(GameMessageStatus)}")
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			isGameInfoVisible = AddProperty(nameof(IsGameInfoVisible), false);
			isWinInfoVisible = AddProperty(nameof(IsWinInfoVisible), false);
			activeGameInfoMessageIndex = AddProperty(nameof(ActiveGameInfoMessageIndex), 0);
		}
	}
}