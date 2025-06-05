//-----------------------------------------------------------------------
// <copyright file = "GameDenominationInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The class defined for storing game denomination information.
    /// </summary>
    public class GameDenominationInfo
    {
        /// <summary>
        /// Gets the available denominations.
        /// </summary>
        public ICollection<long> AvailableDenominations { get; private set; }

        /// <summary>
        /// Get the available denominations that are associated with progressives.
        /// </summary>
        public ICollection<long> AvailableProgressiveDenominations { get; private set; }

        /// <summary>
        /// Gets the default denomination.
        /// </summary>
        public long DefaultGameDenomination { get; private set; }

        /// <summary>
        /// Constructs an instance of the <see cref="GameDenominationInfo"/>.
        /// </summary>
        /// <param name="availableDenominations">The amount of the progressive in cents.</param>
        /// <param name="availableProgressiveDenominations">The game level of the progressive.</param>
        /// <param name="defaultGameDenomination">Prize string associated with the progressive.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="availableDenominations"/> or
        /// <paramref name="availableProgressiveDenominations"/> is null.
        /// </exception>
        public GameDenominationInfo(ICollection<long> availableDenominations,
            ICollection<long> availableProgressiveDenominations, long defaultGameDenomination)
        {
            if(availableDenominations == null)
            {
                throw new ArgumentNullException("availableDenominations");
            }
            if(availableProgressiveDenominations == null)
            {
                throw new ArgumentNullException("availableProgressiveDenominations");
            }
            AvailableDenominations = availableDenominations;
            AvailableProgressiveDenominations = availableProgressiveDenominations;
            DefaultGameDenomination = defaultGameDenomination;
        }
    }
}
