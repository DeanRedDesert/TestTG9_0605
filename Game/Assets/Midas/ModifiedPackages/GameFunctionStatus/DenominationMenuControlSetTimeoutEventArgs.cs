using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Game.Core.Communication.Foundation
{
	/// <summary>
	/// Event indicating that the foundation wishes to initiate or cancel a denomination menu timeout
	/// </summary>
	[Serializable]
	public class DenominationMenuControlSetTimeoutEventArgs : EventArgs
	{
		#region Constructors

		/// <summary>
		/// An event which when raised, indicates that the foundation wants to set the denomination menu timeout parameters
		/// </summary>
		/// <param name="denominationTimeout">The timeout period in milliseconds for the denomination menu timeout</param>
		/// <param name="timeoutActive">If set to true, then the bin must defer control of the denomination menu timeout to the foundation. If it is set to false, then bin may have full control over the timeout functionality</param>
		public DenominationMenuControlSetTimeoutEventArgs(uint denominationTimeout, bool timeoutActive)
		{
			DenominationTimeout = denominationTimeout;
			TimeoutActive = timeoutActive;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The timeout period in milliseconds as requested by the foundation.
		/// Upon the period elapsing the denomination menu status should be updated in accordance with all properties in this instance.
		/// </summary>
		/// <remarks>If this is a negative value, any active timeout should be cancelled. If there is no active timeout, this message can be ignored</remarks>
		public uint DenominationTimeout { get; private set; }

		/// <summary>
		/// If set to true, then the bin must defer control of the denomination menu timeout to the foundation using the value provided by <see cref="DenominationTimeout"/>.
		/// If it is set to false, then bin may have full control over the timeout functionality and use any timeout value it wants
		/// </summary>
		public bool TimeoutActive { get; private set; }

		#endregion
	}
}
