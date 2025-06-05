//-----------------------------------------------------------------------
// <copyright file = "RequiredWinOutcomeItemData.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

 
namespace IGT.Game.Core.Logic.Evaluator
{
    /// <summary>
    /// Object for encapsulating data which is required when adding a win outcome item, but is not required for
    /// the evaluation itself.
    /// </summary>
    public class RequiredWinOutcomeItemData
    {
        /// <summary>
        /// The prize scale name to associate with the win outcome item.
        /// </summary>
        public string PrizeScaleName { get; private set; }

        /// <summary>
        /// The pattern list name to associate with the win outcome item.
        /// </summary>
        public string PatternListName { get; private set; }

        /// <summary>
        /// The population name to associate with the win outcome item.
        /// </summary>
        public string PopulationName { get; private set; }

        /// <summary>
        /// Create an instance of RequiredOutcomeItemData.
        /// </summary>
        /// <param name="prizeScaleName">The prize scale name to associate with the win outcome item.</param>
        /// <param name="patternListName">The pattern list name to associate with the win outcome item.</param>
        /// <param name="populationName">The population name to associate with the win outcome item.</param>
        public RequiredWinOutcomeItemData(string prizeScaleName, string patternListName, string populationName)
        {
            PrizeScaleName = prizeScaleName;
            PatternListName = patternListName;
            PopulationName = populationName;
        }
    }
}
