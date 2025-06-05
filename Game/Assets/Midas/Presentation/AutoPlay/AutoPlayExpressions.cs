using System.Diagnostics.CodeAnalysis;
using Midas.Presentation.Data;

namespace Midas.Presentation.AutoPlay
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class AutoPlayExpressions
	{
		[Expression("AutoPlay")]
		public static bool IsPlayerSelectableAutoplayAllowed => StatusDatabase.ConfigurationStatus.GameConfig?.IsPlayerSelectableAutoplayAllowed == true;

		[Expression("AutoPlay")]
		public static bool IsAutoPlayConfirmationRequired => StatusDatabase.ConfigurationStatus.GameConfig?.IsAutoPlayConfirmationRequired == true;

		[Expression("AutoPlay")]
		public static bool IsAutoPlayChangeGameSpeedAllowed => StatusDatabase.ConfigurationStatus.GameConfig?.IsAutoPlayChangeSpeedAllowed == true;

		[Expression("AutoPlay")]
		public static bool IsWaitingForConfirm => StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm;
	}
}