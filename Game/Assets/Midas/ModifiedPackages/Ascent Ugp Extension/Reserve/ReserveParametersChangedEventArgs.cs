//-----------------------------------------------------------------------
// <copyright file = "ReserveParametersChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for reserve parameters being changed.
    /// </summary>
    [Serializable]
    public class ReserveParametersChangedEventArgs : TransactionalEventArgs
    {
		/// <summary>
		/// The new values of the reserve parameters.
		/// </summary>
		public ReserveParameters ReserveParameters { get; private set; }

		/// <summary>
		/// Constructor for creating an object with the new parameters.
		/// </summary>
		public ReserveParametersChangedEventArgs(ReserveParameters reserveParameters)
		{
			ReserveParameters = reserveParameters;
		}
    }
}
