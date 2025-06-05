using System.Collections.Generic;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class PresentationDataStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyDictionary<string, (bool, object)>> presentationData;

		public IReadOnlyDictionary<string, (bool HistoryRequired, object Data)> PresentationData => presentationData.Value;

		public PresentationDataStatus() : base(nameof(PresentationDataStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);
			unregisterHelper.RegisterMessageHandler<PresentationDataMessage>(Communication.PresentationDispatcher, msg => presentationData.Value = msg.Data);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			presentationData = AddProperty(nameof(PresentationData), default(IReadOnlyDictionary<string, (bool, object)>));
		}

		public IMessage UpdatePresentationData(string name, bool historyRequired, object data)
		{
			return new PresentationDataChangeMessage(name, historyRequired, data);
		}

		public void UpdatePresentationDataAndSend(string name, bool historyRequired, object data)
		{
			Communication.ToLogicSender.Send(new PresentationDataChangeMessage(name, historyRequired, data));
		}
	}
}