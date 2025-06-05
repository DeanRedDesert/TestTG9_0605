using System.Collections.Generic;
using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class PlayerSessionReportParametersBeingResetMessage : IMessage
	{
		public IReadOnlyList<PlayerSessionParameterType> ParametersBeingReset { get; }

		public PlayerSessionReportParametersBeingResetMessage(IReadOnlyList<PlayerSessionParameterType> parametersBeingReset) => ParametersBeingReset = parametersBeingReset;
	}
}