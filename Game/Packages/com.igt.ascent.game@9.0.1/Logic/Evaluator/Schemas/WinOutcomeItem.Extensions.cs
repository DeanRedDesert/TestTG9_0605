//-----------------------------------------------------------------------
// <copyright file = "WinOutcomeItem.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated WinOutcomeItem class.
    /// </summary>
    public partial class WinOutcomeItem : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Gets/sets an optional list of <see cref="Way"/> objects that represent the ways that the win matched.
        /// </summary>
        /// <remarks>This property can be <b>null</b>.</remarks>
        public IList<Way> MatchedWays { get; set; }

        /// <summary>
        /// Override function, provide content of win outcome item in string formaat.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("============WinOutcomeItem===============\n");
            if (Prize == null)
            {
                resultBuilder.AppendLine("Prize = NULL");
            }
            else
            {
                resultBuilder.Append(Prize);
            }
            if (Pattern == null)
            {
                resultBuilder.AppendLine("Pattern = NULL");
            }
            else
            {
                resultBuilder.Append(Pattern);
            }
            resultBuilder.AppendLine("WinOutcomeItem name = " + name);
            resultBuilder.AppendLine("winLevelIndex = " + winLevelIndex);
            resultBuilder.AppendLine("displayable = " + displayable);
            resultBuilder.AppendLine("displayableSpecified = " + displayableSpecified);
            resultBuilder.AppendLine("displayReason = " + displayReason);
            resultBuilder.AppendLine("============WinOutcomeItem items are listed.===============");
            return resultBuilder.ToString();
        }

        /// <summary>
        /// Constructor that initializes the <see cref="winLevelIndex"/> to be equal to max uint value when initialized.
        /// </summary>
        public WinOutcomeItem()
        {
            winLevelIndex = uint.MaxValue;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, winLevelIndex);

            CompactSerializer.Write(stream, displayable);
            CompactSerializer.Write(stream, displayableSpecified);
            CompactSerializer.Write(stream, displayReason);

            CompactSerializer.Write(stream, Prize);
            CompactSerializer.Write(stream, Pattern);

            CompactSerializer.WriteList(stream, MatchedWays);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);

            displayable = CompactSerializer.ReadBool(stream);
            displayableSpecified = CompactSerializer.ReadBool(stream);
            displayReason = CompactSerializer.ReadString(stream);

            Prize = CompactSerializer.ReadSerializable<Prize>(stream);
            Pattern = CompactSerializer.ReadSerializable<Pattern>(stream);

            MatchedWays = CompactSerializer.ReadListSerializable<Way>(stream);
        }

        #endregion

        #region IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new WinOutcomeItem
                       {
                           name = name,
                           winLevelIndex = winLevelIndex,
                           displayable = displayable,
                           displayableSpecified = displayableSpecified,
                           displayReason = displayReason,
                           Prize = NullableClone.DeepClone(Prize),
                           Pattern = NullableClone.DeepClone(Pattern),
                       };

            if(MatchedWays == null)
            {
                copy.MatchedWays = null;
            }
            else
            {
                copy.MatchedWays = new List<Way>(MatchedWays.Count);
                foreach(var way in MatchedWays)
                {
                    copy.MatchedWays.Add(NullableClone.DeepClone(way));
                }
            }

            return copy;
        }

        #endregion
    }
}
