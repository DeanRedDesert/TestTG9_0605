using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	public sealed class SymbolBuilderReelSymbolList : ReelSymbolList
	{
		[SerializeField]
		private ReelSymbolBuilder[] builders;

		protected override ReelSymbol ConstructSymbol(string symbolId, GameObject parent)
		{
			foreach (var builder in builders)
			{
				if (builder.CanConstructSymbol(symbolId))
				{
					var sym = builder.ConstructSymbol(symbolId);
					sym.gameObject.SetLayerRecursively(gameObject.layer);
					sym.transform.SetParent(parent.transform, false);
					return sym;
				}
			}

			// Symbol ID is unsupported.

			return null;
		}

		protected override void DestroySymbol(ReelSymbol symbol)
		{
			foreach (var template in builders)
			{
				if (template.CanConstructSymbol(symbol.SymbolId))
				{
					template.DestroySymbol(symbol);
				}
			}
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(params ReelSymbolBuilder[] newBuilders)
		{
			builders = newBuilders.ToArray();
		}
#endif
	}
}