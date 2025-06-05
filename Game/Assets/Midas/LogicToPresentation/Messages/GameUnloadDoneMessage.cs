namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Unload of Presentation/All Scenes is done
	/// </summary>
	/// <remarks>
	/// Pres -> Logic: After all scenes which are loaded /except startup scene are unloaded
	/// </remarks>
	public sealed class GameUnloadDoneMessage : IMessage
	{
	}
}