namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	/// Button function base values. Ensure there is at least 100 gap between each function base.
	/// </summary>
	/// <remarks>GameSpecificBase is the reserved area for game specific button functions. Starts at 1 because 0 is the Undefined button ID.</remarks>
	public enum ButtonFunctions
	{
		GameSpecificBase = 1,
		StakeBase = 100,
		AncillaryGameBase = 200,
		DashboardBase = 300,
		GambleFeatureBase = 400,
		HistoryBase = 500,
		GaffBase = 600,
		CreditPlayoffBase = 700,
		DenomBase = 800,
		InfoBase = 900,
		AutoplayBase = 1000,
		LanguageBase = 1100
	}
}