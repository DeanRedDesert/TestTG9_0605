//-----------------------------------------------------------------------
// <copyright file = "SlotAnticipationEvaluationProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using IGT.Game.Core.Logic.Evaluator.Schemas;
using IGT.Game.Core.Logic.Services;

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    /// <summary>
    /// Provider which exposes the results of a slot anticipation evaluation.
    /// </summary>
    public class SlotAnticipationEvaluationProvider
    {
        /// <summary>
        /// List of win out come items suitable for use by the presentation.
        /// </summary>
        [GameService]
        public List<WinOutcomeItem> WinOutComeList { private set; get; }

        /// <summary>
        /// Repopulate the WinOutComeList based off of a win outcome list.
        /// </summary>
        /// <param name="winOutcomeItems">The win outcome list to use for updating the win outcome list.</param>
        public void UpdateWins(List<WinOutcomeItem> winOutcomeItems)
        {
            WinOutComeList = winOutcomeItems;
        }
    }
}
