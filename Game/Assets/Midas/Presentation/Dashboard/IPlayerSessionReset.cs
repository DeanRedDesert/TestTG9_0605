using System.Collections.Generic;
using Midas.Core;

namespace Midas.Presentation.Dashboard
{
	public interface IPlayerSessionReset
	{
		#region Public

		void ResetForNewPlayerSession(IReadOnlyList<PlayerSessionParameterType> pendingResetParams, IList<PlayerSessionParameterType> resetDoneParams);

		#endregion
	}
}