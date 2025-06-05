using Midas.Core;
using Midas.Presentation.Data;

namespace Midas.Gaff.Utp
{
	public sealed partial class GleGaffModule
	{
		public sealed class UtpGaffStatus : StatusBlock
		{
			#region Fields

			private StatusProperty<IDialUpResults> gaffResults;
			private StatusProperty<bool> isWaiting;
			private StatusProperty<bool> isShowing;

			#endregion

			#region Properties

			public IDialUpResults GaffResults
			{
				get => gaffResults.Value;
				set => gaffResults.Value = value;
			}

			public bool IsWaiting
			{
				get => isWaiting.Value;
				set => isWaiting.Value = value;
			}

			public bool IsShowing
			{
				get => isShowing.Value;
				set => isShowing.Value = value;
			}

			#endregion

			public UtpGaffStatus() : base(nameof(UtpGaffStatus))
			{
			}

			protected override void DoResetProperties()
			{
				base.DoResetProperties();
				gaffResults = AddProperty<IDialUpResults>(nameof(GaffResults), null);
				isWaiting = AddProperty(nameof(IsWaiting), false);
				isShowing = AddProperty(nameof(IsShowing), false);
			}
		}
	}
}