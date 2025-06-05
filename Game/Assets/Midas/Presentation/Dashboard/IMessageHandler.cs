namespace Midas.Presentation.Dashboard
{
	public interface IMessageHandler
	{
		bool MessageDisplayDone { get; }
		void DisplayMessage(string message);
		void DisplayMessage(GameMessage message);
	}
}