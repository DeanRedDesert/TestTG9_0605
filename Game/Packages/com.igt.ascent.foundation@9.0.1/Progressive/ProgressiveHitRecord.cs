//-----------------------------------------------------------------------
// <copyright file = "ProgressiveHitRecord.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using CompactSerialization;
    using System;
    using System.IO;

    /// <summary>
    /// The ProgressiveHitRecord keeps the information on
    /// the status of a progressive level being hit.
    /// </summary>
    [Serializable]
    public class ProgressiveHitRecord: ICompactSerializable
    {
        /// <summary>
        /// Number of times this progressive level has been hit.
        /// </summary>
        public ulong HitCount { get; set; }

        /// <summary>
        /// Total amount awarded for this progressive level so far,
        /// in base units.
        /// </summary>
        public long TotalHitAmount { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return string.Format("Hit Count({0})/Total Hit Amount({1})",
                                 HitCount, TotalHitAmount);
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, HitCount);
            CompactSerializer.Write(stream, TotalHitAmount);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            HitCount = CompactSerializer.ReadUlong(stream);
            TotalHitAmount = CompactSerializer.ReadLong(stream);
        }

        #endregion
    }
}