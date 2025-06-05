using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	[CreateAssetMenu(menuName = "Midas/Symbols/StandardSymbolBuilder")]
	public sealed class StandardSymbolBuilder : ReelSymbolBuilder
	{
		private Dictionary<string, ReelSymbol> symbolDictionary;

		[SerializeField]
		private List<ReelSymbol> symbols = new List<ReelSymbol>();

		public override bool CanConstructSymbol(string symbolId) => GetSymbolDictionary().ContainsKey(symbolId);

		public override ReelSymbol ConstructSymbol(string symbolId)
		{
			GetSymbolDictionary().TryGetValue(symbolId, out var result);

			if (result == null)
				return null;

			// Create an instance of the symbol.

			result = Instantiate(result, Vector3.zero, Quaternion.identity);
			result.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);

			return result;
		}

		private Dictionary<string, ReelSymbol> GetSymbolDictionary() => symbolDictionary ??= symbols == null ? new Dictionary<string, ReelSymbol>() : symbols.ToDictionary(s => s.SymbolId, s => s);
	}
}