using System;
using System.Reflection;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation;
using IGT.Game.Core.Communication.Foundation.Standard;

namespace Midas.Ascent
{
	internal sealed class StandardRawCriticalData : IRawCriticalData
	{
		private delegate void WriteDelegate(CriticalDataScope scope, string path, byte[] data);

		private delegate byte[] ReadDelegate(CriticalDataScope scope, string path);

		private readonly GameLib gameLib;
		private readonly ReadDelegate read;
		private readonly WriteDelegate write;

		public StandardRawCriticalData(GameLib gameLib)
		{
			var t = gameLib.GetType();
			var method = t.GetMethod("ReadRawCriticalData", BindingFlags.Instance | BindingFlags.NonPublic);

			if (method == null)
				throw new Exception("GameLib has changed!");

			read = (scope, path) => (byte[])method.Invoke(gameLib, new object[] { scope, path });

			var mustHaveOpenTransactionMethod = t.GetMethod("MustHaveOpenTransaction", BindingFlags.Instance | BindingFlags.NonPublic);
			var validateAccessMethod = typeof(Utility).GetMethod("ValidateCriticalDataAccess", BindingFlags.Static | BindingFlags.NonPublic);
			var cacheField = t.GetField("criticalDataCache", BindingFlags.Instance | BindingFlags.NonPublic);

			if (cacheField == null)
				throw new Exception("GameLib has changed!");

			var cacheFieldValue = cacheField.GetValue(gameLib);
			var writeDataMethod = cacheField.FieldType.GetMethod("WriteData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			if (mustHaveOpenTransactionMethod == null ||
				validateAccessMethod == null ||
				writeDataMethod == null)
			{
				throw new Exception("GameLib has changed!");
			}

			var dataAccessingType = validateAccessMethod.GetParameters()[2].ParameterType;
			var writeAccessing = Enum.ToObject(dataAccessingType, 1);

			write = (scope, path, data) =>
			{
				mustHaveOpenTransactionMethod.Invoke(gameLib, Array.Empty<object>());
				validateAccessMethod.Invoke(null, new[] { gameLib.GameContextMode, GameCycleState.Invalid, writeAccessing, scope });
				writeDataMethod.Invoke(cacheFieldValue, new object[] { scope, path, data });
			};
		}

		public void WriteCriticalData(CriticalDataScope scope, string path, byte[] data)
		{
			write(scope, path, data);
		}

		public byte[] ReadCriticalData(CriticalDataScope scope, string path)
		{
			return read(scope, path);
		}

		public bool RemoveCriticalData(CriticalDataScope scope, string path)
		{
			return gameLib.RemoveCriticalData(scope, path);
		}
	}
}