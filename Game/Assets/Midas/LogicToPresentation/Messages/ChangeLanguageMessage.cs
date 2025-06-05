namespace Midas.LogicToPresentation.Messages
{
	public sealed class ChangeLanguageMessage : IMessage
	{
		public string Language { get; }

		public ChangeLanguageMessage(string language)
		{
			Language = language;
		}
	}
}