//-----------------------------------------------------------------------
// <copyright file = "IVkBingoCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.VKBingo;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="VKBingo"/> category.
    /// Video King Bingo / VK Bingo category of messages.
    /// Category: 3020; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IVkBingoCategoryCallbacks
    {
        /// <summary>
        /// Message to the client indicating the context's display state has changed.
        /// </summary>
        /// <param name="displayState">
        /// The Display Control state the user context should be set to.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessDisplayStateChanged(DisplayControlState displayState);

        /// <summary>
        /// Foundation has processed game play metering and response to RequestGamePlayEndMetering request.
        /// </summary>
        /// <param name="status">
        /// Inform client about status of game play metering.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessGamePlayEndMeteringResponse(GamePlayEndMeteringResponseType status);

        /// <summary>
        /// Inform client that the requested cashout has been finished and the machine is now locked
        /// </summary>
        /// <param name="accountNumber">
        /// Pass back the Account Number sent in the lock request.
        /// </param>
        /// <param name="success">
        /// True: lock request successfully completed; machine is locked and only accepts VK Bingo voucher to unlock.
        /// False: lock request not successfully completed; unable to lock the machine.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessLockOutResponse(string accountNumber, bool success);

        /// <summary>
        /// Inform client if VK Bingo lock was successfully released.  Note: This response is only valid for the VK
        /// Bingo lock.  Machine can still be locked by others reasons.
        /// </summary>
        /// <param name="accountNumber">
        /// Pass back the Account Number sent in the unlock request.
        /// </param>
        /// <param name="reason">
        /// Pass back the reason sent in the unlock request.
        /// </param>
        /// <param name="success">
        /// True: VK Bingo lock was removed. False: VK Bingo lock failed to remove.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessUnLockResponse(string accountNumber, UnLockingReason reason, bool success);

    }

}

