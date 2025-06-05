using IGT.Ascent.Communication.Platform.Interfaces;

namespace Midas.Ascent
{
	internal interface IRawCriticalData
	{
		public void WriteCriticalData(CriticalDataScope scope, string path, byte[] data);

		public byte[] ReadCriticalData(CriticalDataScope scope, string path);

		public bool RemoveCriticalData(CriticalDataScope scope, string path);
	}
}