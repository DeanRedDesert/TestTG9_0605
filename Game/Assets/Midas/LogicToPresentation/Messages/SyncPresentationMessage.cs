namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// This message is used to do a sync call to presentation
	/// This means this message is sent back from the presentation
	/// to acknowledge that everything until now was processed
	/// </summary>
	public sealed class SyncPresentationMessage : IMessage
	{
	}
}