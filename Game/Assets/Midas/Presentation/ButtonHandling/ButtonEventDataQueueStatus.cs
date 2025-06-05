using System;
using Midas.Presentation.Data;

namespace Midas.Presentation.ButtonHandling
{
	public sealed class ButtonEventDataQueueStatus : StatusBlock
	{
		#region Public

		public ButtonEventDataQueueStatus()
			: base(nameof(ButtonEventDataQueueStatus))
		{
			
		}

		public override string ToString()
		{
			return $"IsEmpty={IsEmpty}, Peek={Peek},"
				+ $" BecauseGameCycleActive={BecauseGameCycleActive}, BecauseCreditPlayoffStatusActive={BecauseCreditPlayoffStatusActive}, "
				+ $" BecauseCreditPlayoffStatusChoosing={BecauseCreditPlayoffStatusChoosing}, BecausePOSOpen={BecausePOSOpen}"
				+ $" BecauseInterruptable={BecauseInterruptable}";
		}

		// public override void ResetForNewGameCycle()
		// {
		// 	base.ResetForNewGameCycle();
		// 	Peek = (null, () => ButtonFunctionHinderedReasons.None, _ => { });
		// 	IsEmpty = true;
		// 	BecauseGameCycleActive = false;
		// 	BecauseCreditPlayoffStatusActive = false;
		// 	BecauseCreditPlayoffStatusChoosing = false;
		// 	BecausePOSOpen = false;
		// 	BecauseInterruptable = false;
		// }

		public void Enqueue((ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction) entry)
		{
			Peek = entry;
			IsEmpty = false;
			UpdateProperties();
		}

		public (ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction) Dequeue()
		{
			var result = Peek;
			IsEmpty = true;
			Peek = (null, () => ButtonFunctionHinderedReasons.None, _ => { });
			UpdateProperties();
			return result;
		}

		public (ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction) Peek
		{
			get => _peek.Value;
			private set
			{
				_peek.Value = value;
				ButtonFunction = value.buttonEventData!=null? value.buttonEventData.ButtonFunction: ButtonFunction.Undefined;
			}
		}

		public ButtonFunction ButtonFunction
		{
			get => _buttonFunction.Value;
			private set => _buttonFunction.Value = value;
		}

		public bool IsEmpty
		{
			get => _isEmpty.Value;
			private set => _isEmpty.Value = value;
		}

		public bool BecauseGameCycleActive
		{
			get => _becauseGameCycleActive.Value;
			private set => _becauseGameCycleActive.Value = value;
		}

		public bool BecauseCreditPlayoffStatusActive
		{
			get => _becauseCreditPlayoffStatusActive.Value;
			private set => _becauseCreditPlayoffStatusActive.Value = value;
		}

		public bool BecauseCreditPlayoffStatusChoosing
		{
			get => _becauseCreditPlayoffStatusChoosing.Value;
			private set => _becauseCreditPlayoffStatusChoosing.Value = value;
		}

		public bool BecausePOSOpen
		{
			get => _becausePOSOpen.Value;
			private set => _becausePOSOpen.Value = value;
		}

		public bool BecauseInterruptable
		{
			get => _becauseInterruptable.Value;
			private set => _becauseInterruptable.Value = value;
		}

		#endregion

		#region Protected

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			_peek = AddProperty<(ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction)>(nameof(Peek), (null, () => ButtonFunctionHinderedReasons.None, _ => { }));
			_buttonFunction = AddProperty(nameof(ButtonFunction), ButtonFunction.Undefined);
			_isEmpty = AddProperty(nameof(IsEmpty), true);
			_becauseGameCycleActive = AddProperty(nameof(BecauseGameCycleActive), false);
			_becauseCreditPlayoffStatusActive = AddProperty(nameof(BecauseCreditPlayoffStatusActive), false);
			_becauseCreditPlayoffStatusChoosing = AddProperty(nameof(BecauseCreditPlayoffStatusChoosing), false);
			_becausePOSOpen = AddProperty(nameof(BecausePOSOpen), false);
			_becauseInterruptable = AddProperty(nameof(BecauseInterruptable), false);
		}

		#endregion

		#region Private

		private void UpdateProperties()
		{
			var peek = Peek;
			var reason = peek.reason();
			BecauseGameCycleActive = (reason & ButtonFunctionHinderedReasons.BecauseGameCycleActive) != ButtonFunctionHinderedReasons.None;
			BecauseCreditPlayoffStatusActive = (reason & ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusActive) != ButtonFunctionHinderedReasons.None;
			BecauseCreditPlayoffStatusChoosing = (reason & ButtonFunctionHinderedReasons.BecauseCreditPlayoffStatusChoosing) != ButtonFunctionHinderedReasons.None;
			BecausePOSOpen = (reason & ButtonFunctionHinderedReasons.BecausePOSOpen) != ButtonFunctionHinderedReasons.None;
			BecauseInterruptable = (reason & ButtonFunctionHinderedReasons.BecauseInterruptable) != ButtonFunctionHinderedReasons.None;
		}

		private StatusProperty<(ButtonEventData buttonEventData, Func<ButtonFunctionHinderedReasons> reason, Action<ButtonEventData> executeAction)> _peek;
		private StatusProperty<ButtonFunction> _buttonFunction;
		private StatusProperty<bool> _isEmpty;

		private StatusProperty<bool> _becauseGameCycleActive;
		private StatusProperty<bool> _becauseCreditPlayoffStatusActive;
		private StatusProperty<bool> _becauseCreditPlayoffStatusChoosing;
		private StatusProperty<bool> _becausePOSOpen;
		private StatusProperty<bool> _becauseInterruptable;

		#endregion
	}
}