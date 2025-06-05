//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardData.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains data of the progressive award.
    /// </summary>
    [Serializable]
    public class ProgressiveAwardData : ICompactSerializable
    {
        /// <summary>
        /// Gets or sets the index of progressive award.
        /// </summary>
        public int AwardIndex { get; set; }

        /// <summary>
        /// Gets or sets the ID of progressive level.
        /// </summary>
        public string LevelId { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating if the start message has been sent or not.
        /// </summary>
        public bool HasSentStartMessage { get; set; }

        /// <summary>
        /// Gets or sets the amount of progressive award, in units of base denom.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Gets or sets the transferred amount of progressive award, in units of base denom.
        /// </summary>
        public long TransferredAmount { get; set; }

        /// <summary>
        /// Gets or sets the pay type of progressive award.
        /// </summary>
        public ProgressiveAwardPayType PayType { get; set; }

        /// <summary>
        /// Gets or sets the status of progressive award.
        /// </summary>
        public ProgressiveAwardStatus Status { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Progressive Award Data -");
            builder.AppendLine("\t AwardIndex = " + AwardIndex);
            builder.AppendLine("\t LevelId = " + LevelId);
            builder.AppendLine("\t HasSentStartMessage = " + HasSentStartMessage);
            builder.AppendLine("\t Amount = " + Amount);
            builder.AppendLine("\t TransferredAmount = " + TransferredAmount);
            builder.AppendLine("\t PayType = " + PayType);
            builder.AppendLine("\t Status = " + Status);

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, AwardIndex);
            CompactSerializer.Write(stream, LevelId);
            CompactSerializer.Write(stream, Amount);
            CompactSerializer.Write(stream, TransferredAmount);
            CompactSerializer.Write(stream, PayType);
            CompactSerializer.Write(stream, Status);
            CompactSerializer.Write(stream, HasSentStartMessage);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            AwardIndex = CompactSerializer.ReadInt(stream);
            LevelId = CompactSerializer.ReadString(stream);
            Amount = CompactSerializer.ReadLong(stream);
            TransferredAmount = CompactSerializer.ReadLong(stream);
            PayType = CompactSerializer.ReadEnum<ProgressiveAwardPayType>(stream);
            Status = CompactSerializer.ReadEnum<ProgressiveAwardStatus>(stream);
            HasSentStartMessage = CompactSerializer.ReadBool(stream);
        }

        #endregion
    }
}
