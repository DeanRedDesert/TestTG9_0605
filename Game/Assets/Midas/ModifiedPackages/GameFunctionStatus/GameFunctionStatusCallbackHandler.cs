using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Foundation.F2X;
using IGT.Game.Core.Communication.Foundation.F2XTransport;
using F2XDenominationPlayableStatusType = IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus.DenominationPlayableStatusType;
using F2XGameButtonBehaviorType = IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameFunctionStatus.GameButtonBehaviorType;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Handles <see cref="GameFunctionStatusCategory"/> messages from the foundation
	/// </summary>
	public class GameFunctionStatusCallbackHandler : IGameFunctionStatusCategoryCallbacks
	{
		#region Fields

		private readonly IEventCallbacks callbacks;

		#endregion

		#region Constructors

		/// <summary>
		/// Provides functionality to handle messages from the foundation
		/// </summary>
		/// <param name="callbacks">The callback instance messages should be processed with</param>
		public GameFunctionStatusCallbackHandler(IEventCallbacks callbacks)
		{
			this.callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		}

		#endregion

		#region IGameFunctionStatusCategoryCallbacks members

		///<inheritdoc />
		public string ProcessSetDenominationTimeout(uint denominationTimeout, bool timeoutActive)
		{
			var eventArg = new DenominationMenuControlSetTimeoutEventArgs(denominationTimeout, timeoutActive);

			this.callbacks.PostEvent(eventArg);

			return null;
		}

		///<inheritdoc />
		public string ProcessChangeDenominationPlayableStatus(IEnumerable<F2XDenominationPlayableStatusType> denominationSelectionStatus)
		{
			if (denominationSelectionStatus == null) { throw new ArgumentNullException(nameof(denominationSelectionStatus)); }

			var eventArg = new DenominationPlayableStatusChangeEventArgs(denominationSelectionStatus.Select(o => o.ToPublic()));
			this.callbacks.PostEvent(eventArg);
			return null;
		}

		///<inheritdoc />
		public string ProcessButtonStatusChanged(IEnumerable<F2XGameButtonBehaviorType> gameButtonBehavior)
		{
			if (gameButtonBehavior == null) { throw new ArgumentNullException(nameof(gameButtonBehavior)); }

			var eventArg = new GameButtonBehaviorTypeChangeEventArgs(gameButtonBehavior.Select(o => o.ToPublic()));
			this.callbacks.PostEvent(eventArg);
			return null;
		}

		#endregion
	}
}
