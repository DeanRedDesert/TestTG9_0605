using System;
using System.Collections.Generic;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Provides functionality for handling requests from the foundation to control the denomination selection menu
	/// </summary>
	public interface IGameFunctionStatus
	{
		#region Events

		/// <summary>
		/// An event that is raised when the foundation has requested a new denomination menu timeout
		/// </summary>
		/// <remarks>The bin should modify the event args with values indicating a success or not</remarks>
		event EventHandler<DenominationMenuControlSetTimeoutEventArgs> OnReceivedDenominationMenuTimeout;

		/// <summary>
		/// An event that is raised when the foundation has requested to change the denomination playable status.
		/// </summary>
		/// <remarks>The bin should modify the event args with values indicating a success or not</remarks>
		event EventHandler<DenominationPlayableStatusChangeEventArgs> OnReceivedDenominationPlayableStatus;

		/// <summary>
		/// An event that is raised when the foundation has requested to change the game button status.
		/// </summary>
		/// <remarks>The bin should modify the event args with values indicating a success or not</remarks>
		event EventHandler<GameButtonBehaviorTypeChangeEventArgs> OnReceivedGameButtonBehaviorType;

		#endregion

		#region Methods

		/// <summary>
		/// Queries the foundation for the current denomination menu timeout information
		/// </summary>
		/// <returns></returns>
		DenominationMenuTimeoutConfiguration GetDenominationMenuTimeoutInformation();

		/// <summary>
		/// Queries the foundation for the current game button status
		/// </summary>
		/// <returns></returns>
		IEnumerable<GameButtonBehavior> GetGameButtonStatus();

		/// <summary>
		/// Queries the foundation for the current denomination playable status.
		/// </summary>
		/// <returns></returns>
		IEnumerable<DenominationPlayableStatus> GetDenominationPlayableStatus();

		#endregion

	}
}
