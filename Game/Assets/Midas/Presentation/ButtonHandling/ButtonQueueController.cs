using System;
using System.Collections.Generic;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	/// Button queue controller allows us to enqueue a button action but process it once the game is in a state to do so.
	/// eg. Pressing play from offer gamble requires the game to be completed before the play may be processed.
	/// </summary>
	public class ButtonQueueController : IPresentationController
	{
		// TODO: Rename this. Hindered is onerous.

		private readonly List<Func<ButtonFunctionHinderedReasons, ButtonFunctionHinderedReasons>> _hinderedChecks =
			new List<Func<ButtonFunctionHinderedReasons, ButtonFunctionHinderedReasons>>();

		private readonly List<(ButtonFunctionHinderedReasons reason, Action action)> _hindernessResolver = new List<(ButtonFunctionHinderedReasons, Action)>();

		private ButtonEventDataQueueStatus _buttonEventDataQueueStatus;

		#region IPresentationController Implementation

		public void Init()
		{
			_buttonEventDataQueueStatus = StatusDatabase.ButtonEventDataQueueStatus;

			_hinderedChecks.Add(BecauseOfGameCycleActiveHindered);
			_hinderedChecks.Add(BecauseCreditPlayoffStatusActiveHindered);
			_hinderedChecks.Add(BecauseCreditPlayoffStatusChoosingHindered);
			_hinderedChecks.Add(BecauseNotPlayerWagerOfferableHindered);

			FrameUpdateService.LateUpdate.OnFrameUpdate += Update;
		}

		public void DeInit()
		{
			FrameUpdateService.LateUpdate.OnFrameUpdate -= Update;

			CheckAndClearHindernessResolver();
			_hinderedChecks.Clear();

			_buttonEventDataQueueStatus = null;
		}

		public void Destroy()
		{
		}

		#endregion

		#region Public Methods

		public void Enqueue(ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction)
		{
			if (!_buttonEventDataQueueStatus.IsEmpty)
			{
				// Ignore button because there is already one waiting to be executed
				return;
			}

			if (reason() == ButtonFunctionHinderedReasons.None)
			{
				executeAction(buttonEventData);
			}
			else
			{
				_buttonEventDataQueueStatus.Enqueue((buttonEventData, reason, executeAction));
			}
		}

		public void AddHindernessResolver(ButtonFunctionHinderedReasons reason, Action action)
		{
			_hindernessResolver.Add((reason, action));
		}

		public void RemoveHindernessResolver(ButtonFunctionHinderedReasons reason, Action action)
		{
			_hindernessResolver.Remove((reason, action)); //How is that equals implemented?
		}

		public void AddHinderedChecker(Func<ButtonFunctionHinderedReasons, ButtonFunctionHinderedReasons> checker)
		{
			_hinderedChecks.Add(checker);
		}

		public void RemoveHinderedChecker(Func<ButtonFunctionHinderedReasons, ButtonFunctionHinderedReasons> checker)
		{
			_hinderedChecks.Remove(checker);
		}

		public ButtonFunctionHinderedReasons IsHindered(ButtonFunctionHinderedReasons hinderedReasons)
		{
			var result = ButtonFunctionHinderedReasons.None;
			foreach (Func<ButtonFunctionHinderedReasons, ButtonFunctionHinderedReasons> hinderedCheck in _hinderedChecks)
			{
				result |= hinderedCheck(hinderedReasons);
			}

			return result;
		}

		public bool IsEmpty => _buttonEventDataQueueStatus.IsEmpty;

		#endregion

		private void Update()
		{
			if (!_buttonEventDataQueueStatus.IsEmpty)
			{
				var peek = _buttonEventDataQueueStatus.Peek;
				var reasons = peek.reason();
				if (reasons == ButtonFunctionHinderedReasons.None)
				{
					var entry = _buttonEventDataQueueStatus.Dequeue();
					entry.executeAction(entry.buttonEventData);
				}
				else
				{
					//try to remove the hinderness
					for (int i = 0; i < _hindernessResolver.Count; i++)
					{
						if ((_hindernessResolver[i].reason & reasons) != ButtonFunctionHinderedReasons.None)
						{
							_hindernessResolver[i].action();
						}
					}

					//check if we now can execute the action
					reasons = peek.reason();
					if (reasons == ButtonFunctionHinderedReasons.None)
					{
						var entry = _buttonEventDataQueueStatus.Dequeue();
						entry.executeAction(entry.buttonEventData);
					}
				}
			}
		}

		private ButtonFunctionHinderedReasons BecauseOfGameCycleActiveHindered(ButtonFunctionHinderedReasons hinderedReasons)
		{
			ButtonFunctionHinderedReasons result = ButtonFunctionHinderedReasons.None;
			if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseGameCycleActive) != ButtonFunctionHinderedReasons.None && StatusDatabase.GameStatus.GameIsActive)
			{
				result |= ButtonFunctionHinderedReasons.BecauseGameCycleActive;
			}

			return result;
		}

		private ButtonFunctionHinderedReasons BecauseCreditPlayoffStatusActiveHindered(ButtonFunctionHinderedReasons hinderedReasons)
		{
			ButtonFunctionHinderedReasons result = ButtonFunctionHinderedReasons.None;
			// if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusActive) != ButtonFunctionHinderedReasons.None
			// 	&& StatusDatabase.CreditPlayoffStatus.Status == CreditPlayoffStatus.Active)
			// {
			// 	result |= ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusActive;
			// }

			return result;
		}

		private ButtonFunctionHinderedReasons BecauseCreditPlayoffStatusChoosingHindered(ButtonFunctionHinderedReasons hinderedReasons)
		{
			ButtonFunctionHinderedReasons result = ButtonFunctionHinderedReasons.None;
			// if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusChoosing) != ButtonFunctionHinderedReasons.None
			// 	&& StatusDatabase.CreditPlayoffStatus.Status == CreditPlayoffStatus.Choosing)
			// {
			// 	result |= ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusChoosing;
			// }

			return result;
		}

		private ButtonFunctionHinderedReasons BecauseNotPlayerWagerOfferableHindered(ButtonFunctionHinderedReasons hinderedReasons)
		{
			ButtonFunctionHinderedReasons result = ButtonFunctionHinderedReasons.None;
			// if ((hinderedReasons & ButtonFunctionHinderedReasons.BecauseNotPlayerWagerOfferable) != ButtonFunctionHinderedReasons.None
			// 	&& !StatusDatabase.BankStatus.IsPlayerWagerOfferable)
			// {
			// 	result |= ButtonFunctionHinderedReasons.BecauseNotPlayerWagerOfferable;
			// }

			return result;
		}

		private void CheckAndClearHindernessResolver()
		{
			foreach (var entry in _hindernessResolver)
			{
				Log.Instance.Fatal($"HindernessResolver '{entry.action}' for hinderness '{entry.reason}' not unregistered");
			}

			_hindernessResolver.Clear();
		}
	}
}