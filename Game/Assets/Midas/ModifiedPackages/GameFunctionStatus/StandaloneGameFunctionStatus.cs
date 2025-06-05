using System;
using System.Collections.Generic;
using IGT.Ascent.Restricted.EventManagement.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Provides a stub implementation of <see cref="IGameFunctionStatus"/>
	/// </summary>
	internal class StandaloneGameFunctionStatus : IInterfaceExtension, IGameFunctionStatus, IStandaloneGameFunctionStatusHelper
	{
		#region Fields

		private readonly IStandaloneEventPosterDependency poster;
		private readonly IEventDispatcher dispatcher;

		private DenominationMenuTimeoutConfiguration denominationMenuConfiguration = new DenominationMenuTimeoutConfiguration((uint)TimeSpan.FromSeconds(30).Milliseconds, true);
		private IEnumerable<DenominationPlayableStatus> denominationPlayableStatusList = new List<DenominationPlayableStatus>();
		private IEnumerable<GameButtonBehavior> gameButtonBehaviorList = new List<GameButtonBehavior>();

		#endregion

		#region Constructor

		public StandaloneGameFunctionStatus(IStandaloneEventPosterDependency poster, IEventDispatcher dispatcher)
		{
			this.poster = poster;
			this.dispatcher = dispatcher;

			dispatcher.EventDispatchedEvent += (sender, args) => RaiseEvent(args, OnReceivedDenominationMenuTimeout);
			dispatcher.EventDispatchedEvent += (sender, args) => RaiseEvent(args, OnReceivedDenominationPlayableStatus);
			dispatcher.EventDispatchedEvent += (sender, args) => RaiseEvent(args, OnReceivedGameButtonBehaviorType);
		}

		#endregion

		#region IGameFunctionStatus implementation

		public event EventHandler<DenominationMenuControlSetTimeoutEventArgs> OnReceivedDenominationMenuTimeout;
		public event EventHandler<DenominationPlayableStatusChangeEventArgs> OnReceivedDenominationPlayableStatus;
		public event EventHandler<GameButtonBehaviorTypeChangeEventArgs> OnReceivedGameButtonBehaviorType;

		public DenominationMenuTimeoutConfiguration GetDenominationMenuTimeoutInformation() => denominationMenuConfiguration;
		public IEnumerable<DenominationPlayableStatus> GetDenominationPlayableStatus() => denominationPlayableStatusList;
		public IEnumerable<GameButtonBehavior> GetGameButtonStatus() => gameButtonBehaviorList;

		#endregion

		#region Private methods

		/// <summary>
		/// Raise the dispatched event using the specified event handler.
		/// </summary>
		/// <typeparam name="TEventArgs">Type of the event to raise.</typeparam>
		/// <param name="dispatchedEventArgs">The arguments used for processing the dispatched event.</param>
		/// <param name="eventHandler">The event handler used for raising the event.</param>
		private void RaiseEvent<TEventArgs>(EventDispatchedEventArgs dispatchedEventArgs,
			EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs
		{
			if (dispatchedEventArgs.DispatchedEventType == typeof(TEventArgs))
			{
				if (eventHandler != null)
				{
					eventHandler(this, dispatchedEventArgs.DispatchedEvent as TEventArgs);

					dispatchedEventArgs.IsHandled = true;
				}
			}
		}

		#endregion

		#region IStandaloneGameFunctionStatusHelper implementation

		/// <inheritdoc />
		public void SetGameFunctionStatusConfiguration(DenominationMenuTimeoutConfiguration denomMenuConfiguration, IEnumerable<DenominationPlayableStatus> denomPlayableStatusList, IEnumerable<GameButtonBehavior> gameButtonBehaviorList)
		{
			denominationMenuConfiguration = denomMenuConfiguration;
			denominationPlayableStatusList = denomPlayableStatusList;
			this.gameButtonBehaviorList = gameButtonBehaviorList;

			poster.PostTransactionalEvent(new DenominationMenuControlSetTimeoutEventArgs(this.denominationMenuConfiguration.DenominationTimeout, this.denominationMenuConfiguration.TimeoutActive));
			poster.PostTransactionalEvent(new DenominationPlayableStatusChangeEventArgs(this.denominationPlayableStatusList));
			poster.PostTransactionalEvent(new GameButtonBehaviorTypeChangeEventArgs(this.gameButtonBehaviorList));
		}

		#endregion
	}
}
