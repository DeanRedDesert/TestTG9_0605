using Midas.Core;

namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	/// Load the Presentation/Scene message including Paytable Config
	/// </summary>
	/// <remarks>
	/// Logic -> Pres: Before SetupPresentation.
	/// </remarks>
	public sealed class GameLoadMessage : IMessage
	{
		public GameIdentityType GameIdentity { get; }

		#region Public

		public GameLoadMessage(GameIdentityType gameIdentity)
		{
			GameIdentity = gameIdentity;
		}

		#endregion
	}
}