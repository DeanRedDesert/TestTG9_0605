using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	/// <summary>
	/// Abstract base of all symbol list types.
	/// </summary>
	public abstract class ReelSymbolList : MonoBehaviour
	{
		#region Fields

		private GameObject pool;
		private readonly Dictionary<string, Stack<ReelSymbol>> pooledSymbols = new Dictionary<string, Stack<ReelSymbol>>();

		#endregion

		/// <summary>
		/// Gets a symbol of the provided ID from the symbol list.
		/// </summary>
		/// <param name="symbolId">The symbol ID to retrieve.</param>
		/// <returns>The ReelSymbol instance of the requested symbol ID, or null if the symbol ID is not supported.</returns>
		public ReelSymbol GetSymbol(string symbolId)
		{
			// Check if we have any symbols available in the pool.

			if (!pooledSymbols.TryGetValue(symbolId, out var availableSymbols) || availableSymbols.Count == 0)
			{
				// Instantiate from the base symbol list since we don't have a symbol available.

				if (!pool)
				{
					pool = new GameObject("Pool");
					pool.transform.SetParent(transform, false);
					pool.SetActive(false);
				}

				var symbol = ConstructSymbol(symbolId, pool);
				if (symbol)
					symbol.gameObject.SetActive(false);
				return symbol;
			}

			// Remove a symbol from the pool.

			return availableSymbols.Pop();
		}

		/// <summary>
		/// Return a symbol to the symbol list.
		/// </summary>
		/// <param name="symbol">The symbol to return.</param>
		public void ReturnSymbol(ReelSymbol symbol)
		{
			if (!Application.isPlaying)
			{
				// In editor mode, delete the symbol.

				DestroySymbol(symbol);
				return;
			}

			// Get or create a new stack to store the new symbol.

			if (!pooledSymbols.TryGetValue(symbol.SymbolId, out var availableSymbols))
			{
				availableSymbols = new Stack<ReelSymbol>();
				pooledSymbols[symbol.SymbolId] = availableSymbols;
			}

			// Deactivate the symbol and put it in the pool.

			var t = symbol.transform;
			t.SetParent(pool.transform, false);
			t.localPosition = Vector3.zero;
			symbol.gameObject.SetActive(false);
			availableSymbols.Push(symbol);
		}

		/// <summary>
		/// Constructs a symbol of the provided ID.
		/// </summary>
		/// <param name="symbolId">The symbol ID to construct.</param>
		/// <param name="parent">The parent game object for the symbol.</param>
		/// <returns>The ReelSymbol instance of the requested symbol ID, or null if the symbol ID is not supported.</returns>
		protected abstract ReelSymbol ConstructSymbol(string symbolId, GameObject parent);

		/// <summary>
		/// Destroy a symbol.
		/// </summary>
		/// <param name="symbol">The symbol to destroy.</param>
		protected abstract void DestroySymbol(ReelSymbol symbol);
	}
}