namespace Midas.Presentation.Game.WinPresentation
{
	public enum CycleMode
	{
		AtLeastOnce,
		Forever
	}

	public interface IDetailedWinPres
	{
		void Init();
		void DeInit();
		void Start(CycleMode newCycleMode);
		bool CanStop();
		void Stop();
	}
}