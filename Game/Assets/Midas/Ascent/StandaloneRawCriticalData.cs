using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation.Standalone;

namespace Midas.Ascent
{
	internal sealed class StandaloneRawCriticalData : IRawCriticalData
	{
		private readonly GameLib gameLib;

		public StandaloneRawCriticalData(GameLib gameLib)
		{
			this.gameLib = gameLib;
		}

		public void WriteCriticalData(CriticalDataScope scope, string path, byte[] data)
		{
			gameLib.WriteCriticalData(scope, path, data);
		}

		public byte[] ReadCriticalData(CriticalDataScope scope, string path)
		{
			byte[] data = null;
			return gameLib.TryReadCriticalData(ref data, scope, path) ? data : null;
		}

		public bool RemoveCriticalData(CriticalDataScope scope, string path)
		{
			return gameLib.RemoveCriticalData(scope, path);
		}
	}
}