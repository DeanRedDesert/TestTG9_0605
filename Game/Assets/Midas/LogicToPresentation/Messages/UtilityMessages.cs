using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class RequestUtilityDataMessage : IMessage
	{
	}

	public sealed class RequestUtilityDataResponse : IMessage
	{
		public IDialUpData GaffData { get; }

		public RequestUtilityDataResponse(IDialUpData gaffData) => GaffData = gaffData;
	}

	public sealed class UtilityResultsMessage : IMessage
	{
		public IDialUpResults UtilityResults { get; }

		public UtilityResultsMessage(IDialUpResults gaffResults) => UtilityResults = gaffResults;
	}
}