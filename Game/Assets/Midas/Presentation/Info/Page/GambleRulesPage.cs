using Midas.Presentation.Data;

namespace Midas.Presentation.Info.Page
{
	public sealed class GambleRulesPage : RulesPage
	{
		public override bool CanEnable() => StatusDatabase.ConfigurationStatus.AncillaryConfig.Enabled;
	}
}