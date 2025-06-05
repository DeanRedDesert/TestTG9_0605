using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Game.Core.Communication.Foundation
{
	/// <summary>
	/// Provides information about the currently configured denomination menu timeout configuration
	/// </summary>
	public class DenominationMenuTimeoutConfiguration
	{
		/// <summary>
		/// Holds information about the active configuration settings for the denomination menu timeout
		/// </summary>
		/// <param name="denominationTimeout">The timeout period in milliseconds</param>
		/// <param name="timeoutActive">If set to true, then the bin must defer control of the denomination menu timeout to the foundation. If it is set to false, then bin may have full control over the timeout functionality</param>
		public DenominationMenuTimeoutConfiguration(uint denominationTimeout, bool timeoutActive)
		{
			DenominationTimeout = denominationTimeout;
			TimeoutActive = timeoutActive;
		}

		/// <summary>
		/// The current denomination menu timeout period, in milliseconds
		/// </summary>
		/// <remarks>This value may be negative to indicate a timeout has been cancelled</remarks>
		public uint DenominationTimeout { get; }

		/// <summary>
		/// If set to true, then the bin must defer control of the denomination menu timeout to the foundation. If it is set to false, then bin may have full control over the timeout functionality
		/// </summary>
		public bool TimeoutActive { get; }
	}
}
