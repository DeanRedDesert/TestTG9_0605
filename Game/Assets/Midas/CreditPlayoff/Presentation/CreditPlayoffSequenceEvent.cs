using Midas.Presentation.Sequencing;

namespace Midas.CreditPlayoff.Presentation
{
	public enum CreditPlayoffSequenceEvent
	{
		Activate = CustomEvent.MidasEventStartId,
		ZoomBig,
		ZoomBackToActive,
		SpinToWin,
		SpinToLose,
		Outro,
		Reset,
	}
}