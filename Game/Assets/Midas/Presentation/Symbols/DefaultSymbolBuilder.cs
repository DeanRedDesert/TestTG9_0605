using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	[CreateAssetMenu(menuName = "Midas/Symbols/DefaultSymbolBuilder")]
	public sealed class DefaultSymbolBuilder : ReelSymbolBuilder
	{
		private IReadOnlyDictionary<string, Sprite> symbolSpriteDict;

		[SerializeField]
		[Tooltip("Symbol prefab is required to have a sprite renderer somewhere in it")]
		private ReelSymbol symbolPrefab;

		[SerializeField]
		private Sprite[] symbolSprites;

		[SerializeField]
		private FallbackSymbolOverlay fallbackSymbol;

		private void OnEnable()
		{
			if (!Application.isPlaying)
				return;

			symbolSpriteDict ??= symbolSprites == null ? new Dictionary<string, Sprite>() : symbolSprites.ToDictionary(s => s.name, s => s);
		}

		public override bool CanConstructSymbol(string symbolId) => true;

		public override ReelSymbol ConstructSymbol(string symbolId)
		{
			if (!GetSymbolSpriteDictionary().TryGetValue(symbolId, out var sprite))
				return ConstructFallback();

			// Create an instance of the symbol.

			var result = Instantiate(symbolPrefab, Vector3.zero, Quaternion.identity);
			result.gameObject.name = symbolId;
			result.SymbolId = symbolId;
			result.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);
			result.GetComponentInChildren<SpriteRenderer>().sprite = sprite;

			return result;

			ReelSymbol ConstructFallback()
			{
				var go = new GameObject(symbolId);
				go.SetHideFlagsRecursively(HideFlags.DontSave);
				var symbol = go.AddComponent<ReelSymbol>();
				symbol.SymbolId = symbolId;
				var fallback = symbol.gameObject.InstantiatePreFabAsChild(fallbackSymbol);
				fallback.SetSymbolId(symbol.SymbolId);
				return symbol;
			}
		}

		private IReadOnlyDictionary<string, Sprite> GetSymbolSpriteDictionary() => symbolSpriteDict ??= symbolSprites == null ? new Dictionary<string, Sprite>() : symbolSprites.ToDictionary(s => s.name, s => s);

#if UNITY_EDITOR

		public void ConfigureForMakeGame(ReelSymbol prefab, IReadOnlyList<Sprite> sprites, FallbackSymbolOverlay fallback)
		{
			symbolPrefab = prefab;
			symbolSprites = sprites.ToArray();
			fallbackSymbol = fallback;
		}

#endif
	}
}