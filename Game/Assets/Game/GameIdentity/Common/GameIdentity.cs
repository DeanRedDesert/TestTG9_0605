using System.Collections.Generic;
using Game.GameIdentity.Anz;
using Game.GameIdentity.Global;
using Midas.Core;
using Midas.Presentation.Game;

namespace Game.GameIdentity.Common
{
	public static class GameIdentity
	{
		public static IReadOnlyList<IPresentationController> GetPresentationControllers()
		{
			// ANZ and ANZ hybrid use the same GI configuration.

			return new IPresentationController[]
			{
				new CommonSequences(),
				new GiAnzConfig(GameIdentityType.Anz),
				new GiAnzConfig(GameIdentityType.AnzHybrid),
				new GiGlobalConfig()
			};
		}
	}
}