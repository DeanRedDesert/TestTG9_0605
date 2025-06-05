using System;
using System.Collections;
using System.Collections.Generic;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Symbols;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	/// <summary>
	/// Basic reel implementation.
	/// </summary>
	public class Reel : MonoBehaviour
	{
		#region Fields

		private readonly List<ReelSymbol> symbols = new List<ReelSymbol>();
		private GameObject symbolContainer;
		private ReelSpin reelSpin;
		private bool immediateStop;

		#endregion

		#region Inspector Fields

		/// <summary>
		/// The row that the first symbol on this reel appears in.
		/// </summary>
		[Header("Cell Location")]
		[SerializeField]
		private int row;

		/// <summary>
		/// The column that this reel appears in.
		/// </summary>
		[SerializeField]
		private int column;

		/// <summary>
		/// The number of visible symbols in the window.
		/// </summary>
		[Header("Reel Layout")]
		[SerializeField]
		[Tooltip("The number of visible symbols in the window.")]
		private int visibleSymbols;

		/// <summary>
		/// The number of symbols to draw above the window.
		/// </summary>
		[SerializeField]
		[Tooltip("The number of symbols to draw above the window.")]
		private int symbolsAbove;

		/// <summary>
		/// The number of symbols to draw below the window.
		/// </summary>
		[SerializeField]
		[Tooltip("The number of symbols to draw below the window.")]
		private int symbolsBelow;

		/// <summary>
		/// The size of a symbol.
		/// </summary>
		[Tooltip("The size of a symbol.")]
		//[CustomVectorLabelAttribute(new[] { "Width", "Height" })]
		public Vector2 SymbolSize;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the spin state of the reel.
		/// </summary>
		public ReelSpinState SpinState { get; private set; }

		/// <summary>
		/// The row that the first symbol on this reel appears in.
		/// </summary>
		public int Row
		{
			get => row;
			set => row = value;
		}

		/// <summary>
		/// The column that this reel appears in.
		/// </summary>
		public int Column
		{
			get => column;
			set => column = value;
		}

		/// <summary>
		/// Gets the reel group that this reel is in.
		/// </summary>
		public int Group { get; set; }

		/// <summary>
		/// Gets the number of visible symbols on the reel.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWhenPossible - Don't convert unity inspector fields
		public int VisibleSymbols
		{
			get => visibleSymbols;
			set => visibleSymbols = value;
		}

		/// <summary>
		/// Gets the number of symbols above the visible area of the reel.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWhenPossible - Don't convert unity inspector fields
		public int SymbolsAbove
		{
			get => symbolsAbove;
			set => symbolsAbove = value;
		}

		/// <summary>
		/// Gets the number of symbols below the visible area of the reel.
		/// </summary>
		// ReSharper disable once ConvertToAutoProperty - Don't convert unity inspector fields
		public int SymbolsBelow
		{
			get => symbolsBelow;
			set => symbolsBelow = value;
		}

		public int TotalSymbolCount => VisibleSymbols + SymbolsAbove + symbolsBelow;

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			// Make sure that the reel is empty before we start using it.

			for (var i = transform.childCount - 1; i >= 0; --i)
				Destroy(transform.GetChild(i).gameObject);
			transform.DetachChildren();

			SpinState = ReelSpinState.Idle;
			symbols.Clear();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Initialise the reel, reset symbols and stop any spin.
		/// </summary>
		public void Initialise(ReelStrip reelStrip, ReelSymbolList symbolList)
		{
			// Initialise the symbol container.

			if (symbolContainer == null)
			{
				symbolContainer = new GameObject("Symbol Container");
				symbolContainer.layer = gameObject.layer;
				symbolContainer.hideFlags = HideFlags.DontSave;
				symbolContainer.transform.SetParent(transform, false);
			}

			symbolContainer.transform.localPosition = Vector3.zero;

			// Stop the spin if it is in progress.

			StopAllCoroutines();
			SpinState = ReelSpinState.Idle;
			reelSpin = null;

			// Empty the symbol list and return them.

			foreach (var symbol in symbols)
			{
				if (symbol != null)
					symbol.SymbolOffReel();
			}

			symbols.Clear();

			// Leave the reel empty if there are no strips or symbols.

			if (reelStrip == null || symbolList == null)
				return;

			// Strip symbols come out of the factory in the order that they would spin past.

			for (var i = 0; i < TotalSymbolCount; i++)
			{
				PlaceOnReel(reelStrip.GetSymbol(VisibleSymbols + SymbolsBelow - (i + 1)), symbolList);
			}
		}

		/// <summary>
		/// Spin the reels.
		/// </summary>
		public void Spin(ReelStrip reelStrip, TimeSpan duration, SpinSettings spinSettings, ReelSymbolList symbolList, Action<Reel> reelStateChangedCallback)
		{
			if (symbolContainer == null)
			{
				Debug.LogError("Reel is not initialised");
				return;
			}

			if (SpinState != ReelSpinState.Idle)
			{
				Debug.LogError("Reel is already spinning");
				return;
			}

			immediateStop = false;
			reelSpin = new ReelSpin(spinSettings, reelStrip, duration, this);
			StartCoroutine(DoSpin(symbolList, reelStateChangedCallback));
		}

		/// <summary>
		/// Get the symbol at the provided index.
		/// </summary>
		/// <param name="symbolIndex">The index of the symbol to get.</param>
		/// <param name="onlyVisibleSymbols">Only get visible symbols.</param>
		/// <returns>The game object of the requested symbol.</returns>
		public GameObject GetSymbol(int symbolIndex, bool onlyVisibleSymbols = true)
		{
			if (symbolIndex < 0 || onlyVisibleSymbols && symbolIndex >= VisibleSymbols || !onlyVisibleSymbols && symbolIndex >= TotalSymbolCount)
			{
				Debug.LogError("No symbol found at index " + symbolIndex);
				return null;
			}

			if (onlyVisibleSymbols)
				symbolIndex += SymbolsAbove;

			return symbolIndex < symbols.Count && symbols[symbolIndex] != null ? symbols[symbolIndex].gameObject : null;
		}

		/// <summary>
		/// Get the symbol position relative to the parent object.
		/// </summary>
		/// <param name="symbolIndex">The symbol index.</param>
		/// <returns>The symbol position.</returns>
		public Vector3 GetSymbolPosition(int symbolIndex)
		{
			var pos = new Vector3(0, ((visibleSymbols - 1) / 2f - symbolIndex) * SymbolSize.y, 0);
			pos += transform.localPosition;
			pos.Scale(transform.localScale);
			return pos;
		}

		/// <summary>
		/// Gets the world position of the requested symbol.
		/// </summary>
		/// <param name="symbolIndex">The index on the reel of the symbol.</param>
		/// <returns>The world position of the symbol.</returns>
		public Vector3 GetSymbolWorldPosition(int symbolIndex)
		{
			return transform.TransformPoint(new Vector3(0, ((visibleSymbols - 1) / 2f - symbolIndex) * SymbolSize.y, 0));
		}

		/// <summary>
		/// Get the symbol size relative to the parent transform.
		/// </summary>
		/// <param name="symbolIndex">The symbol index.</param>
		/// <returns>The symbol size relative to the parent transform.</returns>
		public Vector2 GetSymbolSize(int symbolIndex)
		{
			return new Vector2(SymbolSize.x * transform.localScale.x, SymbolSize.y * transform.localScale.y);
		}

		/// <summary>
		/// Request the reel to stop as soon as possible.
		/// </summary>
		public void Slam(bool immediate)
		{
			reelSpin?.Slam();
			immediateStop = immediate;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Place a symbol on the reel.
		/// </summary>
		/// <param name="symbolId">The ID of the symbol to place</param>
		/// <param name="symbolList">The list of symbols to pull the symbol out of.</param>
		private void PlaceOnReel(string symbolId, ReelSymbolList symbolList)
		{
			var yVal = ((visibleSymbols - 1) / 2f + symbolsAbove) * SymbolSize.y;
			var symbol = symbolList.GetSymbol(symbolId);
			if (symbol != null)
			{
				symbol.transform.SetParent(symbolContainer.transform, false);
				symbol.transform.localPosition = new Vector3(0, yVal, 0);
				symbol.transform.localRotation = Quaternion.identity;
				symbol.SymbolOnReel(this, symbolList.ReturnSymbol);
				symbol.SetIndexOnReel(-symbolsAbove);
			}

			symbols.Insert(0, symbol);

			while (symbols.Count > TotalSymbolCount)
			{
				symbol = symbols[symbols.Count - 1];
				symbols.RemoveAt(symbols.Count - 1);

				if (symbol != null)
					symbol.SymbolOffReel();
			}

			for (var i = 1; i < symbols.Count; i++)
			{
				yVal -= SymbolSize.y;
				symbol = symbols[i];
				if (symbol != null)
				{
					symbol.transform.SetLocalPosY(yVal);
					symbol.SetIndexOnReel(symbol.IndexOnReel + 1);
				}
			}
		}

		/// <summary>
		/// Coroutine that does all the reel spin handling.
		/// </summary>
		private IEnumerator DoSpin(ReelSymbolList symbolList, Action<Reel> reelStateChangedCallback)
		{
			// Start with the wind-up if there is one.

			float yVal;
			SpinState = ReelSpinState.WindingUp;
			reelStateChangedCallback?.Invoke(this);

			if (reelSpin.Settings.WindupTime > 0)
			{
				var t = reelSpin.Settings.WindupTime - Time.smoothDeltaTime;
				while (t > 0)
				{
					yVal = SymbolSize.y * reelSpin.Settings.GetWindupPosition(1 - t / reelSpin.Settings.WindupTime);
					symbolContainer.transform.SetLocalPosY(yVal);
					yield return null;
					t -= Time.smoothDeltaTime;
				}
			}

			// Start with the spin.

			SpinState = ReelSpinState.Spinning;
			reelStateChangedCallback?.Invoke(this);

			var spinDelay = reelSpin.Settings.SpinFrameRate == 0 ? default(float?) : 1f / reelSpin.Settings.SpinFrameRate;
			var nextSymbol = reelSpin.GetNextSymbol();
			yVal = reelSpin.Settings.WindupDistance - SymbolSize.y * Time.smoothDeltaTime * reelSpin.SpinSpeed;
			while (nextSymbol != null)
			{
				while (Math.Abs(yVal) >= SymbolSize.y && nextSymbol != null)
				{
					yVal += SymbolSize.y;
					PlaceOnReel(nextSymbol, symbolList);
					nextSymbol = reelSpin.GetNextSymbol();
				}

				if (nextSymbol != null)
				{
					if (immediateStop)
					{
						do
						{
							PlaceOnReel(nextSymbol, symbolList);
							nextSymbol = reelSpin.GetNextSymbol();
						} while (nextSymbol != null);
					}
					else
					{
						symbolContainer.transform.SetLocalPosY(yVal);
						if (spinDelay.HasValue)
						{
							while (spinDelay > 0f)
							{
								yield return null;
								yVal -= SymbolSize.y * Time.smoothDeltaTime * reelSpin.SpinSpeed;
								spinDelay -= Time.smoothDeltaTime;
							}

							spinDelay += 1f / reelSpin.Settings.SpinFrameRate;
						}
						else
						{
							yield return null;
							yVal -= SymbolSize.y * Time.smoothDeltaTime * reelSpin.SpinSpeed;
						}
					}
				}
			}

			// Then the overshoot.

			SpinState = ReelSpinState.Overshooting;
			reelStateChangedCallback?.Invoke(this);

			while (Math.Abs(yVal) < SymbolSize.y * reelSpin.Settings.OvershootDistance)
			{
				symbolContainer.transform.SetLocalPosY(yVal);
				yield return null;

				yVal -= SymbolSize.y * Time.smoothDeltaTime * reelSpin.SpinSpeed;
			}

			// Finally recovery.

			SpinState = ReelSpinState.Recovering;
			reelStateChangedCallback?.Invoke(this);

			if (reelSpin.Settings.RecoveryTime > 0)
			{
				var t = reelSpin.Settings.RecoveryTime - Time.smoothDeltaTime;
				while (t > 0)
				{
					yVal = -SymbolSize.y * reelSpin.Settings.GetRecoveryPosition(1 - t / reelSpin.Settings.RecoveryTime);
					symbolContainer.transform.SetLocalPosY(yVal);
					yield return null;
					t -= Time.smoothDeltaTime;
				}
			}

			symbolContainer.transform.SetLocalPosY(0);

			SpinState = ReelSpinState.Idle;
			reelStateChangedCallback?.Invoke(this);
			reelSpin = null;
		}

		#endregion
	}
}