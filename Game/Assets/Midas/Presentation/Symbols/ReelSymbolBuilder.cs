using UnityEngine;

namespace Midas.Presentation.Symbols
{
	public abstract class ReelSymbolBuilder : ScriptableObject
	{
		/// <summary>
		/// Checks if the symbol ID passed in can be constructed with this template.
		/// </summary>
		public abstract bool CanConstructSymbol(string symbolId);

		/// <summary>
		/// Constructs the symbol into the given parent.
		/// </summary>
		public abstract ReelSymbol ConstructSymbol(string symbolId);

		/// <summary>
		/// Override this method if you need special handling for cleaning up your symbol.
		/// </summary>
		public virtual void DestroySymbol(ReelSymbol symbol)
		{
			Destroy(symbol.gameObject);
		}
	}
}