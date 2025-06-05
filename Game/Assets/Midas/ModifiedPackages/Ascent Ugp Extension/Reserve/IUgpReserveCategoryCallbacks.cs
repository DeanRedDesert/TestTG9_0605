//-----------------------------------------------------------------------
// <copyright file = "IUgpReserveCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    /// <summary>
    /// Interface to accept callbacks from the UGP Reserve category.
    /// </summary>
    public interface IUgpReserveCategoryCallbacks
    {
		/// <summary>
		/// Method called when UgpReserveCategory SetReserveParameters message is received from the foundation.
		/// </summary>
		/// <param name="parameters">The latest reserve parameters to pass to the callbacks.</param>
		/// <returns>
		/// An error message if an error occurs; otherwise, null.
		/// </returns>
		string ProcessSetReserveParameters(ReserveParameters parameters);
    }
}
