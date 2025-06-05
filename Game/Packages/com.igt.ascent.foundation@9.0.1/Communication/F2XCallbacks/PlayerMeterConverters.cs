//-----------------------------------------------------------------------
// <copyright file = "PlayerMeterConverters.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using InternalType = F2X.Schemas.Internal.BankStatus;

    /// <summary>
    /// Class containing a method that converts the schema player meters to the Foundation.PlayerMeters.
    /// </summary>
    public static class PlayerMeterConverters
    {
        /// <summary>
        /// Extracts the type "IPlayerMeters" out of the category type "PlayerMeters". 
        /// </summary>
        /// <param name="internalPlayerMeters">
        /// Data format provided by the BankStatusCategory about player meters.
        /// </param>
        /// <returns>PlayerMeters.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="internalPlayerMeters"/> is null.
        /// </exception>
        public static PlayerMeters ToPublic(this InternalType.PlayerMeters internalPlayerMeters)
        {
            if(internalPlayerMeters == null)
            {
                throw new ArgumentNullException(nameof(internalPlayerMeters));
            }
            return new PlayerMeters(internalPlayerMeters.PlayerWagerableMeter,
                                    internalPlayerMeters.PlayerBankMeter,
                                    internalPlayerMeters.PlayerPaidMeter);
        }
    }
}
