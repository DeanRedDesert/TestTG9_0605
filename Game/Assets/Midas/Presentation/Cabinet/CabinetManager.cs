namespace Midas.Presentation.Cabinet
{
	public static class CabinetManager
	{
		public static ICabinetShim Cabinet { get; private set; }

		public static void Init(ICabinetShim cabinet)
		{
			Cabinet = cabinet;
		}

		public static void DeInit()
		{
			Cabinet = null;
		}
	}
}