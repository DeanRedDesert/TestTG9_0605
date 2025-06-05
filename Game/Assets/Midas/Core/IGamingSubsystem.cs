namespace Midas.Core
{
	public interface IGamingSubsystem
	{
		/// <summary>
		/// The name of the subsystem.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Create any resources that may be needed in the subsystem.
		/// </summary>
		void Init();

		/// <summary>
		/// Start the subsystem.
		/// </summary>
		void OnStart();

		void OnBeforeLoadGame();
		void OnAfterUnloadGame();
		void OnStop();
		void DeInit();
	}
}