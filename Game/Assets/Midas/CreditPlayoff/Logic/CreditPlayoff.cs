using Midas.Core;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.Logic;
using Midas.LogicToPresentation;

namespace Midas.CreditPlayoff.Logic
{
	public sealed partial class CreditPlayoff : ICreditPlayoff
	{
		private const string CreditPlayoffCriticalData = "CreditPlayoffData";
		private readonly bool isEnabled;
		private IFoundationShim foundation;
		private CreditPlayoffService creditPlayoffService;
		private CreditPlayoffData creditPlayoffData;
		private bool? dialUpWin;

		public CreditPlayoff(bool isEnabled)
		{
			this.isEnabled = isEnabled;
		}

		public void Init(IFoundationShim foundationShim, object historyState)
		{
			foundation = foundationShim;

			if (foundation.GameMode == FoundationGameMode.History)
			{
				creditPlayoffData = (CreditPlayoffData)historyState ?? ResetData();
			}
			else if (!foundation.TryReadNvram(NvramScope.Variation, CreditPlayoffCriticalData, out creditPlayoffData))
			{
				creditPlayoffData = ResetData();
				SaveCreditPlayoffData();
			}

			creditPlayoffService = CreditPlayoffService.Instance;

			UpdateServices();

			Communication.LogicDispatcher.AddHandler<CreditPlayoffActivatedMessage>(OnCreditPlayoffActivatedMessage);
			Communication.LogicDispatcher.AddHandler<CreditPlayoffReturnToGameMessage>(OnCreditPlayoffDeclinedMessage);

			if (foundation.ShowMode == FoundationShowMode.Development)
				Communication.LogicDispatcher.AddHandler<CreditPlayoffDialUpMessage>(OnCreditPlayoffDialUpMessage);

			CreditPlayoffData ResetData()
			{
				return new CreditPlayoffData
				{
					State = CreditPlayoffState.Unavailable,
					Cash = Money.Zero,
					Bet = Money.Zero
				};
			}
		}

		public void DeInit()
		{
			Communication.LogicDispatcher.RemoveHandler<CreditPlayoffActivatedMessage>(OnCreditPlayoffActivatedMessage);
			Communication.LogicDispatcher.RemoveHandler<CreditPlayoffReturnToGameMessage>(OnCreditPlayoffDeclinedMessage);

			if (foundation.ShowMode == FoundationShowMode.Development)
				Communication.LogicDispatcher.RemoveHandler<CreditPlayoffDialUpMessage>(OnCreditPlayoffDialUpMessage);

			foundation = null;
		}

		public void Reset()
		{
			if (creditPlayoffData.State != CreditPlayoffState.Idle)
				return;

			creditPlayoffData.State = CreditPlayoffState.Available;
			SaveCreditPlayoffData();
			UpdateServices();
		}

		public void Offer(Money cash, Money bet)
		{
			if (creditPlayoffData.State == CreditPlayoffState.Idle)
				return;

			if (isEnabled && cash > Money.Zero && cash < bet)
			{
				creditPlayoffData.State = CreditPlayoffState.Available;
				creditPlayoffData.Cash = cash;
				creditPlayoffData.Bet = bet;
			}
			else
			{
				creditPlayoffData.State = CreditPlayoffState.Unavailable;
			}

			SaveCreditPlayoffData();
			UpdateServices();
		}

		public void Decline()
		{
			// Framework may only decline credit playoff prior to playing it

			if (isEnabled && (creditPlayoffData.State == CreditPlayoffState.Available || creditPlayoffData.State == CreditPlayoffState.Idle))
			{
				creditPlayoffData.State = CreditPlayoffState.Unavailable;
				SaveCreditPlayoffData();
				UpdateServices();
			}
		}

		public bool TryCommit()
		{
			if (isEnabled && creditPlayoffData.State == CreditPlayoffState.Idle)
			{
				creditPlayoffData.State = CreditPlayoffState.Committed;
				SaveCreditPlayoffData();
				UpdateServices();
				return true;
			}

			return false;
		}

		public IOutcome StartCreditPlayoff()
		{
			if (creditPlayoffData.State != CreditPlayoffState.Committed)
				Log.Instance.Fatal("Trying to start credit playoff but state is not committed");

			var weight = creditPlayoffData.Cash.AsMinorCurrency;
			var totalWeight = creditPlayoffData.Bet.AsMinorCurrency;
			var number = foundation.GetRandomNumbers(1, 1, (int)totalWeight)[0];
			var isWin = weight >= number;

			if (foundation.ShowMode == FoundationShowMode.Development && dialUpWin != null)
			{
				while (isWin != dialUpWin.Value)
				{
					number = foundation.GetRandomNumbers(1, 1, (int)totalWeight)[0];
					isWin = weight >= number;
				}

				dialUpWin = null;
			}

			Log.Instance.Info($"Playing round wager up with {weight} / {totalWeight}");
			Log.Instance.Info($"{(isWin ? "Win" : "Loss")} with magic number {number}");

			creditPlayoffData.State = isWin ? CreditPlayoffState.Win : CreditPlayoffState.Loss;
			creditPlayoffData.Number = number;
			SaveCreditPlayoffData();
			UpdateServices();

			return isWin
				? CreditPlayoffOutcome.CreateWin(creditPlayoffData.Cash, creditPlayoffData.Bet)
				: CreditPlayoffOutcome.CreateLoss(creditPlayoffData.Cash);
		}

		public object GetHistoryState()
		{
			// Clone so it can safely change after the fact.

			return new CreditPlayoffData
			{
				State = creditPlayoffData.State,
				Cash = creditPlayoffData.Cash,
				Bet = creditPlayoffData.Bet,
				Number = creditPlayoffData.Number
			};
		}

		public void ShowHistory(object historyData)
		{
		}

		public object GetGameCycleHistoryData()
		{
			return null;
		}

		private void OnCreditPlayoffActivatedMessage(CreditPlayoffActivatedMessage obj)
		{
			Log.Instance.Info("CreditPlayoffActivatedMessage received by logic.");

			if (creditPlayoffData.State == CreditPlayoffState.Available)
			{
				creditPlayoffData.State = CreditPlayoffState.Idle;
				SaveCreditPlayoffData();
				UpdateServices();
			}
			else
			{
				Log.Instance.Warn("CreditPlayoffActivatedMessage should only be received if credit playoff is in Available state.");
			}
		}

		private void OnCreditPlayoffDeclinedMessage(CreditPlayoffReturnToGameMessage obj)
		{
			Log.Instance.Info("CreditPlayoffDeclineMessage received by logic.");

			if (creditPlayoffData.State == CreditPlayoffState.Idle)
			{
				creditPlayoffData.State = CreditPlayoffState.Available;
				SaveCreditPlayoffData();
				UpdateServices();
			}
			else
			{
				Log.Instance.Warn("CreditPlayoffDeclineMessage should only be received if credit playoff is in Idle state.");
			}
		}

		private void OnCreditPlayoffDialUpMessage(CreditPlayoffDialUpMessage msg)
		{
			if (foundation.ShowMode != FoundationShowMode.Development)
				return;

			dialUpWin = msg.Win;
		}

		private void SaveCreditPlayoffData()
		{
			foundation.WriteNvram(NvramScope.Variation, CreditPlayoffCriticalData, creditPlayoffData);
		}

		private void UpdateServices()
		{
			creditPlayoffService.StateService.SetValue(creditPlayoffData.State);
			creditPlayoffService.BetService.SetValue(creditPlayoffData.Bet);
			creditPlayoffService.CashService.SetValue(creditPlayoffData.Cash);
			creditPlayoffService.ResultNumberService.SetValue(creditPlayoffData.Number);
		}
	}
}