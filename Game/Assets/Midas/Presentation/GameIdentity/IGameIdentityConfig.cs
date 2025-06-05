using Midas.Core;

namespace Midas.Presentation.GameIdentity
{
	/// <summary>
	/// This interface allows any controller in the game to configure itself dependent on the game identity.
	/// </summary>
	public interface IGameIdentityConfig
	{
		void ConfigureGameIdentity(GameIdentityType gameIdentityType);
	}
}