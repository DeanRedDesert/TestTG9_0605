using Midas.Presentation.Data.PropertyReference;

namespace Midas.Presentation.Dashboard
{
	public enum ChooseConfigState
	{
		None,
		Denom,
		MoreGames
	}

	public sealed class ChooseConfigStatePropUnityEvents : PropertyRefEnumUnityEvent<ChooseConfigState>
	{
	}
}