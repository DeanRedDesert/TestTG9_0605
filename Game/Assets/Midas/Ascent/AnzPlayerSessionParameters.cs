using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams;

namespace Midas.Ascent
{
	public sealed class AnzPlayerSessionParameters : IPlayerSessionParameters
	{
		private readonly List<PlayerSessionParameterType> pendingParameters = new List<PlayerSessionParameterType>();

		public bool IsPlayerSessionParameterResetEnabled => true;
		public IList<PlayerSessionParameterType> PendingParametersToReset => pendingParameters;

		public event EventHandler<CurrentResetParametersChangedEventArgs> CurrentResetParametersChangedEvent;

		public void ReportParametersBeingReset(IEnumerable<PlayerSessionParameterType> parametersBeingReset)
		{
			var reset = parametersBeingReset.ToArray();
			foreach (var p in reset)
				pendingParameters.Remove(p);

			CurrentResetParametersChangedEvent?.Invoke(this, new CurrentResetParametersChangedEventArgs(PendingParametersToReset, reset));
		}

		public void ResetRequired(IReadOnlyList<PlayerSessionParameterType> parametersToReset)
		{
			pendingParameters.Clear();
			pendingParameters.AddRange(parametersToReset);
			CurrentResetParametersChangedEvent?.Invoke(this, new CurrentResetParametersChangedEventArgs(parametersToReset, new List<PlayerSessionParameterType>()));
		}
	}
}