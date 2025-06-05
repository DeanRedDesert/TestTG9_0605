namespace Midas.Core
{
	public enum GameIdentityType
	{
		Global,
		Anz,
		AnzHybrid
	}

	public static class GameIdentityTypeExt
	{
		public static bool IsGlobalGi(this GameIdentityType gameIdentityType)
		{
			return gameIdentityType == GameIdentityType.Global || gameIdentityType == GameIdentityType.AnzHybrid;
		}
	}
}