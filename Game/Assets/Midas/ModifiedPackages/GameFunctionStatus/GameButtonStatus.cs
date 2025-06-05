// <copyright file = "GameButtonStatus.cs" company = "IGT">
//     Copyright (c) 2024 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Represents the identifiers for game button status that may be requested, as defined in the GameFunctionStatus, v1.0.
	/// </summary>
	public enum GameButtonStatus
	{
		/// <summary>
		/// Game button is active and selectable.
		/// </summary>
		Active,

		/// <summary>
		/// Game button is not selectable but still shown on the select screen.
		/// </summary>
		SoftDisabled,

		/// <summary>
		/// Game button is not selectable and (TBD) hidden from the select screen but it is up to the game.
		/// </summary>
		Hidden,

		/// <summary>
		/// Unknown, invalid or uninitialised status. Game Will follows the original/current status/behavior of the game if requested and it could be ignored depending on the status of the game.
		/// </summary>
		Invalid,
	}
}