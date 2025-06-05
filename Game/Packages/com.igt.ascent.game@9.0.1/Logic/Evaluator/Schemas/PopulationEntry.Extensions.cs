//-----------------------------------------------------------------------
// <copyright file = "PopulationEntry.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated PopulationEntry class.
    /// </summary>
    public partial class PopulationEntry : ICompactSerializable
    {
        /// <summary>
        /// Override function, provide content of PopulationEntry in string formaat.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("==================PopulationEntry===============\n");
            if(OutcomeCellList.Count == 0)
            {
                resultBuilder.AppendLine("OutcomeCellList.Count is zero.");
            }
            else
            {
                var count = 0;
                foreach(var outcomeCell in OutcomeCellList)
                {
                    resultBuilder.AppendFormat("OutcomeCellList[{0}] \n", count);
                    resultBuilder.Append(outcomeCell);
                    ++count;
                }
            }
            resultBuilder.AppendLine("PopulationEntry name = " + name);
            resultBuilder.AppendLine("==================PopulationEntry items are listed.===============");
            return resultBuilder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.WriteList(stream, OutcomeCellList);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            OutcomeCellList = CompactSerializer.ReadListSerializable<OutcomeCell>(stream);
        }

        #endregion
    }
}
