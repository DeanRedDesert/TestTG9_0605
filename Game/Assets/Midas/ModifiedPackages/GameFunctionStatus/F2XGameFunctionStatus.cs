using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Ascent.Restricted.EventManagement.Interfaces;
using IGT.Game.Core.Communication.Foundation.F2X;
using IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Provides functionality to control the Game Function Status as initiated by F2X commands
	/// </summary>
	internal class F2XGameFunctionStatus : IInterfaceExtension, IGameFunctionStatus
	{
		#region Fields

		private readonly IGameFunctionStatusCategory controlCategory;

		#endregion

		#region IGameFunctionStatus implementation

		/// <inheritdoc />
		public event EventHandler<DenominationMenuControlSetTimeoutEventArgs> OnReceivedDenominationMenuTimeout;

		/// <inheritdoc />
		public event EventHandler<GameButtonBehaviorTypeChangeEventArgs> OnReceivedGameButtonBehaviorType;

		/// <inheritdoc />
		public event EventHandler<DenominationPlayableStatusChangeEventArgs> OnReceivedDenominationPlayableStatus;

		/// <inheritdoc />
		public DenominationMenuTimeoutConfiguration GetDenominationMenuTimeoutInformation()
		{
			var reply = controlCategory.GetDenominationTimeout();
			return new DenominationMenuTimeoutConfiguration(reply.DenominationTimeout, reply.TimeoutActive);
		}

		/// <inheritdoc />
		public IEnumerable<DenominationPlayableStatus> GetDenominationPlayableStatus()
		{
			var reply = controlCategory.GetDenominationPlayableStatus().Statuses.Select(x => x.ToPublic());
			return reply;
		}

		/// <inheritdoc />
		public IEnumerable<GameButtonBehavior> GetGameButtonStatus()
		{
			var reply = controlCategory.GetGameButtonStatus().Select(x => x.ToPublic());
			return reply;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes an instance of <see cref="F2XGameFunctionStatus"/>.
		/// </summary>
		/// <param name="dispatcher"> Interface for processing a transactional event.</param>
		/// <param name="controlCategory"> Game Function Status category instance for communicating with the Foundation.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public F2XGameFunctionStatus(IEventDispatcher dispatcher, IGameFunctionStatusCategory controlCategory)
		{
			if (dispatcher == null) { throw new ArgumentNullException(nameof(dispatcher)); }

			this.controlCategory = controlCategory ?? throw new ArgumentNullException(nameof(controlCategory));
			dispatcher.EventDispatchedEvent += Dispatcher_EventDispatchedEvent;
		}

		#endregion

		#region Private methods

		private void Dispatcher_EventDispatchedEvent(object sender, EventDispatchedEventArgs e)
		{
			if (e.DispatchedEventType == typeof(DenominationMenuControlSetTimeoutEventArgs))
			{
				this.OnReceivedDenominationMenuTimeout?.Invoke(this, (DenominationMenuControlSetTimeoutEventArgs)e.DispatchedEvent);
				e.IsHandled = true;
			}else if (e.DispatchedEventType == typeof(DenominationPlayableStatusChangeEventArgs))
			{
				this.OnReceivedDenominationPlayableStatus?.Invoke(this, (DenominationPlayableStatusChangeEventArgs)e.DispatchedEvent);
				e.IsHandled = true;
			}else if (e.DispatchedEventType == typeof(GameButtonBehaviorTypeChangeEventArgs))
			{
				this.OnReceivedGameButtonBehaviorType?.Invoke(this, (GameButtonBehaviorTypeChangeEventArgs)e.DispatchedEvent);
				e.IsHandled = true;
			}
		}

		#endregion
	}
}
