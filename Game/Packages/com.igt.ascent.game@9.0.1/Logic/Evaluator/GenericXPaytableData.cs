// -----------------------------------------------------------------------
// <copyright file = "GenericPaytableData.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using PaytableLoader.Interfaces;

    /// <summary>
    /// A data class encapsulating generic reporting elements.
    /// </summary>
    internal class GenericXPaytableData : IGenericPaytableData
    {
        /// <inheritdoc/>
        public PaytableType PaytableType { get; internal set; }

        /// <inheritdoc/>
        public string PaytableName { get; internal set; }

        /// <inheritdoc/>
        public string LegacyPaytableName { get; internal set; }

        /// <inheritdoc/>
        public string GameDescription { get; internal set; }

        /// <inheritdoc/>
        public object RawPaytable { get; internal set; }

        /// <inheritdoc/>
        public long MaxWinCredits { get; internal set; }

        /// <inheritdoc/>
        public uint MaxLines { get; internal set; }

        /// <inheritdoc/>
        public uint MinLines { get; internal set; }

        /// <inheritdoc/>
        public uint MaxWays { get; internal set; }

        /// <inheritdoc/>
        public uint MinWays { get; internal set; }

        /// <inheritdoc/>
        public decimal BaseRtpPercent { get; internal set; }

        /// <inheritdoc/>
        public decimal TotalRtpPercent { get; internal set; }

        /// <inheritdoc/>
        public long MinBetCredits { get; internal set; }

    }
}