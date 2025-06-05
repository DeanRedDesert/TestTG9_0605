using System.Text.RegularExpressions;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	[CreateAssetMenu(menuName = "Midas/Symbols/TemplateSymbolBuilder")]
	public sealed class TemplateSymbolBuilder : ReelSymbolBuilder
	{
		private Regex regex;

		[SerializeField]
		[Tooltip("Regular expression to match symbol names this template is for.")]
		private string symbolIdRegex;

		[SerializeField]
		[Tooltip("Symbol template prefab")]
		private ReelSymbol symbolPrefab;

		public override bool CanConstructSymbol(string symbolId) => GetRegex().IsMatch(symbolId);

		public override ReelSymbol ConstructSymbol(string symbolId)
		{
			// Create an instance of the symbol.

			var result = Instantiate(symbolPrefab, Vector3.zero, Quaternion.identity);
			result.gameObject.name = symbolId;
			result.SymbolId = symbolId;
			result.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);

			return result;
		}

		private Regex GetRegex() => regex ??= new Regex(symbolIdRegex, RegexOptions.Compiled);
	}
}