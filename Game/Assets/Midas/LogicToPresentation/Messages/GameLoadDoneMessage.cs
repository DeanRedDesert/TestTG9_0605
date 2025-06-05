namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Initial loading of Presentation/Scenes was done
	/// </summary>
	/// <remarks>
	/// Pres -> Logic: After all initial scenes which should be loaded at startup are loaded
	/// </remarks>
	public sealed class GameLoadDoneMessage : IMessage
	{
	}
}