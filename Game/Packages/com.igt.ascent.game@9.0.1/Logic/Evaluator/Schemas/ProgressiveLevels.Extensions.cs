// -----------------------------------------------------------------------
// <copyright file = "ProgressiveLevels.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the Progressive Levels class.
    /// </summary>
    public partial class ProgressiveLevels : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, ProgressiveLevel);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            ProgressiveLevel = CompactSerializer.ReadListSerializable<ProgressiveLevel>(stream);
        }

        #endregion
    }
}
