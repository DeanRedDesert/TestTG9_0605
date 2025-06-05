// -----------------------------------------------------------------------
// <copyright file = "CabinetTypeExtensions.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// This class contains extension methods for types from Cabinet.
    /// </summary>
    public static class CabinetTypeExtensions
    {
        /// <summary>
        /// An extension method that checks if a switch Id is a virtual Id.
        /// </summary>
        /// <param name="switchId">The switch Id to be checked.</param>
        /// <returns>True, if the switch Id is a virtual one. Otherwise, false.</returns>
        public static bool IsVirtual(this SwitchId switchId)
        {
            return switchId == SwitchId.HandleId ||
                   switchId == SwitchId.GhostId ||
                   switchId == SwitchId.TournamentMenuButtonId;
        }

        /// <summary>
        /// An extension method that checks if a switch Id is supported by SDK.
        /// </summary>
        /// <param name="switchId">The switch Id to be checked.</param>
        /// <returns>True, if the switch Id is supported. Otherwise, false.</returns>
        /// <remarks>The InvalidButtonId is not supported.</remarks>
        public static bool IsSupported(this SwitchId switchId)
        {
            return Enum.IsDefined(typeof(SwitchId), switchId) &&
                   switchId != SwitchId.InvalidButtonId;
        }
    }
}
