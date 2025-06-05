using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation
{
	public sealed class UtilityController : IPresentationController
	{
		private UtilityStatus utilityStatus;

		public void Init()
		{
			utilityStatus = StatusDatabase.UtilityStatus;
			Communication.PresentationDispatcher.AddHandler<UtilityModeDetailsMessage>(OnUtilityModeDetails);
		}

		public void DeInit()
		{
			Communication.PresentationDispatcher.RemoveHandler<UtilityModeDetailsMessage>(OnUtilityModeDetails);
			utilityStatus = null;
		}

		public void Destroy()
		{
		}

		private void OnUtilityModeDetails(UtilityModeDetailsMessage obj)
		{
			utilityStatus.IsUtilityModeEnabled = obj.IsAvailable;
			utilityStatus.RegistrySupportedDenominations = obj.RegistrySupportedDenominations;
			utilityStatus.SupportedThemes = obj.RegistrySupportedThemes;
		}
	}
}