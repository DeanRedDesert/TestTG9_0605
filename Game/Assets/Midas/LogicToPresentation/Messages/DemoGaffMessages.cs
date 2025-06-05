using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class DemoActivateGaffMessage : DebugMessage
	{
		public int GaffIndex { get; }

		public DemoActivateGaffMessage(int gaffIndex) => GaffIndex = gaffIndex;
	}

	public sealed class RequestGaffDataMessage : IMessage
	{
	}

	public sealed class RequestGaffDataResponse : IMessage
	{
		public IDialUpData DialUpData { get; }

		public RequestGaffDataResponse(IDialUpData dialUpData) => DialUpData = dialUpData;
	}

	public sealed class DemoGaffResultsMessage : DebugMessage
	{
		public IDialUpResults GaffResults { get; }

		public DemoGaffResultsMessage(IDialUpResults gaffResults) => GaffResults = gaffResults;
	}
}