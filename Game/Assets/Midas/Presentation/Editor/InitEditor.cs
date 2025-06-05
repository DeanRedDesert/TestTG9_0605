using System;
using System.Linq;
using Midas.Core;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.Game;
using Midas.Tools.Editor;
using StatusDatabase = Midas.Presentation.Data.StatusDatabase;

namespace Midas.Presentation.Editor
{
	[InitForEditor(0)]
	public static class InitEditor
	{
		#region Public

		public static void Init()
		{
			InitLoggingInEditor();
			InitStatusDatabase();
			CreateGameInstance();
		}

		public static void DeInit()
		{
			DestroyGameInstance();
			DeInitStatusDatabase();
			Tools.Logging.DeInit();
		}

		#endregion

		#region Private

		private static void InitLoggingInEditor()
		{
			Tools.Logging.Init();
		}

		private static void DeInitStatusDatabase()
		{
			Log.Instance.Info("DeInit StatusDatabase for Editor");
			StatusDatabase.DeInit();
			GameServices.DeInit();
		}

		private static void InitStatusDatabase()
		{
			Log.Instance.Info("Init StatusDatabase for Editor");
			GameServices.Init();
			StatusDatabase.Init();
		}

		private static void CreateGameInstance()
		{
			// Try to find a GameBase implementation that is not abstract.

			var foundGameAllTypes = ReflectionUtil.GetAllTypes(t => !t.IsAbstract && typeof(GameBase).IsAssignableFrom(t)).ToArray();

			if (foundGameAllTypes.Length < 1)
			{
				Log.Instance.Error($"Found no implementation of {typeof(GameBase).FullName}");
				return;
			}

			if (foundGameAllTypes.Length > 1)
			{
				Log.Instance.Error($"Found more than one implementation of {typeof(GameBase).FullName}");

				for (var i = 0; i < foundGameAllTypes.Length; i++)
					Log.Instance.Error($"Found type {i} is {foundGameAllTypes[i].FullName}");

				return;
			}

			Log.Instance.Info($"Creating game instance {foundGameAllTypes[0]}");

			var game = (GameBase)Activator.CreateInstance(foundGameAllTypes[0]);
			if (game != null)
			{
				game.CreateCustomGameServices();
				game.Create();
			}
			else
				Log.Instance.Error($"Failed creating game instance {foundGameAllTypes[0]}");
		}

		private static void DestroyGameInstance()
		{
			Log.Instance.Info($"Destroying game instance {GameBase.GameInstance?.GetType()}");
			GameBase.GameInstance?.Destroy();
		}

		#endregion
	}
}