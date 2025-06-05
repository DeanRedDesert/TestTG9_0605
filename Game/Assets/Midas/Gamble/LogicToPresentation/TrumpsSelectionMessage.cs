using Midas.LogicToPresentation;

namespace Midas.Gamble.LogicToPresentation
{
	public sealed class TrumpsSelectionMessage : IMessage
	{
		public TrumpsSelection Selection { get; }

		public TrumpsSelectionMessage(TrumpsSelection selection)
		{
			Selection = selection;
		}
	}
}