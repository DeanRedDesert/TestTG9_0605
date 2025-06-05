using System;
using Midas.Core.General;

namespace Midas.LogicToPresentation
{
	public static class AutoUnregisterHelperExtensions
	{
		/// <summary>
		/// Registers for a game service change.
		/// </summary>
		/// <typeparam name="T">The game service type.</typeparam>
		private sealed class RegisterClassMessageDispatcher<T> : IRegisterClass where T : IMessage
		{
			private readonly IDispatcher messageDispatcher;
			private readonly Action<T> handler;

			public RegisterClassMessageDispatcher(IDispatcher messageDispatcher, Action<T> handler)
			{
				this.messageDispatcher = messageDispatcher;
				this.handler = handler;
			}

			public void Register() => messageDispatcher?.AddHandler(handler);
			public void UnRegister() => messageDispatcher?.RemoveHandler(handler);
		}

		public static void RegisterMessageHandler<T>(this AutoUnregisterHelper autoUnregisterHelper, IDispatcher messageDispatcher, Action<T> handler) where T : IMessage
		{
			autoUnregisterHelper.Register(new RegisterClassMessageDispatcher<T>(messageDispatcher, handler));
		}
	}
}