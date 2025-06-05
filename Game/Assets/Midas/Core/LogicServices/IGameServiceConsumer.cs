namespace Midas.Core.LogicServices
{
	/// <summary>
	/// The presentation side of a game service.
	/// </summary>
	/// <typeparam name="T">The service type.</typeparam>
	public interface IGameServiceConsumer<out T>
	{
		/// <summary>
		/// Register a change listener.
		/// </summary>
		/// <param name="onChangeHandler">The listener to register.</param>
		void RegisterChangeListener(IGameServiceEventListener<T> onChangeHandler);

		/// <summary>
		/// Unregister a change listener.
		/// </summary>
		/// <param name="onChangeHandler">The listener to unregister.</param>
		void UnregisterChangeListener(IGameServiceEventListener<T> onChangeHandler);
	}
}