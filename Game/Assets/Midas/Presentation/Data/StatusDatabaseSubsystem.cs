using Midas.Core;

namespace Midas.Presentation.Data
{
	public sealed class StatusDatabaseSubsystem : IGamingSubsystem
	{
		public string Name => "StatusDatabase";

		public void Init()
		{
			StatusDatabase.Init();
		}

		public void OnStart()
		{
		}

		public void OnBeforeLoadGame()
		{
		}

		public void OnAfterUnloadGame()
		{
			StatusDatabase.ResetDatabase();
		}

		public void OnStop()
		{
		}

		public void DeInit()
		{
			StatusDatabase.DeInit();
		}
	}
}