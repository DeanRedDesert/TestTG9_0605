//-----------------------------------------------------------------------
// <copyright file = "EvaluationResult.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using Schemas;
    
    /// <summary>
    /// Class for storing the results of an evaluation.
    /// </summary>
    [Serializable]
    public class EvaluationResult : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// The win outcome for the evaluation.
        /// </summary>
        public WinOutcome WinOutcome { set; get; }

        /// <summary>
        /// The result symbol window of the evaluation.
        /// </summary>
        public CellPopulationOutcome SymbolWindow { set; get; }

        /// <summary>
        /// The name of this evaluation.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// The pay table section name that this evaluation is based on.
        /// </summary>
        public string PayTableSectionName { get; set; }

        /// <summary>
        /// The category that this evaluation is for.
        /// </summary>
        public EvaluationCategory Category { get; set; }

        /// <summary>
        /// Get the total win of this evaluation.
        /// </summary>
        public virtual long TotalWin
        {
            get
            {
                long win = 0;
                if (WinOutcome != null)
                {
                    WinOutcome.WinOutcomeItems
                        .ForEach(item => 
                            win += item.Prize.winAmount);
                }
                return win;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Create an evaluation result instance.
        /// </summary>
        /// <param name="payTableSectionName">The pay table section name that this evaluation is based on.</param>
        /// <param name="winOutcome">The win outcome for the evaluation.</param>
        /// <param name="symbolWindow">The symbol window for the evaluation.</param>
        public EvaluationResult(string payTableSectionName, WinOutcome winOutcome, CellPopulationOutcome symbolWindow)
        {
            PayTableSectionName = payTableSectionName;
            WinOutcome = winOutcome;
            SymbolWindow = symbolWindow;
        }

        /// <summary>
        /// Create a named evaluation result instance.
        /// </summary>
        /// <param name="name">The name of this evaluation.</param>
        /// <param name="category">The category that this evaluation is for.</param>
        /// <param name="payTableSectionName">The pay table section name that this evaluation is based on.</param>
        /// <param name="winOutcome">The win outcome for the evaluation.</param>
        /// <param name="symbolWindow">The resultant symbol window of the evaluation.</param>
        public EvaluationResult(string name,
                                EvaluationCategory category,
                                string payTableSectionName,
                                WinOutcome winOutcome,
                                CellPopulationOutcome symbolWindow)
            : this(payTableSectionName, winOutcome, symbolWindow)
        {
            Name = name;
            Category = category;
        }

        /// <summary>
        /// Create an anonymous evaluation result instance.
        /// </summary>
        /// <param name="category">The category that this evaluation is for.</param>
        /// <param name="payTableSectionName">The pay table section name that this evaluation is based on.</param>
        /// <param name="winOutcome">The win outcome for the evaluation.</param>
        /// <param name="symbolWindow">The resultant symbol window of the evaluation.</param>
        public EvaluationResult(EvaluationCategory category,
                                string payTableSectionName,
                                WinOutcome winOutcome,
                                CellPopulationOutcome symbolWindow)
            : this(payTableSectionName, winOutcome, symbolWindow)
        {
            Category = category;
        }


        /// <summary>
        /// Create an evaluation result instance.
        /// </summary>
        public EvaluationResult()
        {
        }

        #endregion

        /// <summary>
        /// Override function, provide content of EvaluationResult in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("======EvaluationResult===============\n");
            if (WinOutcome == null)
            {
                resultBuilder.AppendLine("WinOutcome = NULL");
            }
            else
            {
                resultBuilder.Append(WinOutcome);
            }
            if (SymbolWindow == null)
            {
                resultBuilder.AppendLine("SymbolWindow = NULL");
            }
            else
            {
                resultBuilder.AppendLine("SymbolWindow is below.");
                resultBuilder.Append(SymbolWindow);
            }
            resultBuilder.AppendLine("EvaluationResult Name = " + Name);
            resultBuilder.AppendLine("PayTableSectionName = " + PayTableSectionName);
            resultBuilder.AppendLine("Category = " + Category);
            resultBuilder.AppendLine("TotalWin = " + TotalWin);
            resultBuilder.AppendLine("======EvaluationResult items are listed.===============");
            return resultBuilder.ToString();
        }

        #region Implementation of ICompactSerializable

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, WinOutcome);
            CompactSerializer.Write(stream, SymbolWindow);
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, PayTableSectionName);
            CompactSerializer.Write(stream, (int)Category);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            WinOutcome = CompactSerializer.ReadSerializable<WinOutcome>(stream);
            SymbolWindow = CompactSerializer.ReadSerializable<CellPopulationOutcome>(stream);
            Name = CompactSerializer.ReadString(stream);
            PayTableSectionName = CompactSerializer.ReadString(stream);
            Category = (EvaluationCategory)CompactSerializer.ReadInt(stream);
        }

        #endregion
    }
}
