// -----------------------------------------------------------------------
// <copyright file = "ProgressiveLevel.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the Progressive Level class.
    /// </summary>
    public partial class ProgressiveLevel : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ProgressiveName);
            CompactSerializer.Write(stream, Level);
            CompactSerializer.Write(stream, ConsolationPay);
            CompactSerializer.Write(stream, ConsolationPaySpecified);
            CompactSerializer.Write(stream, MultiplyConsolationPayByDenom);
            CompactSerializer.Write(stream, MultiplyConsolationPayByDenomSpecified);
            CompactSerializer.Write(stream, MinimumContribution);
            CompactSerializer.Write(stream, ControllerType);
            CompactSerializer.Write(stream, ControllerSubType);
            CompactSerializer.Write(stream, ControllerLevel);
            CompactSerializer.Write(stream, ControllerLevelSpecified);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            ProgressiveName = CompactSerializer.ReadString(stream);
            Level = CompactSerializer.ReadUint(stream);
            ConsolationPay = CompactSerializer.ReadLong(stream);
            ConsolationPaySpecified = CompactSerializer.ReadBool(stream);
            MultiplyConsolationPayByDenom = CompactSerializer.ReadBool(stream);
            MultiplyConsolationPayByDenomSpecified = CompactSerializer.ReadBool(stream);
            MinimumContribution = CompactSerializer.ReadString(stream);
            ControllerType = CompactSerializer.ReadString(stream);
            ControllerSubType = CompactSerializer.ReadString(stream);
            ControllerLevel = CompactSerializer.ReadUint(stream);
            ControllerLevelSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion
    }
}
