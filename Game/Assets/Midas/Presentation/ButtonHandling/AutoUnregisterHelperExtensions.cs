using System;
using Midas.Core.General;
using Midas.LogicToPresentation;

namespace Midas.Presentation.ButtonHandling
{
	public static class AutoUnregisterHelperExtensions
	{
		public static void RegisterButtonStateChangedListener(this AutoUnregisterHelper autoUnregisterHelper, ButtonFunction buttonFunction, Action<ButtonStateData> handler)
		{
			var regClass = new RegisterButtonStateClass(buttonFunction, handler);
			autoUnregisterHelper.Register(regClass);
		}

		public static void RegisterButtonEventListener(this AutoUnregisterHelper autoUnregisterHelper, ButtonFunction buttonFunction, Action<ButtonEventData> handler)
		{
			var regClass = new RegisterButtonEventClass(buttonFunction, handler);
			autoUnregisterHelper.Register(regClass);
		}

		public static void RegisterButtonEventListener(this AutoUnregisterHelper autoUnregisterHelper, Action<ButtonEventData> handler)
		{
			var regClass = new RegisterAllButtonsClass(handler);
			autoUnregisterHelper.Register(regClass);
		}

		public static void RegisterButtonPressListener(this AutoUnregisterHelper autoUnregisterHelper, ButtonFunction buttonFunction, Action<ButtonEventData> handler)
		{
			var regClass = new RegisterButtonPressClass(buttonFunction, handler);
			autoUnregisterHelper.Register(regClass);
		}

		private sealed class RegisterButtonStateClass : IRegisterClass
		{
			private readonly ButtonFunction buttonFunction;
			private readonly Action<ButtonStateData> handler;

			public RegisterButtonStateClass(ButtonFunction buttonFunction, Action<ButtonStateData> handler)
			{
				this.buttonFunction = buttonFunction;
				this.handler = handler;
			}

			public void Register()
			{
				ButtonManager.AddButtonStateChangedListener(buttonFunction, handler);
			}

			public void UnRegister()
			{
				ButtonManager.RemoveButtonStateChangedListener(buttonFunction, handler);
			}
		}

		private sealed class RegisterButtonEventClass : IRegisterClass
		{
			private readonly ButtonFunction buttonFunction;
			private readonly Action<ButtonEventData> handler;

			public RegisterButtonEventClass(ButtonFunction buttonFunction, Action<ButtonEventData> handler)
			{
				this.buttonFunction = buttonFunction;
				this.handler = handler;
			}

			public void Register()
			{
				ButtonManager.AddButtonEventListener(buttonFunction, handler);
			}

			public void UnRegister()
			{
				ButtonManager.RemoveButtonEventListener(buttonFunction, handler);
			}
		}

		private sealed class RegisterButtonPressClass : IRegisterClass
		{
			private readonly ButtonFunction buttonFunction;
			private readonly Action<ButtonEventData> handler;

			public RegisterButtonPressClass(ButtonFunction buttonFunction, Action<ButtonEventData> handler)
			{
				this.buttonFunction = buttonFunction;
				this.handler = handler;
			}

			public void Register()
			{
				ButtonManager.AddButtonPressListener(buttonFunction, handler);
			}

			public void UnRegister()
			{
				ButtonManager.RemoveButtonPressListener(buttonFunction, handler);
			}
		}

		private sealed class RegisterAllButtonsClass : IRegisterClass
		{
			private readonly Action<ButtonEventData> handler;

			public RegisterAllButtonsClass(Action<ButtonEventData> handler)
			{
				this.handler = handler;
			}

			public void Register()
			{
				ButtonManager.AddButtonEventListener(handler);
			}

			public void UnRegister()
			{
				ButtonManager.RemoveButtonEventListener(handler);
			}
		}
	}
}