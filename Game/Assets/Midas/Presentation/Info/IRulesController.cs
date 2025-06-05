using System.Collections.Generic;

namespace Midas.Presentation.Info
{
	public enum RulesPageType
	{
		Rules,
		Paytable
	}

	public interface IRulesController
	{
		IReadOnlyList<RulesPageType> Setup();
	}
}