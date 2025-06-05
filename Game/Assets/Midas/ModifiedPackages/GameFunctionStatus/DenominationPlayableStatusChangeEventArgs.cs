using System;
using System.Collections.Generic;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus
{
	/// <summary>
	/// Event arguments for Denomination Playable Status being changed.
	/// </summary>
	[Serializable]
	public class DenominationPlayableStatusChangeEventArgs : EventArgs
	{
		#region Constructors

		/// <summary>
		/// The constructor for the event data of the denomination playable status.
		/// </summary>
		/// <param name="denominationPlayableStatusTypes"></param>
		public DenominationPlayableStatusChangeEventArgs(IEnumerable<DenominationPlayableStatus> denominationPlayableStatusTypes)
		{
			DenominationPlayableStatusTypes = denominationPlayableStatusTypes;
		}

		#endregion

		#region Properties

		/// <summary>
		/// List of game button behavior from the foundation.
		/// </summary>
		public IEnumerable<DenominationPlayableStatus> DenominationPlayableStatusTypes { get; private set; }

		#endregion
	}
}
