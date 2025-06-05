// -----------------------------------------------------------------------
// <copyright file = "OutcomeListProgressiveHelper.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController.ServiceProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// Helper class for obtaining specific progressive data from an <see cref="Ascent.OutcomeList.OutcomeList"/>.
    /// </summary>
    public static class OutcomeListProgressiveHelper
    {
        /// <summary>
        /// Obtains the progressive lock values from a given <see cref="Ascent.OutcomeList.OutcomeList"/>. This data will be used to lock
        /// the progressive values in the <see cref="ProgressiveProvider"/>.
        /// </summary>
        /// <remarks>
        /// This method retrieves the progressive lock levels and amount values with the following constraints:
        /// 1) The progressive level must be present in the <see cref="Ascent.OutcomeList.FeatureProgressiveAward"/> list in the FeatureEntry.
        /// 2) The progressive level must also have a <see cref="ProgressiveAwardHitState"/> of Hit.
        /// Note: The award can be either displayable or not displayable.  
        /// </remarks>
        /// <param name="outcomeList">
        /// OutcomeList used to obtain progressive data.
        /// </param>
        /// <returns>
        /// Dictionary of progressive lock values indexed by game level.
        /// </returns>
        public static Dictionary<long, long> GetProgressiveLockValues(IOutcomeList outcomeList)
        {
            if(outcomeList == null)
            {
                return new Dictionary<long, long>();
            }

            // Obtain progressive lock values if there is a progressive hit.
            return outcomeList.GetFeatureEntries()
                .Where(entry => entry.ContainsFeatureAwardOutcome)
                .SelectMany(entry => entry.GetAwards<FeatureAward>())
                .SelectMany(featureAward => featureAward.GetFeatureProgressiveAwards(),
                    (featureAward, award) => new { featureAward, award })
                .Where(featureProgressiveAward =>
                    featureProgressiveAward.award.HitState == ProgressiveAwardHitState.Hit)
                .Select(featureProgressiveAward => featureProgressiveAward.award)
                    .ToDictionary<IFeatureProgressiveAward, long, long>
                        (award => award.GameLevel ?? 0, award => award.AmountValue);
        }
    }
}
