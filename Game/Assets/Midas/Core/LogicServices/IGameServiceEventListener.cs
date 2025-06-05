namespace Midas.Core.LogicServices
{
	/// <summary>
	/// Provides the ability to listen for game service changes.
	/// </summary>
	public interface IGameServiceEventListener<in T>
	{
		/// <summary>
		/// Called whenever the game service changes.
		/// </summary>
		/// <param name="newValue">The new service value.</param>
		void OnEventRaised(T newValue);
	}
}