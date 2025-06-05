using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Represents the identifiers for game button type that may be requested, as defined in the GameFunctionStatus, v1.0.
	/// </summary>
	public enum GameButtonTypeEnum
	{
		/// <summary>
		/// Collect button in the game.
		/// </summary>
		Collect,

		/// <summary>
		/// Volume button in the game.
		/// </summary>
		Volume,

		/// <summary>
		/// DenominationSelection button in the game.
		/// </summary>
		DenominationSelection,

		/// <summary>
		/// Bets button in the game and including all bet related buttons like the one in the DPP.
		/// </summary>
		Bets,

		/// <summary>
		/// button in the game.
		/// </summary>
		Reserve,

		/// <summary>
		/// Info button in the game.
		/// </summary>
		Info,

		/// <summary>
		/// More games button in the game
		/// </summary>
		MoreGames,
	}
}
