using System.Collections.Generic;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class PresentationDataMessage : IMessage
	{
		public IReadOnlyDictionary<string, (bool, object)> Data { get; }

		public PresentationDataMessage(IReadOnlyDictionary<string, (bool, object)> data)
		{
			Data = data;
		}
	}

	public sealed class PresentationDataChangeMessage : IMessage
	{
		public string Name { get; }
		public bool HistoryRequired { get; }
		public object Data { get; }

		public PresentationDataChangeMessage(string name, bool historyRequired, object data)
		{
			Name = name;
			HistoryRequired = historyRequired;
			Data = data;
		}
	}
}