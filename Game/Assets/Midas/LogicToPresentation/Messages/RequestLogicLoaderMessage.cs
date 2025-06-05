using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class RequestLogicLoaderMessage : IMessage
	{
	}

	public sealed class RequestLogicLoaderResponse : IMessage
	{
		public ILogicLoader LogicLoader { get; }

		public RequestLogicLoaderResponse(ILogicLoader logicLoader)
		{
			LogicLoader = logicLoader;
		}
	}
}