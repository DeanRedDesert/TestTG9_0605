using System;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	/// <summary>
	/// Reel container extension methods.
	/// </summary>
	public static class ReelContainerExtensions
	{
		private static Reel GetReel(this ReelContainer reelContainer, int reelIndex)
		{
			if (reelIndex < 0 || reelIndex >= reelContainer.ReelCount)
			{
				Debug.LogError("No reel at index " + reelIndex);
				return null;
			}

			return reelContainer.Reels[reelIndex];
		}

		/// <summary>
		/// Gets the number of symbols on a reel.
		/// </summary>
		/// <param name="reelContainer">The reel index.</param>
		/// <param name="reelIndex">The reel index.</param>
		/// <param name="onlyVisibleSymbols">Only request visible symbols.</param>
		/// <returns>The number of symbols on the requested reel.</returns>
		public static int GetSymbolCount(this ReelContainer reelContainer, int reelIndex, bool onlyVisibleSymbols = true)
		{
			var reel = reelContainer.GetReel(reelIndex);
			if (!reel)
				return -1;

			return onlyVisibleSymbols ? reel.VisibleSymbols : reel.TotalSymbolCount;
		}

		/// <summary>
		/// Gets the size of a symbol with respect to the reel container parent transform.
		/// </summary>
		/// <param name="reelIndex">The reel index.</param>
		/// <param name="symbolIndex">The symbol index.</param>
		/// <returns>The size of the symbol.</returns>
		public static Vector2 GetSymbolSize(this ReelContainer reelContainer, int reelIndex, int symbolIndex)
		{
			var reel = reelContainer.GetReel(reelIndex);
			if (!reel)
				return Vector2.zero;

			if (symbolIndex >= reel.VisibleSymbols)
			{
				Debug.LogError("No symbol at reel " + reelIndex + " symbol " + symbolIndex);
				return Vector2.zero;
			}

			return reelContainer.GetSymbolSizeByCell(reel.Row + symbolIndex, reel.Column);
		}

		/// <summary>
		/// Gets the location of a symbol with respect to the reel container parent transform.
		/// </summary>
		/// <param name="reelIndex">The reel index.</param>
		/// <param name="symbolIndex">The symbol index.</param>
		/// <returns>The location of the symbol.</returns>
		public static Vector3 GetSymbolLocation(this ReelContainer reelContainer, int reelIndex, int symbolIndex)
		{
			var reel = reelContainer.GetReel(reelIndex);
			if (!reel)
				return Vector3.zero;

			if (symbolIndex >= reel.VisibleSymbols)
			{
				Debug.LogError("No symbol at reel " + reelIndex + " symbol " + symbolIndex);
				return Vector3.zero;
			}

			return reelContainer.GetSymbolLocationByCell(reel.Row + symbolIndex, reel.Column);
		}

		/// <summary>
		/// Gets a symbol object from the reel.
		/// </summary>
		/// <param name="reelIndex">The reel index.</param>
		/// <param name="symbolIndex">The symbol index.</param>
		/// <param name="onlyVisibleSymbols">Set to false to get non visible symbols.</param>
		/// <returns>The requested symbol game object.</returns>
		public static GameObject GetSymbol(this ReelContainer reelContainer, int reelIndex, int symbolIndex, bool onlyVisibleSymbols = true)
		{
			var reel = reelContainer.GetReel(reelIndex);
			if (!reel)
				return null;

			if (symbolIndex >= reel.VisibleSymbols)
			{
				Debug.LogError("No symbol at reel " + reelIndex + " symbol " + symbolIndex);
				return null;
			}

			return reel.GetSymbol(symbolIndex, onlyVisibleSymbols);
		}

		/// <summary>
		/// Returns the component of Type T if the symbol has one attached, null if it doesn't.
		/// </summary>
		/// <typeparam name="T">The type of Component to retrieve.</typeparam>
		/// <param name="reelContainer">The reel container to take the symbol from.</param>
		/// <param name="row">The row to take the symbol from.</param>
		/// <param name="column">The column take the symbol from.</param>
		/// <returns>The component of Type type if the game object has one attached, null if it doesn't.</returns>
		public static T GetSymbolComponent<T>(this ReelContainer reelContainer, int row, int column) where T : class
		{
			var sym = reelContainer.GetSymbolByCell(row, column);
			return sym == null ? null : sym.GetComponent<T>();
		}

		/// <summary>
		/// Returns all components of Type T in the GameObject.
		/// </summary>
		/// <typeparam name="T">The type of Component to retrieve.</typeparam>
		/// <param name="reelContainer">The reel container to take the symbol from.</param>
		/// <param name="row">The row to take the symbol from.</param>
		/// <param name="column">The column take the symbol from.</param>
		/// <returns>All components of Type T in the GameObject.</returns>
		public static T[] GetSymbolComponents<T>(this ReelContainer reelContainer, int row, int column) where T : class
		{
			var sym = reelContainer.GetSymbolByCell(row, column);
			return sym == null ? Array.Empty<T>() : sym.GetComponents<T>();
		}

		/// <summary>
		/// Returns the component of Type T in the GameObject or any of its children using depth first search.
		/// </summary>
		/// <remarks>Only active components are returned.</remarks>
		/// <typeparam name="T">The type of Component to retrieve.</typeparam>
		/// <param name="reelContainer">The reel container to take the symbol from.</param>
		/// <param name="row">The row to take the symbol from.</param>
		/// <param name="column">The column take the symbol from.</param>
		/// <returns>The component of Type T in the GameObject or any of its children using depth first search.</returns>
		public static T GetSymbolComponentInChildren<T>(this ReelContainer reelContainer, int row, int column) where T : class
		{
			var sym = reelContainer.GetSymbolByCell(row, column);
			return sym == null ? null : sym.GetComponentInChildren<T>();
		}

		/// <summary>
		/// Returns all components of Type T in the GameObject or any of its children.
		/// </summary>
		/// <typeparam name="T">The type of Component to retrieve.</typeparam>
		/// <param name="reelContainer">The reel container to take the symbol from.</param>
		/// <param name="row">The row to take the symbol from.</param>
		/// <param name="column">The column take the symbol from.</param>
		/// <param name="includeInactive">Should inactive Components be included in the found set?</param>
		/// <returns>All components of Type T in the GameObject or any of its children.</returns>
		public static T[] GetSymbolComponentsInChildren<T>(this ReelContainer reelContainer, int row, int column, bool includeInactive = false) where T : class
		{
			var sym = reelContainer.GetSymbolByCell(row, column);
			return sym == null ? Array.Empty<T>() : sym.GetComponentsInChildren<T>(includeInactive);
		}
	}
}