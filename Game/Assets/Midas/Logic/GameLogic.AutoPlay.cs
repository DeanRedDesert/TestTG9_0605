using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;

namespace Midas.Logic
{
	public sealed partial class GameLogic
	{
		private bool isAutoplayActive;

		bool IGameLogic.SetAutoplayOn()
		{
			Log.Instance.Info("Request Autoplay On");

			// Nothing to do if autoplay is already active

			if (isAutoplayActive)
				return true;

			if (canChangeStakeCombo)
			{
				Communication.ToPresentationSender.Send(new AutoPlayMessage(AutoPlayMode.Start, AutoPlayRequestSource.Foundation));
				isAutoplayActive = true;
				return true;
			}

			return false;
		}

		void IGameLogic.SetAutoplayOff()
		{
			Log.Instance.Info("Request Autoplay Off");

			if (!isAutoplayActive)
				return;

			Communication.ToPresentationSender.Send(new AutoPlayMessage(AutoPlayMode.Stop, AutoPlayRequestSource.Foundation));
			isAutoplayActive = false;
		}

		private void UpdateAutoPlay()
		{
			if (isAutoplayActive)
			{
				var stopAutoPlay = false;

				if (!foundation.IsAutoPlayOn())
					stopAutoPlay = true;
				else
				{
					var bet = Money.FromCredit(game.StakeCombinations[logicState.SelectedStakeCombinationIndex].TotalBet);
					var playableCredits = GameServices.MetersService.CreditsService.Value + GameServices.MetersService.TotalAwardService.Value;
					stopAutoPlay = playableCredits < bet;
				}

				if (stopAutoPlay)
				{
					isAutoplayActive = false;
					Communication.ToPresentationSender.Send(new AutoPlayMessage(AutoPlayMode.Stop, AutoPlayRequestSource.GameFlow));
				}
			}
		}

		private void OnAutoPlayMessage(AutoPlayMessage msg)
		{
			switch (msg.Mode)
			{
				case AutoPlayMode.Start:
				{
					isAutoplayActive = foundation.SetAutoPlayOn();
					if (!isAutoplayActive)
						msg = new AutoPlayMessage(AutoPlayMode.StartCancelled, msg.Source);
					break;
				}

				case AutoPlayMode.Stop:
				{
					foundation.SetAutoPlayOff();
					isAutoplayActive = false;
					break;
				}
			}

			Communication.ToPresentationSender.Send(msg);
		}
	}
}