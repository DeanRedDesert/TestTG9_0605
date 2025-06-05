using System.Collections.Generic;
using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class PlayerSessionInitiateResetMessage : IMessage
	{
		public IReadOnlyList<PlayerSessionParameterType> ParametersToReset { get; }

		public PlayerSessionInitiateResetMessage(IReadOnlyList<PlayerSessionParameterType> parametersToReset) => ParametersToReset = parametersToReset;
	}
}