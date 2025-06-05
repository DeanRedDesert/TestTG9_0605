// -----------------------------------------------------------------------
// <copyright file = "IGenericPaytableData.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.PaytableLoader.Interfaces
{
    /// <summary>
    /// Paytable data common to all supported paytable types.
    /// </summary>
    public interface IGenericPaytableData
    {
        /// <summary>
        /// Gets the <see cref="PaytableType"/> that this data originated from.
        /// </summary>
        PaytableType PaytableType { get; }
    
        /// <summary>
        /// Gets the MPT paytable name, also referred to as the paytable ID or variant.
        /// </summary>
        string PaytableName { get;  }

        /// <summary>
        /// Gets the legacy XPaytable style paytable name in the format "AVV*.
        /// </summary>
        string LegacyPaytableName { get;  }

        /// <summary>
        /// Gets the raw paytable object. This object is dependent on the context of what underlying paytable type is present./> 
        /// method.
        /// </summary>
        object RawPaytable { get; }

        /// <summary>
        /// Gets the game description that this paytable is currently associated with.
        /// </summary>
        string GameDescription { get; }

        /// <summary>
        /// Gets the max number of credits that can be won.
        /// </summary>
        long MaxWinCredits { get; }

        /// <summary>
        /// Gets the max number of lines that can be played.
        /// </summary>
        uint MaxLines { get; }

        /// <summary>
        /// Gets the min number of lines that can be played.
        /// </summary>
        uint MinLines { get; }

        /// <summary>
        /// Gets the number of ways that can be won.
        /// </summary>
        uint MaxWays { get; }

        /// <summary>
        /// Gets the number of ways that can be won.
        /// </summary>
        uint MinWays { get; }

        /// <summary>
        /// Gets the base Return To Player percentage.
        /// </summary>
        decimal BaseRtpPercent { get; }

        /// <summary>
        /// Gets the total Return To Player percentage.
        /// </summary>
        decimal TotalRtpPercent { get; }

        /// <summary>
        /// Gets the minimum number of credits that can be bet.
        /// </summary>
        long MinBetCredits { get; }
    }
}