namespace Midas.Gamble.Presentation
{
	public interface ITrumpsPresenter
	{
		public bool IsRevealing { get; }
		void Reveal();
		void Abort();
	}
}