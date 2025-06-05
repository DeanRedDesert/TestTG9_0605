//-----------------------------------------------------------------------
// <copyright file = "WinOutcomeUtilities.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;
    using Communication.Foundation;
    using ProgressiveController;
    using Schemas;

    /// <summary>
    /// Class for managing the translation to and from the OutcomeList.
    /// </summary>
    public static class WinOutcomeUtilities
    {
        /// <summary>
        /// Prefix for name of a win outcome.
        /// </summary>
        private const string WinOutcomePrefix = "_WO";

        #region Public Methods

        /// <summary>
        /// Merge the given outcome list into the given win outcomes list. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts. Any adjustment awards created by the foundation will
        /// be added to the win outcomes.
        /// </summary>
        /// <param name="winOutcomes">List of win outcomes to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcomes.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome or when there are no win outcomes in the list to merge.
        /// </exception>
        /// <returns>
        /// A list of wins which were not adjusted. The value of these wins will not be counted toward the total win
        /// amount. Generally this list should be empty, if there are any items in it, then it is the responsibility
        /// of the developer to reconcile the difference. If extras are not expected, then SafeMergeOutcome should
        /// be used instead.
        /// </returns>
        public static IEnumerable<WinOutcomeItem> MergeOutcomes(IList<WinOutcome> winOutcomes, IOutcomeList outcomeList)
        {
            return MergeOutcomes(winOutcomes, outcomeList, item => true);
        }

        /// <summary>
        /// Merge the given outcome list into the given win outcomes list. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts. Any adjustment awards created by the foundation will
        /// be added to the first win outcome if the given predicate returns <b>true</b> for a given award.
        /// </summary>
        /// <param name="winOutcomes">List of win outcomes to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcomes.</param>
        /// <param name="includeGameCycleAdjustmentInWin">
        /// A predicate that is called for each game cycle adjustment. If it returns false the 
        /// <see cref="WinOutcomeItem"/> will be filtered from the merge, otherwise it will be added to the wins in 
        /// the first <see cref="WinOutcome"/>. Any items that are filtered must be tracked separately by the caller.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome or when there are no win outcomes in the list to merge.
        /// </exception>
        /// <returns>
        /// A list of wins which were not adjusted. The value of these wins will not be counted toward the total win
        /// amount. Generally this list should be empty, if there are any items in it, then it is the responsibility
        /// of the developer to reconcile the difference. If extras are not expected, then SafeMergeOutcome should
        /// be used instead.
        /// </returns>
        /// <remarks>
        /// Any <see cref="WinOutcomeItem"/>s that are filtered by the predicate must be tracked separately by the 
        /// caller. Failure to do so will result in the game's win not being fully reconciled with the actual win
        /// recorded by the Foundation. This can result in an incorrect amount being presented to the player.
        /// </remarks>
        public static IEnumerable<WinOutcomeItem> MergeOutcomes(
            IList<WinOutcome> winOutcomes,
            IOutcomeList outcomeList,
            Func<WinOutcomeItem, bool> includeGameCycleAdjustmentInWin)
        {
            if(outcomeList == null)
            {
                throw new ArgumentNullException("outcomeList", "Argument may not be null");
            }

            if(winOutcomes == null)
            {
                throw new ArgumentNullException("winOutcomes", "Argument may not be null");
            }

            if(winOutcomes.Count == 0 && outcomeList.GetFeatureEntries().Count != 0)
            {
                throw new OutcomeListMergeException(
                    "There are no win outcomes present to merge with the adjusted outcome.");
            }

            if(includeGameCycleAdjustmentInWin == null)
            {
                throw new ArgumentNullException("includeGameCycleAdjustmentInWin");
            }

            var countedWins = new List<WinOutcomeItem>();
            var uncountedWins = new List<WinOutcomeItem>();
            var isFeatureAwardMatched = new Dictionary<string, bool>();

            for(var i = 0; i < winOutcomes.Count; i++)
            {
                if(winOutcomes[i] == null)
                {
                    throw new OutcomeListMergeException(
                        "You cannot have null items in win outcomes.");
                }

                foreach(var featureEntry in outcomeList.GetFeatureEntries())
                {
                    foreach(var award in featureEntry.GetAwards<IAward>())
                    {
                        if(!isFeatureAwardMatched.ContainsKey(award.Tag))
                        {
                            isFeatureAwardMatched[award.Tag] = false;
                        }

                        var winName = award.Tag;
                        var totalAwardAmount = award.AmountValue;
                        var matchingWins =
                            (from win in winOutcomes[i].WinOutcomeItems
                             where win.name == winName
                             select win).ToList();

                        if(matchingWins.Count == 0)
                        {
                            continue;
                        }

                        if(matchingWins.Count > 1)
                        {
                            throw new OutcomeListMergeException(
                                "Multiple win outcome items share the same name. Outcome could not be merged back to win outcome.");
                        }

                        isFeatureAwardMatched[award.Tag] = true;

                        var matchingWin = matchingWins[0];
                        matchingWin.name = RemoveString(winOutcomes[i].name, matchingWin.name);
                        countedWins.Add(matchingWin);
                        matchingWin.displayable = award.IsDisplayable;
                        matchingWin.displayableSpecified = true;
                        matchingWin.displayReason = award.DisplayableReason;
                        var featureAward = award as FeatureAward;

                        if(featureAward != null)
                        {
                            totalAwardAmount +=
                                featureAward.GetFeatureProgressiveAwards().Where(
                                    progressiveAward => progressiveAward.IsDisplayable)
                                    .Sum(progressiveAward => progressiveAward.AmountValue);

                            matchingWin.Prize.ProgressivePrizeStrings =
                                featureAward.GetFeatureProgressiveAwards().Select(featureProgressiveAward => featureProgressiveAward.PrizeString).Where
                                (featureProgressiveAward => !string.IsNullOrEmpty(featureProgressiveAward)).ToList();
                        }

                        matchingWin.Prize.winAmount = totalAwardAmount;
                        winOutcomes[i].totalWinAmount += totalAwardAmount;
                    }
                }

                //Determine which, if any, wins in the win outcome were not adjusted.
                uncountedWins.AddRange(winOutcomes[i].WinOutcomeItems.Except(countedWins));
                winOutcomes[i].name = RemoveString(WinOutcomePrefix + (i + 1) + '_', winOutcomes[i].name);
            }

            // If foundation there are any game cycle entries from foundation
            // add them to the first win outcome.
            AddFoundationGameCycleAdjustments(winOutcomes[0], outcomeList.GetGameCycleEntries(), includeGameCycleAdjustmentInWin);

            if(isFeatureAwardMatched.ContainsValue(false))
            {
                throw new OutcomeListMergeException(
                    "One or more win outcomes were not matched with the adjusted outcome list.");
            }

            return uncountedWins;
        }

        /// <summary>
        /// Removes the <paramref name="stringToBeRemoved"/> from <paramref name="fromString"/> if it exists.
        /// </summary>
        /// <param name="stringToBeRemoved">String that is to be removed.</param>
        /// <param name="fromString">String from which <paramref name="stringToBeRemoved"/> is to be removed from.</param>
        /// <returns>A modified form of <paramref name="fromString"/> if <paramref name="stringToBeRemoved"/> exists.</returns>
        private static string RemoveString(string stringToBeRemoved, string fromString)
        {
            if(string.IsNullOrEmpty(fromString) || string.IsNullOrEmpty(stringToBeRemoved))
            {
                return fromString;
            }

            var index = fromString.LastIndexOf(stringToBeRemoved, StringComparison.Ordinal);
            return index < 0 ? fromString : fromString.Remove(index, stringToBeRemoved.Length);
        }

        /// <summary>
        /// Merge the given outcome list into the given win outcome. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts.  Any adjustment awards created by the foundation will
        /// be added to the win outcome if the given predicate returns <b>true</b> for a given award.
        /// </summary>
        /// <param name="winOutcome">Win outcome to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcome.</param>
        /// <param name="includeGameCycleAdjustmentInWin">
        /// A predicate that is called for each game cycle adjustment. If it returns false the 
        /// <see cref="WinOutcomeItem"/> will be filtered from the merge, otherwise it will be added to the wins in 
        /// the first <see cref="WinOutcome"/>. Any items that are filtered must be tracked separately by the caller.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome.
        /// </exception>
        /// <returns>
        /// A list of wins which were not adjusted. The value of these wins will not be counted toward the total win
        /// amount. Generally this list should be empty, if there are any items in it, then it is the responsibility
        /// of the developer to reconcile the difference. If extras are not expected, then SafeMergeOutcome should
        /// be used instead.
        /// </returns>
        /// <remarks>
        /// Any <see cref="WinOutcomeItem"/>s that are filtered by the predicate must be tracked separately by the 
        /// caller. Failure to do so will result in the game's win not being fully reconciled with the actual win
        /// recorded by the Foundation. This can result in an incorrect amount being presented to the player.
        /// </remarks>
        public static IEnumerable<WinOutcomeItem> MergeOutcome(
            WinOutcome winOutcome,
            OutcomeList outcomeList,
            Func<WinOutcomeItem, bool> includeGameCycleAdjustmentInWin)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }

            return MergeOutcomes(
                new List<WinOutcome>
                {
                    winOutcome
                },
                outcomeList,
                includeGameCycleAdjustmentInWin);
        }

        /// <summary>
        /// Merge the given outcome list into the given win outcome. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts. Any adjustment awards created by the foundation will
        /// be added to the win outcome.
        /// </summary>
        /// <param name="winOutcome">Win outcome to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcome.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome.
        /// </exception>
        /// <returns>
        /// A list of wins which were not adjusted. The value of these wins will not be counted toward the total win
        /// amount. Generally this list should be empty, if there are any items in it, then it is the responsibility
        /// of the developer to reconcile the difference. If extras are not expected, then SafeMergeOutcome should
        /// be used instead.
        /// </returns>
        public static IEnumerable<WinOutcomeItem> MergeOutcome(WinOutcome winOutcome, IOutcomeList outcomeList)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }

            return MergeOutcomes(new List<WinOutcome> { winOutcome }, outcomeList);
        }

        /// <summary>
        /// Merge the given outcome list into the given list of win outcomes. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts. Any adjustment awards created by the foundation will
        /// be added to the win outcomes.
        /// </summary>
        /// <param name="winOutcomes">List of win outcomes to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcomes.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome. Also thrown when the winOutcome contains items which were not in the outcome list.
        /// </exception>
        public static void SafeMergeOutcomes(IList<WinOutcome> winOutcomes, IOutcomeList outcomeList)
        {
            var extraWins = MergeOutcomes(winOutcomes, outcomeList);
            if(extraWins.Count() != 0)
            {
                throw new OutcomeListMergeException(
                    "WinOutcome contained wins which were not included in the OutcomeList.");
            }
        }

        /// <summary>
        /// Merge the given outcome list into the given list of win outcomes. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts.  Any adjustment awards created by the foundation will
        /// be added to the first win outcome if the given predicate returns <b>true</b> for a given award.
        /// </summary>
        /// <param name="winOutcomes">List of win outcomes to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcomes.</param>
        /// <param name="includeGameCycleAdjustmentInWin">
        /// A predicate that is called for each game cycle adjustment. If it returns false the 
        /// <see cref="WinOutcomeItem"/> will be filtered from the merge, otherwise it will be added to the wins in 
        /// the first <see cref="WinOutcome"/>. Any items that are filtered must be tracked separately by the caller.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome. Also thrown when the winOutcome contains items which were not in the outcome list.
        /// </exception>
        /// <remarks>
        /// Any <see cref="WinOutcomeItem"/>s that are filtered by the predicate must be tracked separately by the 
        /// caller. Failure to do so will result in the game's win not being fully reconciled with the actual win
        /// recorded by the Foundation. This can result in an incorrect amount being presented to the player.
        /// </remarks>
        public static void SafeMergeOutcomes(
            IList<WinOutcome> winOutcomes,
            OutcomeList outcomeList,
            Func<WinOutcomeItem, bool> includeGameCycleAdjustmentInWin)
        {
            var extraWins = MergeOutcomes(winOutcomes, outcomeList, includeGameCycleAdjustmentInWin);
            if(extraWins.Count() != 0)
            {
                throw new OutcomeListMergeException(
                    "Win Outcome contained wins which were not included in the OutcomeList.");
            }
        }

        /// <summary>
        /// Merge the given outcome list into the given win outcome. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts. Any adjustment awards created by the foundation will
        /// be added to the win outcome.
        /// </summary>
        /// <param name="winOutcome">Win outcome to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcome.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome. Also thrown when the winOutcome contains items which were not in the outcome list.
        /// </exception>
        public static void SafeMergeOutcome(WinOutcome winOutcome, IOutcomeList outcomeList)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }

            SafeMergeOutcomes(new List<WinOutcome> { winOutcome }, outcomeList);
        }

        /// <summary>
        /// Merge the given outcome list into the given win outcome. Progressive feature entries are updated with any
        /// changes made by the foundation to their win amounts.  Any adjustment awards created by the foundation will
        /// be added to the first win outcome if the given predicate returns <b>true</b> for a given award.
        /// </summary>
        /// <param name="winOutcome">Win outcome to merge the outcome list into.</param>
        /// <param name="outcomeList">Outcome list to merge into the win outcome.</param>
        /// <param name="includeGameCycleAdjustmentInWin">
        /// A predicate that is called for each game cycle adjustment. If it returns false the 
        /// <see cref="WinOutcomeItem"/> will be filtered from the merge, otherwise it will be added to the wins in 
        /// the first <see cref="WinOutcome"/>. Any items that are filtered must be tracked separately by the caller.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when any parameters are null.</exception>
        /// <exception cref="OutcomeListMergeException">
        /// Thrown when multiple win outcome items share the same name preventing the OutcomeList from being merged
        /// into the WinOutcome. Also thrown when the winOutcome contains items which were not in the outcome list.
        /// </exception>
        /// <remarks>
        /// Any <see cref="WinOutcomeItem"/>s that are filtered by the predicate must be tracked separately by the 
        /// caller. Failure to do so will result in the game's win not being fully reconciled with the actual win
        /// recorded by the Foundation. This can result in an incorrect amount being presented to the player.
        /// </remarks>
        public static void SafeMergeOutcome(
            WinOutcome winOutcome,
            OutcomeList outcomeList,
            Func<WinOutcomeItem, bool> includeGameCycleAdjustmentInWin)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }

            SafeMergeOutcomes(
                new List<WinOutcome>
                {
                    winOutcome
                },
                outcomeList,
                includeGameCycleAdjustmentInWin);
        }

        /// <summary>
        /// Create feature entries from the specified list of winOutcomes.
        /// </summary>
        /// <param name="winOutcomePackages">WinOutcome packages to generate a feature entries from.</param>
        /// <param name="progressiveController">The game side progressive controller that validates GCP awards.</param>
        /// <param name="winOutcomes">List of win outcomes and its win outcome items with updated names.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <returns>List of feature entries with the wins contained in the win outcomes.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="winOutcomePackages"/> is null.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Thrown when any of the WinOutcomes in the packages is null.
        /// </exception>
        public static List<FeatureEntry> CreateFeatureEntries(IList<WinOutcomePackage> winOutcomePackages, out IList<WinOutcome> winOutcomes,
            IProgressiveController progressiveController, long denomination)
        {
            if(winOutcomePackages == null)
            {
                throw new ArgumentNullException("winOutcomePackages");
            }

            winOutcomes = new List<WinOutcome>(winOutcomePackages.Count);

            var featureEntries = new List<FeatureEntry>();
            for(var i = 0; i < winOutcomePackages.Count; i++)
            {
                if(winOutcomePackages[i].WinOutcome == null)
                {
                    throw new NullReferenceException("One or more win outcomes are null in the WinOutcomePackages");
                }
                winOutcomePackages[i].WinOutcome.name += WinOutcomePrefix + (i + 1) + '_';
                var featureEntry = CreateFeatureEntry(winOutcomePackages[i].WinOutcome,
                    winOutcomePackages[i].ProgressiveLevels,
                    winOutcomePackages[i].FeatureIndex, winOutcomePackages[i].RandomNumbers, progressiveController,
                    denomination);
                featureEntries.Add(featureEntry);
                winOutcomes.Add(winOutcomePackages[i].WinOutcome);
            }

            return featureEntries;
        }

        /// <summary>
        /// Creates list of feature entries from the specified list of winOutcome packages.
        /// </summary>
        /// <param name="winOutcomePackages">WinOutcome packages to generate feature entries from.</param>
        /// <param name="winOutcomes">List of win outcomes and its win outcome items with updated names.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <returns>List of feature entries with the wins contained in the win outcomes.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="winOutcomePackages"/> is null.
        /// </exception>
        public static List<FeatureEntry> CreateFeatureEntries(IList<WinOutcomePackage> winOutcomePackages,
            out IList<WinOutcome> winOutcomes, long denomination)
        {
            return CreateFeatureEntries(winOutcomePackages, out winOutcomes, null, denomination);
        }

        /// <summary>
        /// Create a feature entry for the specified winOutcome package.
        /// </summary>
        /// <param name="winOutcomePackage">The winOutcom package containing the winOutcome and other related data.</param>
        /// <param name="newWinOutcome">A modified winOutcome which should be used while merging win outcomes.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <returns>A feature entry with the wins contained in the WinOutcome.</returns>
        public static FeatureEntry CreateFeatureEntry(WinOutcomePackage winOutcomePackage,
            out WinOutcome newWinOutcome, long denomination)
        {
            return CreateFeatureEntry(winOutcomePackage, out newWinOutcome, null, denomination);
        }

        /// <summary>
        /// Create a feature entry for the specified winOutcome package, where the GCP progressive
        /// awards are validated by a game side progressive controller.
        /// </summary>
        /// <param name="winOutcomePackage">The winOutcom package containing the winOutcome and other related data.</param>
        /// <param name="newWinOutcome">A modified winOutcome which should be used while merging win outcomes.</param>
        /// <param name="iProgressiveController">The game side progressive controller that validates GCP awards.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <returns>A feature entry with the wins contained in the WinOutcome.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when winOutcomePackage is null.
        /// </exception>
        public static FeatureEntry CreateFeatureEntry(WinOutcomePackage winOutcomePackage, out WinOutcome newWinOutcome,
             IProgressiveController iProgressiveController, long denomination)
        {
            if(winOutcomePackage == null)
            {
                throw new ArgumentNullException("winOutcomePackage", "Argument may not be null");
            }

            IList<WinOutcome> winOutcomes;
            var entries = CreateFeatureEntries(new List<WinOutcomePackage> { winOutcomePackage }, out winOutcomes, iProgressiveController, denomination);
            newWinOutcome = winOutcomes.FirstOrDefault();
            return entries.FirstOrDefault();
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Create a feature entry for the specified winOutcome, where the GCP progressive
        /// awards are validated by a game side progressive controller.
        /// </summary>
        /// <param name="winOutcome">WinOutcome to generate a feature entry for.</param>
        /// <param name="progressiveLevels">Progressive information for the paytable associated with the WinOutcome.</param>
        /// <param name="featureIndex">The feature index for the wins in the WinOutcome.</param>
        /// <param name="randomNumbers">The random numbers associated with the WinOutcome.
        /// If no random numbers available, please leave this parameter as null.
        /// </param>
        /// <param name="iProgressiveController">The game side progressive controller that validates GCP awards.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <returns>A feature entry with the wins contained in the WinOutcome.</returns>
        /// <exception cref="WinOutcomeItemException">
        /// Thrown when winOutcomeItem name is null/empty or winLevelIndex is invalid.
        /// </exception>
        private static FeatureEntry CreateFeatureEntry(WinOutcome winOutcome,
                                                                 ProgressiveLevels progressiveLevels, uint featureIndex,
                                                                 IList<int> randomNumbers,
                                                                 IProgressiveController iProgressiveController,
                                                                 long denomination)
        {
            var featureEntry = new FeatureEntry(featureIndex);

            // Add FeatureRngNumbers only if randomNumbers exists.
            if(randomNumbers != null && randomNumbers.Count != 0)
            {
                featureEntry.AddFeatureRngNumbers(randomNumbers);
            }

            foreach(var winOutcomeItem in winOutcome.WinOutcomeItems)
            {
                if(string.IsNullOrEmpty(winOutcomeItem.name))
                {
                    throw new WinOutcomeItemException("Win Outcome Name", "Cannot be null or empty");
                }

                if(winOutcomeItem.winLevelIndex == uint.MaxValue)
                {
                    throw new WinOutcomeItemException("Win Level Index", "Has invalid value");
                }

                winOutcomeItem.name = winOutcome.name + winOutcomeItem.name;
                var featureAward =
                    new FeatureAward(
                        amount: winOutcomeItem.Prize.winAmount,
                        //Indicate that the win was added by the game.
                        origin: OutcomeOrigin.Client,
                        //The tag is used to merge the OutcomeList back into the win outcome after the foundation has made
                        //any adjustments.
                        tag: winOutcomeItem.name,
                        isDisplayable: true,
                        winLevel: winOutcomeItem.winLevelIndex
                    );

                if(winOutcomeItem.Prize.nearHitProgressive)
                {
                    featureAward.AddProgressiveNearHit(new ProgressiveNearHit());
                }

                featureEntry.AddAward(featureAward);

                //In addition to the monetary win add an award for each progressive which is part of the prize.
                AddProgressiveAwards(featureAward, winOutcomeItem, progressiveLevels, iProgressiveController, denomination);
            }

            return featureEntry;
        }

        /// <summary>
        /// The Foundation may add adjustments to the win outcome. These adjustments award additional prizes, or add
        /// negative adjustment amounts.
        /// </summary>
        /// <param name="winOutcome">Win outcome to add adjustments to.</param>
        /// <param name="gameCycleEntries">List of game cycle entries to add adjustments from.</param>
        /// <param name="includeInWin">
        /// A predicate that is used to determine if an adjustment should be included in the win. If it returns false, 
        /// the <see cref="WinOutcomeItem"/> will not be included. Otherwise it will be added to the the wins in 
        /// <paramref name="winOutcome"/>.
        /// </param>
        /// <remarks>
        /// If a game wishes to make a game cycle award, then the game is responsible for converting it to be in the
        /// win outcome and ensuring correct display.
        /// </remarks>
        private static void AddFoundationGameCycleAdjustments(WinOutcome winOutcome,
                                                              IEnumerable<IGameCycleEntry> gameCycleEntries,
                                                              Func<WinOutcomeItem, bool> includeInWin)
        {
            foreach(var gameCycleEntry in gameCycleEntries)
            {
                IEnumerable<IAward> awards = gameCycleEntry.GetAwards();

                awards = awards.Concat(gameCycleEntry.GetProgressiveAwards().Select(progressiveAward => progressiveAward as IAward));

                foreach(var award in awards)
                {
                    //Only process awards from the Foundation.
                    if(award.Origin == OutcomeOrigin.Foundation)
                    {
                        var item = new WinOutcomeItem
                        {
                            displayable = award.IsDisplayable,
                            displayableSpecified = true,
                            displayReason = award.DisplayableReason,
                            name =
                                award.Origin + ":" + award.Tag + ":" + award.Source,
                            Prize =
                                new Prize
                                {
                                    winAmount = award.AmountValue,
                                }
                        };

                        if(includeInWin(item))
                        {
                            winOutcome.WinOutcomeItems.Add(item);
                            winOutcome.totalWinAmount += award.AmountValue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add progressive awards from the specified win outcome item to the given feature award.
        /// Validate GCP awards using a game side progressive controller.
        /// </summary>
        /// <param name="featureAward">FeatureAward to add progressive information to.</param>
        /// <param name="winOutcomeItem">Win outcome item to add progressive information for.</param>
        /// <param name="progressiveLevels">ProgressiveLevels used to get information for the progressive wins.</param>
        /// <param name="iProgressiveController">The game side progressive controller that validates GCP awards.</param>
        /// <param name="denomination">The denomination of the game.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when a progressive level could not be found in the ProgressiveLevels.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the passed in progressive controller is invalid while GCP awards exist in the win outcome item.
        /// </exception>
        private static void AddProgressiveAwards(FeatureAward featureAward, WinOutcomeItem winOutcomeItem,
                                                 ProgressiveLevels progressiveLevels, IProgressiveController iProgressiveController, long denomination)
        {
            if(progressiveLevels == null)
            {
                if(winOutcomeItem.Prize.ProgressiveLevels.Count != 0)
                {
                    throw new ArgumentNullException("progressiveLevels", "Argument may not be null unless winOutcomeItem.Prize.ProgressiveLevels is empty.");
                }
                return;
            }

            foreach(var progressiveLevel in winOutcomeItem.Prize.ProgressiveLevels)
            {
                //Get the paytable information for this progressive.
                var progressiveInformation =
                    (from level in progressiveLevels.ProgressiveLevel
                     where level.ProgressiveName == progressiveLevel
                     select level).FirstOrDefault();

                if(progressiveInformation == null)
                {
                    throw new KeyNotFoundException("Progressive: " + progressiveLevel + " could not be found.");
                }

                var progressiveAward = new FeatureProgressiveAward
                (
                    gameLevel: progressiveInformation.Level,
                    isDisplayable: true,
                    tag: progressiveLevel,
                    hitState: ProgressiveAwardHitState.PotentialHit
                );

                if(progressiveInformation.ConsolationPaySpecified)
                {
                    progressiveAward.UpdateConsolationAmountValue(progressiveInformation.ConsolationPay);

                    if(progressiveInformation.MultiplyConsolationPayByDenomSpecified &&
                       progressiveInformation.MultiplyConsolationPayByDenom)
                    {
                        progressiveAward.UpdateConsolationAmountValue(progressiveAward.ConsolationAmountValue * denomination);
                    }
                }

                //TODO: Need to determine how we deal with specified controller types, sub controller types, and controller levels.
                //TODO: Until we define the meta data that gets sent to the system they will remain in the paytable.

                // If it is a game controlled progressive, validate the hit with the GCP controller.
                if(progressiveInformation.ControllerType == ProgressiveControllerTypes.GCP)
                {
                    if(iProgressiveController == null)
                    {
                        throw new ArgumentNullException("iProgressiveController",
                            "GCP awards exist in win outcome, but no game side progressive controller is specified.");
                    }

                    iProgressiveController.ValidateProgressiveHit(progressiveAward);
                }

                featureAward.AddFeatureProgressiveAward(progressiveAward);
            }
        }

        #endregion
    }
}
