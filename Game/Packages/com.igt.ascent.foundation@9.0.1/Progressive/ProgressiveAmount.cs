//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAmount.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using CompactSerialization;

    /// <summary>
    /// The ProgressiveAmount class acts as the amount tracker for
    /// a progressive level.
    /// </summary>
    [Serializable]
    public class ProgressiveAmount : ICompactSerializable
    {
        /// <summary>
        /// Amount displayed to the player, in base units.
        /// It is also the amount to be awarded once the progressive level is hit.
        /// </summary>
        public long Displayable { get; set; }

        /// <summary>
        /// Amount contributed to the progressive level after the
        /// displayable amount reaches the upper limit,
        /// in base units.
        /// </summary>
        public long Overflow { get; set; }

        /// <summary>
        /// Part of the contribution left over after the
        /// rounded integer has been added to the displayable
        /// amount, in base units.  It will
        /// be added to the next contribution before rounding. 
        /// </summary>
        public decimal Remainder { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return string.Format("Displayable({0})/Overflow({1})/Remainder({2:F6})",
                                 Displayable, Overflow, Remainder);
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, Displayable);
            CompactSerializer.Write(stream, Overflow);
            CompactSerializer.Write(stream, Remainder);
        }

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            Displayable = CompactSerializer.ReadLong(stream);
            Overflow = CompactSerializer.ReadLong(stream);
            Remainder = CompactSerializer.ReadDecimal(stream);
        }

        #endregion
    }
}
