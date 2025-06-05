using Midas.LogicToPresentation;

namespace Midas.Gle.LogicToPresentation
{
	public sealed class GleSelectionMessage : IMessage
	{
		public GleUserSelection UserSelection { get; }

		public GleSelectionMessage(GleUserSelection userSelection)
		{
			UserSelection = userSelection;
		}
	}
}