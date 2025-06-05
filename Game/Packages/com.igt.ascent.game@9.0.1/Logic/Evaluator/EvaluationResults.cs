//-----------------------------------------------------------------------
// <copyright file = "EvaluationResults.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// A container which exposes the evaluation results of a game.
    /// This container may contains results from more than one play section, like base game, bonus and double up.
    /// </summary>
    [Serializable]
    public class EvaluationResults : ICompactSerializable
    {
        #region Private fields

        /// <summary>
        /// List which keeps the evaluation results
        /// </summary>
        private List<EvaluationResult> evaluations;

        #endregion

        #region Public Properties

        /// <summary>
        /// The total win of all the evaluation results.
        /// </summary>
        public long TotalWin
        {
            get
            {
                long totalWin = 0;
                evaluations.ForEach(e => totalWin += e.TotalWin);
                return totalWin;
            }
        }

        /// <summary>
        /// All the evaluation result items.
        /// </summary>
        public List<EvaluationResult> AllEvaluations
        {
            get
            {
                return evaluations;
            }
        }

        /// <summary>
        /// The latest evaluation result item.
        /// </summary>
        public EvaluationResult LatestEvaluation
        {
            get
            {
                return evaluations.LastOrDefault();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new EvaluationResults instance.
        /// </summary>
        public EvaluationResults()
        {
            evaluations = new List<EvaluationResult>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a evaluation result item to this container.
        /// </summary>
        /// <param name="result">The evaluation result item.</param>
        public void AddEvaluation(EvaluationResult result)
        {
            evaluations.Add(result);
        }

        /// <summary>
        /// Get a evaluation result item with specified name.
        /// </summary>
        /// <param name="name">Name of the evaluation result item.</param>
        /// <returns>The evaluation result item.</returns>
        public EvaluationResult GetEvaluationByName(string name)
        {
            return evaluations.Find(e => e.Name == name);
        }

        /// <summary>
        /// Get all the evaluation result items with specified category.
        /// </summary>
        /// <param name="category">Category of evaluation result items.</param>
        /// <returns>The list of evaluation result items.</returns>
        public List<EvaluationResult> GetEvaluationsForCategory(EvaluationCategory category)
        {
            return evaluations.FindAll(e => e.Category == category);
        }

        /// <summary>
        /// Get the latest evaluation result item with specified category.
        /// </summary>
        /// <param name="category">Category of evaluation result items.</param>
        /// <returns>The latest evaluation result item with specified category.</returns>
        public EvaluationResult GetLatestEvaluationForCategory(EvaluationCategory category)
        {
            return GetEvaluationsForCategory(category).LastOrDefault();
        }

        /// <summary>
        /// Get the total win of all the evaluation results with specified category.
        /// </summary>
        /// <param name="category">Category of evaluation result items.</param>
        /// <returns>The total win of all the evaluation results with specified category.</returns>
        public long GetTotalWinForCategory(EvaluationCategory category)
        {
            long win = 0;
            GetEvaluationsForCategory(category).ForEach(e => win += e.TotalWin);
            return win;
        }

        #endregion

        /// <summary>
        /// Override function, provide content of EvaluationResults in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("===EvaluationResults===============\n");
            if (AllEvaluations.Count == 0)
            {
                resultBuilder.AppendLine("AllEvaluations.Count is zero.");
            }
            else
            {
                var count = 0;
                foreach(var evaluation in AllEvaluations)
                {
                    resultBuilder.AppendFormat("EvaluationResult[{0}] \n", count);
                    resultBuilder.Append(evaluation);
                    ++count;
                }
            }
            resultBuilder.AppendLine("TotalWin = " + TotalWin);
            if(LatestEvaluation == null)
            {
                resultBuilder.AppendLine("LatestEvaluation = NULL");
            }
            else
            {
                resultBuilder.AppendLine("LatestEvaluation is below");
                resultBuilder.Append(LatestEvaluation);
            }
            resultBuilder.AppendLine("===EvaluationResults items are listed.===============");
            return resultBuilder.ToString();
        }

        #region Implementation of ICompactSerializable

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, evaluations);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            evaluations = CompactSerializer.ReadListSerializable<EvaluationResult>(stream);
        }

        #endregion
    }
}