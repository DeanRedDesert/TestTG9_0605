//-----------------------------------------------------------------------
// <copyright file = "Pattern.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated Pattern class.
    /// </summary>
    public partial class Pattern : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Override function, provide content of Pattern in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("===============Pattern===============\n");
            resultBuilder.AppendFormat("Pattern Cluster: {0} \n", Cluster);
            if (SymbolList.Count == 0)
            {
                resultBuilder.AppendLine("SymbolList.Count is zero.");
            }
            else
            {
                for (var i = 0; i < SymbolList.Count; i++)
                {
                    resultBuilder.AppendFormat("SymbolList[{0}] = {1} \n", i, SymbolList[i]);
                }
            }
            resultBuilder.AppendLine("count = " + count);
            resultBuilder.AppendLine("Pattern name = " + name);
            resultBuilder.AppendLine("===============Pattern items are listed.===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, patternListName);
            CompactSerializer.Write(stream, count);
            CompactSerializer.Write(stream, Cluster);
            CompactSerializer.WriteList(stream, SymbolList);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            patternListName = CompactSerializer.ReadString(stream);
            count = CompactSerializer.ReadUint(stream);
            Cluster = CompactSerializer.ReadSerializable<Cluster>(stream);
            SymbolList = CompactSerializer.ReadListString(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new Pattern
                       {
                           name = name,
                           patternListName = patternListName,
                           count = count,
                           Cluster = NullableClone.DeepClone(Cluster),
                           SymbolList = NullableClone.ShallowCloneList(SymbolList)
                       };
            return copy;
        }

        #endregion
    }
}
