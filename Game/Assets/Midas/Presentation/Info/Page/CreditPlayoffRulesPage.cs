using Midas.Presentation.Data;

namespace Midas.Presentation.Info.Page
{
	public sealed class CreditPlayoffRulesPage : RulesPage
	{
		public override bool CanEnable() => StatusDatabase.ConfigurationStatus.GameConfig.IsCreditPlayoffEnabled;
	}
}