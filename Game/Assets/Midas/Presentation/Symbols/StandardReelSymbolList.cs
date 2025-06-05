using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	/// <summary>
	/// Basic implementation of the reel symbol list. Just provides simple symbols that appear in the symbol list with no special processing.
	/// </summary>
	public sealed class StandardReelSymbolList : ReelSymbolList
	{
		#region Fields

		private Dictionary<string, ReelSymbol> symbolDictionary;

		#endregion

		#region Inspector Fields

		/// <summary>
		/// The symbols that this symbol list supports.
		/// </summary>
		public List<ReelSymbol> Symbols = new List<ReelSymbol>();

		public FallbackSymbolOverlay FallbackSymbolOverlay;

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			symbolDictionary = Symbols == null ? new Dictionary<string, ReelSymbol>() : Symbols.ToDictionary(s => s.SymbolId, s => s);

			if (FallbackSymbolOverlay)
			{
				foreach (var symbol in symbolDictionary.Values)
				{
					if (symbol.GetComponentsInChildren<SpriteRenderer>().All(r => r.sprite == null && symbol.GetComponentsInChildren<MeshFilter>().All(f => f.mesh == null)))
					{
						var fallback = symbol.gameObject.InstantiatePreFabAsChild(FallbackSymbolOverlay);
						fallback.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);
						fallback.SetSymbolId(symbol.SymbolId);
					}
				}
			}
		}

		#endregion

		#region Overrides of ReelSymbolList

		/// <summary>
		/// Gets a symbol of the provided ID from the symbol list.
		/// </summary>
		/// <param name="symbolId">The symbol ID to retrieve.</param>
		/// <param name="parent">The parent transform for the symbol.</param>
		/// <returns>The ReelSymbol instance of the requested symbol ID, or null if the symbol ID is not supported.</returns>
		protected override ReelSymbol ConstructSymbol(string symbolId, GameObject parent)
		{
			ReelSymbol result;
#if UNITY_EDITOR
			if (symbolDictionary == null)
				result = Symbols.FirstOrDefault(s => s.SymbolId == symbolId);
			else
				symbolDictionary.TryGetValue(symbolId, out result);
#else
			symbolDictionary.TryGetValue(symbolId, out result);
#endif

			if (!result)
			{
				if (!FallbackSymbolOverlay)
					return null;

				result = ConstructFallback();
				symbolDictionary?.Add(symbolId, result);
			}

			// Create an instance of the symbol.

			result = parent.InstantiatePreFabAsChild(result);
			result.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);

			// Need to do this if we are constructing a fallback symbol.

			result.SymbolId = symbolId;

			return result;

			ReelSymbol ConstructFallback()
			{
				var go = new GameObject(symbolId);
				go.transform.SetParent(transform, false);
				go.SetHideFlagsRecursively(HideFlags.DontSave);
				var symbol = go.AddComponent<ReelSymbol>();
				symbol.SymbolId = symbolId;
				var fallback = symbol.gameObject.InstantiatePreFabAsChild(FallbackSymbolOverlay);
				fallback.SetSymbolId(symbol.SymbolId);
				return symbol;
			}
		}

		/// <summary>
		/// Return a symbol to the symbol list.
		/// </summary>
		/// <param name="symbol">The symbol to return.</param>
		protected override void DestroySymbol(ReelSymbol symbol)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyImmediate(symbol.gameObject);
			else
				Destroy(symbol.gameObject);
#else
			Destroy(symbol.gameObject);
#endif
		}

		#endregion
	}
}