// -----------------------------------------------------------------------
// <copyright file = "TypeConverters.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using F2X.Schemas.Internal.PlayerSession;

    /// <summary>
    /// Contains static extension methods to convert between public and F2X internal types.
    /// </summary>
    internal static class TypeConverters
    {
        /// <summary>
        /// Converts an instance of the F2X internal <see cref="PlayerSessionStatusData"/> class to
        /// a new instance of the public <see cref="PlayerSessionStatus"/> class.
        /// </summary>
        /// <param name="playerSessionStatusData">
        /// The F2X type to convert.
        /// </param>
        /// <returns>
        /// The converted result of public type of player session status.
        /// </returns>
        public static PlayerSessionStatus ToPublic(this PlayerSessionStatusData playerSessionStatusData)
        {
            return playerSessionStatusData == null
                     ? null
                     : new PlayerSessionStatus(
                                            playerSessionStatusData.SessionActive,
                                            playerSessionStatusData.SessionStartTime);
        }
    }
}
