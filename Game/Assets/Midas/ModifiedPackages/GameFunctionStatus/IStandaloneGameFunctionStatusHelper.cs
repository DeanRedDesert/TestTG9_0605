using System.Collections.Generic;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Provides functionality to manually configure a standalone implementation of <see cref="IGameFunctionStatus"/>>
	/// </summary>
	public interface IStandaloneGameFunctionStatusHelper
	{
		/// <summary>
		/// Sets the active Game Function Status information for the instance
		/// </summary>
		/// <param name="denomMenuConfiguration">The configuration parameters for the denomination menu</param>
		/// <param name="denomPlayableStatusList">The configuration parameters for the denomination playable status list.</param>
		/// <param name="gameButtonBehaviorList">The configuration parameters for the game button behavior list.</param>
		void SetGameFunctionStatusConfiguration(DenominationMenuTimeoutConfiguration denomMenuConfiguration, IEnumerable<DenominationPlayableStatus> denomPlayableStatusList, IEnumerable<GameButtonBehavior> gameButtonBehaviorList);
	}
}
