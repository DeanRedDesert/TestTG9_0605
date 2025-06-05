using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Foundation;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IGameFunctionStatus gameFunctionStatus;

		/// <summary>
		/// Indicates that a denomination menu timeout action should be initiated
		/// </summary>
		public event EventHandler<DenominationMenuControlSetTimeoutEventArgs> DenominationMenuTimeoutSet;

		/// <summary>
		/// Indicates that a denomination playable status change request should be initiated.
		/// </summary>
		public event EventHandler<DenominationPlayableStatusChangeEventArgs> DenominationPlayableStatusChange;

		/// <summary>
		/// Indicates that a game button behavior change request should be initiated.
		/// </summary>
		public event EventHandler<GameButtonBehaviorTypeChangeEventArgs> GameButtonBehaviorTypeChange;

		private void InitGameFunctionStatus()
		{
			gameFunctionStatus = GameLib.GetInterface<IGameFunctionStatus>();
			if (gameFunctionStatus != null)
			{
				gameFunctionStatus.OnReceivedDenominationMenuTimeout += OnReceivedDenominationMenuTimeout;
				gameFunctionStatus.OnReceivedDenominationPlayableStatus += OnReceivedDenominationPlayableStatus;
				gameFunctionStatus.OnReceivedGameButtonBehaviorType += OnReceivedGameButtonBehaviorType;
			}
		}

		private void DeInitGameFunctionStatus()
		{
			if (gameFunctionStatus != null)
			{
				gameFunctionStatus.OnReceivedDenominationMenuTimeout -= OnReceivedDenominationMenuTimeout;
				gameFunctionStatus.OnReceivedDenominationPlayableStatus -= OnReceivedDenominationPlayableStatus;
				gameFunctionStatus.OnReceivedGameButtonBehaviorType -= OnReceivedGameButtonBehaviorType;
			}

			gameFunctionStatus = null;
		}

		/// <summary>
		/// When the change request for the denomination menu timeout was received.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceivedDenominationMenuTimeout(object sender, DenominationMenuControlSetTimeoutEventArgs e) => DenominationMenuTimeoutSet?.Invoke(sender, e);

		/// <summary>
		/// When the change request for the denomination playable status was received.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceivedDenominationPlayableStatus(object sender, DenominationPlayableStatusChangeEventArgs e) => DenominationPlayableStatusChange?.Invoke(sender, e);

		/// <summary>
		/// When the change request for the gameButton behavior was received.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReceivedGameButtonBehaviorType(object sender, GameButtonBehaviorTypeChangeEventArgs e) => GameButtonBehaviorTypeChange?.Invoke(sender, e);

		/// <summary>
		/// Returns the current denomination timeout configuration
		/// </summary>
		/// <returns></returns>
		public DenominationMenuTimeoutConfiguration GetDenominationMenuTimeoutConfiguration() => gameFunctionStatus != null ? gameFunctionStatus.GetDenominationMenuTimeoutInformation() : new DenominationMenuTimeoutConfiguration(0, false);

		/// <summary>
		/// Returns the current denomination playable status.
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<DenominationPlayableStatus> GetDenominationPlayableStatus() => gameFunctionStatus != null ? gameFunctionStatus.GetDenominationPlayableStatus().ToList() : new List<DenominationPlayableStatus>();

		/// <summary>
		/// Returns the current game button status.
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<GameButtonBehavior> GetGameButtonStatus() => gameFunctionStatus != null ? gameFunctionStatus.GetGameButtonStatus().ToList() : new List<GameButtonBehavior>();
	}
}