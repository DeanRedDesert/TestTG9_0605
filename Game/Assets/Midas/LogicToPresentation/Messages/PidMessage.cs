namespace Midas.LogicToPresentation.Messages
{
	public enum PidAction
	{ 
		Activated,
		Deactivated,
		GameInfoEntered,
		SessionInfoEntered,
		StartSessionTracking,
		StopSessionTracking,
		ToggleService
	}

	public sealed class PidMessage : IMessage
	{
		public PidAction Action { get; }

		public PidMessage(PidAction action)
		{
			Action = action;
		}
	}
}