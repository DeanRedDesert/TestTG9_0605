using System;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	/// <summary>
	/// Base functionality for a symbol on a reel.
	/// </summary>
	public class ReelSymbol : MonoBehaviour
	{
		#region Fields

		private IReelSymbolEventHandler[] eventHandlers;
		private Action<ReelSymbol> offReelCallback;

		#endregion

		#region Inspector Fields

		/// <summary>
		/// The symbol ID.
		/// </summary>
		public string SymbolId;

		#endregion

		#region Properties

		/// <summary>
		/// The reel that the symbol is on.
		/// </summary>
		public Reel Reel { get; private set; }

		/// <summary>
		/// Get whether the symbol is currently on a reel.
		/// </summary>
		public bool IsOnReel => Reel != null;

		/// <summary>
		/// The index on the reel that this symbol is sitting.
		/// </summary>
		public int IndexOnReel { get; private set; }

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			eventHandlers = gameObject.GetComponents<IReelSymbolEventHandler>();
		}

		#endregion

		#region Virtual Methods

		/// <summary>
		/// Called by the framework when the symbol goes on a reel.
		/// </summary>
		/// <remarks>
		/// This is the first thing done after the symbol has been parented to the symbol container game object on the reel, meaning that the
		/// reel itself does not have the symbol in its list and the symbol does not have its IndexOnReel value configured yet.
		/// </remarks>
		/// <param name="reel">The reel that the symbol was put on.</param>
		/// <param name="offReelCallback">This action is called when the symbol goes off the reel.</param>
		// ReSharper disable once ParameterHidesMember
		public virtual void SymbolOnReel(Reel reel, Action<ReelSymbol> offReelCallback)
		{
			if (this.offReelCallback != null)
			{
				Debug.LogError("Symbol is already on a reel");
				return;
			}

			this.offReelCallback = offReelCallback;
			Reel = reel;
			gameObject.SetActive(true);

			if (eventHandlers != null)
			{
				foreach (var eh in eventHandlers)
					eh.SymbolOnReel(this);
			}
		}

		/// <summary>
		/// Take the symbol off the reel. This calls the off reel callback if one was provided in the SymbolOnReel call.
		/// </summary>
		public virtual void SymbolOffReel()
		{
			if (!IsOnReel)
			{
				Debug.LogWarning("Symbol was not on a reel before calling SymbolOffReel");
			}

			if (eventHandlers != null)
			{
				foreach (var eh in eventHandlers)
					eh.SymbolOffReel(this);
			}

			gameObject.SetActive(false);
			if (offReelCallback != null)
			{
				offReelCallback(this);
				offReelCallback = null;
			}

			Reel = null;
		}

		/// <summary>
		/// Called when the symbol changes index on the reel.
		/// </summary>
		/// <remarks>
		/// When a symbol is first added to a reel, this call will happen immediately after the call to SymbolOnReel.
		/// </remarks>
		/// <param name="index">The reel index.</param>
		public virtual void SetIndexOnReel(int index)
		{
			IndexOnReel = index;

			if (eventHandlers != null)
			{
				foreach (var eh in eventHandlers)
					eh.SymbolPositionChanged(this);
			}
		}

		#endregion
	}
}