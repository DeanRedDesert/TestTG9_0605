using Midas.Presentation.Sequencing;
using Midas.Presentation.WinPresentation;
using SequenceEvent = Midas.Presentation.WinPresentation.SequenceEvent;

namespace Game.GameIdentity.Common
{
	public sealed class DefaultWinPresSequence : WinPresSequence
	{
		public enum AdditionalSequenceEvents
		{
			TopAwardSound = CustomEvent.CustomEventStartId + 100,
		}

		public DefaultWinPresSequence(string name) : base(name)
		{
			InsertCustomEventAfter((int)SequenceEvent.WinSequenceStart, new CustomEvent(AdditionalSequenceEvents.TopAwardSound));

			for (var winLevel = (int)WinLevel.LNoCredit; winLevel <= (int)WinLevel.L6; ++winLevel)
			{
				var topAwardIntensity = winLevel < (int)WinLevel.L5 ? 0 : 1;
				WinPresEventTable.SetCustomIntensity((int)AdditionalSequenceEvents.TopAwardSound, winLevel, topAwardIntensity);
			}
		}
	}
}