namespace Midas.Ascent.Cabinet
{
	public interface ICabinetController
	{
		void Init();
		void OnBeforeLoadGame();
		void Resume();
		void Pause();
		void OnAfterUnLoadGame();
		void DeInit();
	}
}