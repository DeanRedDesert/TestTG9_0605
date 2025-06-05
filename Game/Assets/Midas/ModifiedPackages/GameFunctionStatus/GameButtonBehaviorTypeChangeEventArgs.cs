using System;
using System.Collections.Generic;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Event arguments for Game Button Behavior being changed.
	/// </summary>
	[Serializable]
	public class GameButtonBehaviorTypeChangeEventArgs : EventArgs
	{
		#region Constructors

		/// <summary>
		///  The constructor for the event data of the game button behavior.
		/// </summary>
		/// <param name="gameButtonBehaviorTypes"></param>
		public GameButtonBehaviorTypeChangeEventArgs(IEnumerable<GameButtonBehavior> gameButtonBehaviorTypes)
		{
			GameButtonBehaviorTypes = gameButtonBehaviorTypes;
		}

		#endregion

		#region Properties

		/// <summary>
		/// List of game button behavior from the foundation.
		/// </summary>
		public IEnumerable<GameButtonBehavior> GameButtonBehaviorTypes { get; private set; }
		

		#endregion
	}
}
