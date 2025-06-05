using System.Diagnostics.CodeAnalysis;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.Presentation.Data;

namespace Midas.CreditPlayoff.Presentation
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class CreditPlayoffExpressions
	{
		private const string CreditPlayoffCategoryName = "CreditPlayoff";
		private static CreditPlayoffStatus creditPlayoffStatus;

		internal static void Init(CreditPlayoffStatus credPlayStatus) => creditPlayoffStatus = credPlayStatus;
		internal static void DeInit() => creditPlayoffStatus = null;

		[Expression(CreditPlayoffCategoryName)]
		public static bool FeatureAvailable => creditPlayoffStatus != null && creditPlayoffStatus.State != CreditPlayoffState.Unavailable;
	}
}