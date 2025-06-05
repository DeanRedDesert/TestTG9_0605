namespace Midas.Gamble.Presentation
{
	public interface ITrumpsPresenterSubscriber
	{
		void RegisterTrumpsPresenter(ITrumpsPresenter trumpsPresenter);
		void UnregisterTrumpsPresenter(ITrumpsPresenter trumpsPresenter);
	}
}