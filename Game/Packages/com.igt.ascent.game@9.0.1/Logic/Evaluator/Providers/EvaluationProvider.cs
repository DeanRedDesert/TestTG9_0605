//-----------------------------------------------------------------------
// <copyright file = "EvaluationProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Schemas;
    using Services;

    /// <summary>
    /// Provider which exposes the results of an evaluation.
    /// This provider may contains results from more than one play section, like base game, bonus and double up.
    /// </summary>
    public class EvaluationProvider
    {
        #region Private Members

        /// <summary>
        /// Custom wins specified by name.
        /// </summary>
        private readonly Dictionary<string, long> specificWins = new Dictionary<string, long>();

        /// <summary>
        /// Custom wins specified by category.
        /// </summary>
        private readonly Dictionary<EvaluationCategory, long> specificCategoryWins = new Dictionary<EvaluationCategory, long>();

        /// <summary>
        /// List of wins for evaluation provider.
        /// </summary>
        private WinCycleEntryList winEntryList = new WinCycleEntryList();

        #endregion

        #region Protected Members

        /// <summary>
        /// The cold start window of the base game.
        /// </summary>
        protected readonly CellPopulationOutcome BaseGameColdStartWindow;

        /// <summary>
        /// The symbol window of evaluation result.
        /// </summary>
        protected CellPopulationOutcome symbolWindow;

        /// <summary>
        /// The Pay table holding all the pay table sections in the game. 
        /// It must be a slot pay table. 
        /// </summary>
        protected Paytable payTable;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new EvaluationProvider instance.
        /// </summary>
        /// <summary>
        /// Create a new evaluation provider.
        /// </summary>
        /// <param name="payTable">Paytable which the evaluation results are associated with.</param>
        /// <param name="baseGameSectionName">
        /// Section which is used for the base game and contains valid cold start stops. If the section is not a slot
        /// section, then the cold start information will not be populated.
        /// </param>
        public EvaluationProvider(Paytable payTable, string baseGameSectionName)
        {
            if(payTable == null)
            {
                throw new ArgumentNullException("payTable");
            }

            this.payTable = payTable;
            var baseGameSection = payTable.GetPaytableSection(baseGameSectionName);

            if(baseGameSection as SlotPaytableSection != null)
            {
                var paytableSection = (SlotPaytableSection)baseGameSection;

                BaseGameColdStartWindow
                    = StripBasedPopulator.CreateCellPopulationOutcome(
                        paytableSection.StripList,
                        paytableSection.SymbolWindow,
                        paytableSection.SymbolWindow.PopulationEntry
                                       .Select(entry => (int)entry.startPosition)
                                       .ToList());
            }
        }

        /// <summary>
        /// Create a new evaluation provider.
        /// This version is used in case pay table is not available when creating this provider.
        /// </summary>
        public EvaluationProvider()
        {
        }

        #endregion

        #region Game Service

        /// <summary>
        /// List of wins for evaluation provider.
        /// </summary>
        [GameService]
        public virtual WinCycleEntryList WinEntryList
        {
            get
            {
                return winEntryList;
            }
            protected set
            {
                winEntryList = value;
            }
        }

        /// <summary>
        /// Gets or sets the utility stop infos.
        /// </summary>
        /// <value>
        /// The utility stop infos.
        /// </value>
        [GameService]
        public virtual ReadOnlyCollection<UtilityStopInfo> UtilityStopInfos
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the history stop infos.
        /// </summary>
        /// <value>
        /// The history stop infos.
        /// </value>
        [GameService]
        public virtual ReadOnlyCollection<HistoryStopInfo> HistoryStopInfos
        {
            get;
            protected set;
        }

        /// <summary>
        /// Reel stops for evaluation provider.
        /// </summary>
        [GameService]
        public virtual List<int> ReelStops
        {
            get
            {
                return ReelStopsByRow(0);
            }
        }

        /// <summary>
        /// Reel stops for cold power up
        /// </summary>
        [GameService]
        public virtual List<int> ColdReelStops
        {
            get
            {
                return BaseGameColdStartWindow != null
                           ? BaseGameColdStartWindow.GetStops(0)
                                                    .Select(stop => (int)stop)
                                                    .ToList()
                           : new List<int>();
            }
        }

        /// <summary>
        /// Total win for evaluation provider.
        /// </summary>
        [GameService]
        public virtual long TotalWin
        {
            get;
            set;
        }

        /// <summary>
        /// Get the total win of specific evaluation indicated by the name.
        /// </summary>
        /// <param name="name">Evaluation name.</param>
        /// <returns>Total win of specific evaluation. 0 if the win does not exist.</returns>
        [GameService]
        public virtual long SpecificWinByName(string name)
        {
            if(specificWins.ContainsKey(name))
            {
                return specificWins[name];
            }
            return 0;
        }

        /// <summary>
        /// Get the total win of all evaluations with specific category.
        /// </summary>
        /// <param name="category">The string value of <see cref="EvaluationCategory"/>.</param>
        /// <returns>
        /// Total win of all evaluations with specific category.
        /// If the category cannot be found 0 is returned.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="category"/> does not represent any values in the
        /// <see cref="EvaluationCategory"/> enum.
        /// </exception>
        [GameService]
        public virtual long SpecificWinByCategory(string category)
        {
            EvaluationCategory enumValue;

            try
            {
                enumValue = (EvaluationCategory)Enum.Parse(typeof(EvaluationCategory), category, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Invalid category specified. Categories must exist in EvaluationCategory enum.", "category", ex);
            }
            
            if(specificCategoryWins.ContainsKey(enumValue))
            {
                return specificCategoryWins[enumValue];
            }
            return 0;
        }

        /// <summary>
        /// Gets the reel stops for the symbol window of the specified section.
        /// </summary>
        /// <param name="sectionName">
        /// Section which defines the symbol window. Must be a slot paytable section.
        /// </param>
        /// <returns>The reel stops based on the symbol window for the specified section.</returns>
        [GameService]
        public virtual List<int> ReelStopsByWindow(string sectionName)
        {
            var section = payTable.GetPaytableSection<SlotPaytableSection>(sectionName);

            var targetWindow = symbolWindow ??
                               StripBasedPopulator
                                   .CreateCellPopulationOutcome(section.StripList,
                                                                section.SymbolWindow,
                                                                section.SymbolWindow
                                                                       .PopulationEntry.Select(
                                                                           entry => (int)entry.startPosition)
                                                                       .ToList());

            return targetWindow.GetStopsForWindow(section.SymbolWindow).Select(stop => (int)stop).ToList();
        }

        /// <summary>
        /// Get the reel stops for the specified row of the symbol window. If there is not a symbol window set, then
        /// the cold start stops will be returned.
        /// Ensure that the paytable contains the cold start stops for the row being displayed by the presentation.
        /// </summary>
        /// <param name="row">The row to get stops for.</param>
        /// <returns>The stops for the specified row.</returns>
        [GameService]
        public virtual List<int> ReelStopsByRow(int row)
        {
            if(symbolWindow != null)
            {
                return symbolWindow.GetStops(row).Select(stop => (int)stop).ToList();
            }

            if(BaseGameColdStartWindow != null)
            {
                return BaseGameColdStartWindow.GetStops(row).Select(stop => (int)stop).ToList();
            }

            return new List<int>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the pay table.
        /// </summary>
        /// <param name="givenPayTable">The pay table to be set to the provider.</param>
        public void SetPayTable(Paytable givenPayTable)
        {
            payTable = givenPayTable;
        }

        /// <summary>
        /// Set the specific evaluation's win with specified name.
        /// </summary>
        /// <param name="name">Name of the specific evaluation.</param>
        /// <param name="win">Win of the specific evaluation.</param>
        public void SetSpecificWin(string name, long win)
        {
            specificWins[name] = win;
        }

        /// <summary>
        /// Set the total win of all evaluations with specified category.
        /// </summary>
        /// <param name="category">The category of the evaluations.</param>
        /// <param name="win">Total win of all the evaluations with specified category.</param>
        public void SetSpecificWin(EvaluationCategory category, long win)
        {
            specificCategoryWins[category] = win;
        }

        /// <summary>
        /// Set the win cycle entry list, total win and symbol window based off the evaluation results.
        /// </summary>
        /// <param name="evaluationResults">Evaluation results.</param>
        /// <exception cref="ArgumentNullException">Thrown when evaluationResult is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the latest evaluation result is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the PayTableSectionName property of evaluationResult is null.
        /// PayTableSectionName is mandatory for setting win cycle entry list.
        /// </exception>
        public void SetEvaluationResults(EvaluationResults evaluationResults)
        {
            SetEvaluationResults(evaluationResults, false);
        }

        /// <summary>
        /// Set the win cycle entry list, wins and symbol window based off the evaluation results.
        /// </summary>
        /// <param name="evaluationResults">Evaluation results.</param>
        /// <param name="excludeLatestWinFromTotalWin">Indicates if the latest total win should added to the total win of evaluation provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when evaluationResult is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the latest evaluation result is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the PayTableSectionName property of evaluationResult is null.
        /// PayTableSectionName is mandatory for setting win cycle entry list.
        /// </exception>
        public void SetEvaluationResults(EvaluationResults evaluationResults, bool excludeLatestWinFromTotalWin)
        {
            if(evaluationResults == null)
            {
                throw new ArgumentNullException("evaluationResults", "Argument may not be null");
            }

            if(evaluationResults.LatestEvaluation == null)
            {
                throw new ArgumentException("The latest evaluation result is null.", "evaluationResults");
            }

            if(evaluationResults.LatestEvaluation.PayTableSectionName == null)
            {
                throw new ArgumentException("PayTableSectionName property of EvaluationResult object may not be null when added to evaluation provider.", "evaluationResults");
            }

            // Set symbol window.
            SetSymbolWindow(evaluationResults.LatestEvaluation.SymbolWindow);

            // Set win cycle entries.
            SetWinCycleEntryList(evaluationResults.LatestEvaluation.PayTableSectionName, evaluationResults.LatestEvaluation.WinOutcome);
            
            // Set total win.
            TotalWin = excludeLatestWinFromTotalWin
                           ? evaluationResults.TotalWin - evaluationResults.LatestEvaluation.TotalWin
                           : evaluationResults.TotalWin;

            // Set wins for category.
            foreach(EvaluationCategory categoryValue in Enum.GetValues(typeof(EvaluationCategory)))
            {
                SetSpecificWin(categoryValue, evaluationResults.GetTotalWinForCategory(categoryValue));
            }

            // Set wins by name.
            foreach(var result in evaluationResults.AllEvaluations)
            {
                if(!string.IsNullOrEmpty(result.Name))
                {
                    SetSpecificWin(result.Name, result.TotalWin);
                }
            }

        }

        /// <summary>
        /// Set the symbol window for evaluation provider.
        /// </summary>
        /// <param name="givenSymbolWindow">The symbol window.</param>
        public void SetSymbolWindow(CellPopulationOutcome givenSymbolWindow)
        {
            symbolWindow = givenSymbolWindow;
        }

        /// <summary>
        /// Clear the symbol window.
        /// </summary>
        public void ClearSymbolWindow()
        {
            symbolWindow = null;
        }


        /// <summary>
        /// Sets the utility stop info.
        /// </summary>
        /// <param name="sectionName">
        /// Section which defines the symbol window. Must be a slot paytable section.
        /// </param>
        /// <remarks>
        /// The SetSymbolWindow must be called to set symbol window
        /// before you call this method in order to set utility stop info correctly.
        /// </remarks>
        public void SetUtilityStopInfo(string sectionName)
        {
            UtilityStopInfos = null;
            if(symbolWindow == null)
            {
                return;
            }

            var paytableSection = payTable.GetPaytableSection<SlotPaytableSection>(sectionName);

            var physicalStops = symbolWindow.GetStopsForWindow(paytableSection.SymbolWindow)
                                            .Select(stop => (int)stop)
                                            .ToArray();

            var entries = paytableSection.SymbolWindow.PopulationEntry;

            var utilityStopInfoList = new List<UtilityStopInfo>();

            for(var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var strip = paytableSection.StripList.Strip.First(x => x.name == entry.stripName);

                var physicalStop = physicalStops[i];

                UtilityStopInfo utilityStopInfo;

                //If phsicalStop is 0, then don't need to calculate the from, from should be 1.
                if(physicalStop == 0)
                {
                    utilityStopInfo = new UtilityStopInfo(physicalStop + 1,
                                                          new VirtualRange(1, strip.Stop[physicalStop].weight));
                }
                else
                {
                    int floor = strip.GetWeightForStopIndex(physicalStop);//The GetWeightForStopIndex returns sum of weights while stop is less than physicalStop
                    int ceiling = strip.Stop[physicalStop].weight + floor - 1;

                    utilityStopInfo = new UtilityStopInfo(physicalStop + 1, new VirtualRange(floor + 1, ceiling + 1));
                }

                utilityStopInfoList.Add(utilityStopInfo);
            }

            UtilityStopInfos = new ReadOnlyCollection<UtilityStopInfo>(utilityStopInfoList);
        }

        /// <summary>
        /// Sets the history stop info.
        /// </summary>
        /// <param name="sectionName">
        /// Section which defines the symbol window. Must be a slot paytable section.
        /// </param>
        /// <param name="virtualStops">The virtual stops.</param>
        /// <remarks>
        /// The SetSymbolWindow must be called to set symbol window
        /// before you call this method in order to set history stop info correctly.
        /// </remarks>
        public void SetHistoryStopInfo(string sectionName, IEnumerable<int> virtualStops)
        {
            #if !WEB_MOBILE
                HistoryStopInfos = null;
                if(symbolWindow == null)
                {
                    return;
                }

                var paytableSection = payTable.GetPaytableSection<SlotPaytableSection>(sectionName);

                var physicalStops = symbolWindow.GetStopsForWindow(paytableSection.SymbolWindow)
                                                .Select(stop => (int)stop)
                                                .ToArray();

                var list = new List<HistoryStopInfo>();
                var virtualStopArray = virtualStops.ToArray();
                var entries = paytableSection.SymbolWindow.PopulationEntry;

                for(int i = 0; i < entries.Count; i++)
                {
                    int physicalStop = physicalStops[i];
                    int virtualStop = virtualStopArray[i];

                    list.Add(new HistoryStopInfo(++physicalStop, ++virtualStop));
                }

                HistoryStopInfos = new ReadOnlyCollection<HistoryStopInfo>(list);
            #endif
        }

        /// <summary>
        /// Set the win cycle entry list based off of the payTableSection and win outcome.
        /// </summary>
        /// <param name="payTableSectionName">the pay table section name which the win entry list belongs to</param>
        /// <param name="winOutcome">The win outcome to use for updating the win entry list.</param>
        public void SetWinCycleEntryList(string payTableSectionName, WinOutcome winOutcome)
        {
            SetWinCycleEntryList(payTableSectionName, winOutcome, true, 0);
        }

        /// <summary>
        /// Set the win cycle entry list based off of the payTableSection and win outcome.
        /// </summary>
        /// <param name="payTableSectionName">the pay table section name which the win entry list belongs to</param>
        /// <param name="winOutcome">The win outcome to use for updating the win entry list.</param>
        /// <param name="groupByWinAmount">Indicates if the payline wins should be grouped by win amount.</param>
        public void SetWinCycleEntryList(string payTableSectionName, WinOutcome winOutcome, bool groupByWinAmount)
        {
            SetWinCycleEntryList(payTableSectionName, winOutcome, groupByWinAmount, 0);
        }

        /// <summary>
        /// Set the win cycle entry list based off of the payTableSection and win outcome.
        /// </summary>
        /// <param name="payTableSectionName">the pay table section name which the win entry list belongs to</param>
        /// <param name="winOutcome">The win outcome to use for updating the win entry list.</param>
        /// <param name="groupByWinAmount">Indicates if the payline wins should be grouped by win amount.</param>
        /// <param name="count">The maximum number of win cycle entries that can be grouped.</param>
        /// <exception cref="NullReferenceException">Thrown when slot pay table is null, which should be set before this function being called.</exception>
        /// <exception cref="ArgumentException">Thrown when failing to get slot pay table section whose name is payTableSectionName.</exception>
        /// <exception cref="NotSupportedException">Thrown when the pay table section is neither SlotPaytableSection nor PickPaytableSection.</exception>
        public void SetWinCycleEntryList(string payTableSectionName, WinOutcome winOutcome,
            bool groupByWinAmount, int count)
        {
            if(winOutcome == null)
            {
                // If passed-in win result is null, just clear win cycle list.
                WinEntryList.WinEntryList.Clear();
                return;
            }

            // Get the paytable section
            if(payTable == null)
            {
                throw new NullReferenceException("Slot pay table is null. Pay table must be set before calling AddWinCycleEntryList.");
            }

            PaytableSection paytableSection;

            try
            {
                paytableSection = payTable.GetPaytableSection(payTableSectionName);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Can not find slot paytable section named " + payTableSectionName, ex);
            }

            WinCycleEntryList winCycleEntryList;

            switch(paytableSection.GetType().Name)
            {
                case "SlotPaytableSection":
                    {
                        // Convert the win out come to win cycle entry list
                        winCycleEntryList = groupByWinAmount ?
                            MapSlotOutcome2EntryListGroupByWinAmount(paytableSection as SlotPaytableSection, winOutcome, count):
                            MapSlotOutcome2EntryList(paytableSection as SlotPaytableSection, winOutcome);
                        break;
                    }
                case "PickPaytableSection":
                    {
                        // for pick type of win outcome, the map need no pay table section
                        winCycleEntryList = MapPickOutcome2EntryList(winOutcome);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException(string.Format("Pay table section of {0} is not supported. Only SlotPaytableSection and PickPaytableSection are supported right now",
                                                                    paytableSection.GetType().Name));
                    }

            }

            winCycleEntryList.WinEntryList.Sort();
            WinEntryList = winCycleEntryList;
        }

        /// <summary>
        /// Set the win cycle entry list as unknown win type based off specified win outcome.
        /// </summary>
        /// <param name="winOutcome">The specified win outcome.</param>
        public void SetWinCycleEntryList(WinOutcome winOutcome)
        {
            if(winOutcome == null)
            {
                // If passed-in win result is null, just clear win cycle list.
                WinEntryList.WinEntryList.Clear();
                return;
            }

            WinEntryList = MapOutcome2EntryList(WinCycleEntry.WinEntryType.Unknown, winOutcome);
            WinEntryList.WinEntryList.Sort();
        }

        /// <summary>
        /// clear the win cycle entry list.
        /// </summary>
        public void ClearWinCycleEntryList()
        {
            WinEntryList.WinEntryList.Clear();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Convert from win outcome of a slot pay table section to win cycle entry list.
        /// </summary>
        /// <param name="paytableSection">the pay table section which win outcome associates to.</param>
        /// <param name="winOutcome">The win outcome to be converted.</param>
        /// <returns>The conversion result: win cycle entry list.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when win outcome item's pattern can't be found in pay table section.
        /// </exception>
        protected virtual WinCycleEntryList MapSlotOutcome2EntryList(SlotPaytableSection paytableSection, WinOutcome winOutcome)
        {
            var winCycleEntryList = new WinCycleEntryList();

            var winOutcomeItems = winOutcome.WinOutcomeItems;
            foreach(var win in winOutcomeItems)
            {
                if(MapFoundationOutcome2WinCycleEntry(winCycleEntryList, win))
                {
                    continue;
                }

                if(win.Pattern == null)
                {
                    continue;
                }

                // Calculate the entry type.
                var entryType = WinCycleEntry.WinEntryType.Unknown;
                var patternIndexes = new List<int>();
                if(paytableSection.LinePatternList != null && win.Pattern.patternListName == paytableSection.LinePatternList.name)
                {
                    entryType = WinCycleEntry.WinEntryType.LineWin;

                    bool patternFound = false;
                    int patternIndex;
                    for(patternIndex = 0; patternIndex < paytableSection.LinePatternList.Pattern.Count; patternIndex++)
                    {
                        if(paytableSection.LinePatternList.Pattern[patternIndex].name == win.Pattern.name)
                        {
                            patternFound = true;
                            break;
                        }
                    }
                    if(!patternFound)
                    {
                        throw new KeyNotFoundException(string.Format("Pattern: {0} not found in pattern list: {1}.",
                                                                    win.Pattern.name,
                                                                    paytableSection.LinePatternList.name));
                    }

                    // Patterns are indexed 1 based.
                    patternIndex++;
                    patternIndexes.Add(patternIndex);
                }
                else if(paytableSection.ScatterPatternList != null && win.Pattern.patternListName == paytableSection.ScatterPatternList.name)
                {
                    entryType = WinCycleEntry.WinEntryType.Scatter;
                    patternIndexes.Add(0);
                }
                else if(paytableSection.MultiwayPatternList != null && win.Pattern.patternListName == paytableSection.MultiwayPatternList.name)
                {
                    entryType = WinCycleEntry.WinEntryType.Multiway;
                    patternIndexes.Add(0);
                }

                var entry = new WinCycleEntry(entryType, patternIndexes, win.Prize.winAmount, win.displayReason,
                    win.Prize.ProgressiveLevels, win.Prize.ProgressivePrizeStrings)
                    {
                        WinOutcomeItemName = win.name,
                        BonusTriggered = win.Prize.Trigger.Any(),
                        BonusNames = GetBonusTriggerNames(win)
                    };
                entry.Symbols.AddRange(win.Pattern.Cluster.Cells);

                winCycleEntryList.WinEntryList.Add(entry);
            }
            return winCycleEntryList;
        }

        /// <summary>
        /// Convert from win outcome of a slot pay table section to win cycle entry list.
        /// The win cycle entries are grouped by the win entry type and the winning amount, and
        /// the number of entries grouped are limited by the count parameter. Grouping only applies
        /// to line wins.
        /// </summary>
        /// <remarks>Foundation wins are not grouped.</remarks>
        /// <param name="paytableSection">the pay table section which win outcome associates to.</param>
        /// <param name="winOutcome">The win outcome to be converted.</param>
        /// <param name="count">The maximum number of win cycle entries that can be grouped.</param>
        /// <returns>The conversion result: win cycle entry list.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when win outcome item's pattern can't be found in pay table section.
        /// </exception>
        protected virtual WinCycleEntryList MapSlotOutcome2EntryListGroupByWinAmount(SlotPaytableSection paytableSection,
            WinOutcome winOutcome, int count)
        {
            // Only line wins are grouped.
            if(paytableSection.LinePatternList != null)
            {
                var winCycleEntryList = new WinCycleEntryList();

                // Group by win entry type and winning amount.
                var groupedWinOutcomeItems = from win in winOutcome.WinOutcomeItems
                                             where win.Pattern != null && win.Pattern.patternListName == paytableSection.LinePatternList.name
                                             && win.Prize.ProgressiveLevels.Count == 0
                                             group win by paytableSection.LinePatternList.name
                                             into patternGroup
                                             from groupWin in patternGroup
                                             group groupWin by groupWin.Prize.winAmount;

                // Split the number of entries that are grouped if necessary.
                var groupedResult = new List<List<WinOutcomeItem>>();
                foreach(var groupWin in groupedWinOutcomeItems.ToList())
                {
                    var maxNumberOfItemsInGroup = count;
                    if(maxNumberOfItemsInGroup == 0)
                    {
                        maxNumberOfItemsInGroup = groupWin.Count();
                    }

                    var itemCount = 0;
                    while(groupWin.Skip(itemCount).Any())
                    {
                        groupedResult.Add(groupWin.Skip(itemCount).Take(maxNumberOfItemsInGroup).ToList());
                        itemCount += maxNumberOfItemsInGroup;
                    }
                }

                // Create line win win cycle entries.
                foreach(var winGroup in groupedResult)
                {
                    var patternIndexes = new List<int>();
                    var cells = new List<Cell>();
                    long winAmount = 0;
                    var displayReason = string.Empty;
                    bool bonusTriggered = false;
                    var bonusNames = new List<string>();

                    foreach(var win in winGroup)
                    {
                        var patternIndex = 0;

                        bool patternFound = false;
                        for(; patternIndex < paytableSection.LinePatternList.Pattern.Count; ++patternIndex)
                        {
                            if(paytableSection.LinePatternList.Pattern[patternIndex].name == win.Pattern.name)
                            {
                                patternFound = true;
                                break;
                            }
                        }
                        if(!patternFound)
                        {
                            throw new KeyNotFoundException(
                                string.Format("Pattern: {0} not found in pattern list: {1}.",
                                                win.Pattern.name,
                                                paytableSection.LinePatternList.name));
                        }

                        // Line patterns are indexed by 1 base.
                        patternIndex++;
                        patternIndexes.Add(patternIndex);

                        cells = cells.Union(win.Pattern.Cluster.Cells).ToList();
                        winAmount = win.Prize.winAmount;

                        // Use the same display reason for the grouped wins.
                        displayReason = win.displayReason;
                        bonusTriggered = win.Prize.Trigger.Any() || bonusTriggered;
                        bonusNames.AddRange(GetBonusTriggerNames(win));
                    }
                    // Grouped wins cannot contain progressive prizes so the prize string data isn't needed here.
                    var entry = new WinCycleEntry(WinCycleEntry.WinEntryType.LineWin, patternIndexes, winAmount, displayReason);
                    entry.Symbols.AddRange(cells);
                    entry.BonusTriggered = bonusTriggered;
                    entry.BonusNames = bonusNames;
                    winCycleEntryList.WinEntryList.Add(entry);
                }

                // Create win entries for other win types.
                var wins = winOutcome.WinOutcomeItems.Where(win => win.Pattern!= null &&
                    (win.Pattern.patternListName != paytableSection.LinePatternList.name || win.Prize.ProgressiveLevels.Count > 0));
                foreach(var win in wins)
                {
                    var entryType = WinCycleEntry.WinEntryType.Unknown;
                    var entryTypeIndex = 0;

                    if(paytableSection.ScatterPatternList != null &&
                       win.Pattern.patternListName == paytableSection.ScatterPatternList.name)
                    {
                        entryType = WinCycleEntry.WinEntryType.Scatter;
                    }
                    else if(paytableSection.MultiwayPatternList != null &&
                        win.Pattern.patternListName == paytableSection.MultiwayPatternList.name)
                    {
                        entryType = WinCycleEntry.WinEntryType.Multiway;
                    }
                    else if(paytableSection.LinePatternList != null &&
                            win.Pattern.patternListName == paytableSection.LinePatternList.name)
                    {
                        entryType = WinCycleEntry.WinEntryType.LineWin;
                        var patternFound = false;
                        int patternIndex;
                        for(patternIndex = 0; patternIndex < paytableSection.LinePatternList.Pattern.Count; patternIndex++)
                        {
                            if(paytableSection.LinePatternList.Pattern[patternIndex].name == win.Pattern.name)
                            {
                                patternFound = true;
                                break;
                            }
                        }
                        if(!patternFound)
                        {
                            throw new KeyNotFoundException(string.Format("Pattern: {0} not found in pattern list: {1}.",
                                                                        win.Pattern.name,
                                                                        paytableSection.LinePatternList.name));
                        }

                        // Patterns are indexed 1 based.
                        entryTypeIndex = patternIndex + 1;
                    }
                    var entry = new WinCycleEntry(entryType, new List<int> { entryTypeIndex }, win.Prize.winAmount, win.displayReason,
                                                  win.Prize.ProgressiveLevels, win.Prize.ProgressivePrizeStrings);
                    entry.Symbols.AddRange(win.Pattern.Cluster.Cells);
                    entry.WinOutcomeItemName = win.name;
                    entry.BonusTriggered = win.Prize.Trigger.Any();
                    entry.BonusNames = GetBonusTriggerNames(win);

                    winCycleEntryList.WinEntryList.Add(entry);
                }

                // Check for foundation wins in the win outcome item list that do not trigger any pattern defined in the paytable.
                wins = winOutcome.WinOutcomeItems.Where(win => win.Pattern == null);
                foreach(var win in wins)
                {
                    MapFoundationOutcome2WinCycleEntry(winCycleEntryList, win);
                }

                return winCycleEntryList;
            }
            return MapSlotOutcome2EntryList(paytableSection, winOutcome);
        }

        /// <summary>
        /// Convert from win outcome of Pick type to win cycle entry list.
        /// </summary>
        /// <param name="winOutcome">The win outcome to be converted.</param>
        /// <returns>The conversion result: win cycle entry list.</returns>
        protected virtual WinCycleEntryList MapPickOutcome2EntryList(WinOutcome winOutcome)
        {
            var winCycleEntryList = new WinCycleEntryList();

            var winOutcomeItems = winOutcome.WinOutcomeItems;
            foreach(var win in winOutcomeItems)
            {
                var entry = new WinCycleEntry(WinCycleEntry.WinEntryType.Scatter, new List<int> {0}, win.Prize.winAmount,
                    win.displayReason)
                    {
                        WinOutcomeItemName = win.name,
                        BonusTriggered = win.Prize.Trigger.Any(),
                        BonusNames = GetBonusTriggerNames(win)
                    };
                winCycleEntryList.WinEntryList.Add(entry);
            }

            return winCycleEntryList;
        }

        /// <summary>
        /// Convert from win outcome to win cycle entry list.
        /// </summary>
        /// <param name="winOutcome">The win outcome to be converted.</param>
        /// <param name="winType">The win type.</param>
        /// <returns>The converted win cycle entry list.</returns>
        protected virtual WinCycleEntryList MapOutcome2EntryList(WinCycleEntry.WinEntryType winType, WinOutcome winOutcome)
        {
            var winCycleEntryList = new WinCycleEntryList();

            var winOutcomeItems = winOutcome.WinOutcomeItems;
            foreach(var win in winOutcomeItems)
            {
                var entry = new WinCycleEntry(winType, new List<int> {0}, win.Prize.winAmount, win.displayReason)
                {
                    WinOutcomeItemName = win.name,
                    BonusTriggered = win.Prize.Trigger.Any(),
                    BonusNames = GetBonusTriggerNames(win)
                };
                winCycleEntryList.WinEntryList.Add(entry);
            }

            return winCycleEntryList;
        }

        /// <summary>
        /// Converts from a win outcome item containing a foundation win to a win cycle entry.
        /// </summary>
        /// <remarks>The foundation wins are not associated with any particular pattern.</remarks>
        /// <param name="winCycleEntryList">The win cycle entry list where the win cycle entry is to be stored.</param>
        /// <param name="winOutcomeItem">A win outcome item in the win outcome list.</param>
        /// <returns>True if the win outcome item contains a foundation win, and false otherwise.</returns>
        protected virtual bool MapFoundationOutcome2WinCycleEntry(WinCycleEntryList winCycleEntryList,
            WinOutcomeItem winOutcomeItem)
        {
            const string foundationOutcomeName = "foundation";

            if(winOutcomeItem.Pattern == null && winOutcomeItem.name.Length != 0 &&
               winOutcomeItem.name.ToLower().Contains(foundationOutcomeName))
            {
                var entry = new WinCycleEntry(WinCycleEntry.WinEntryType.Foundation, new List<int>(),
                                              winOutcomeItem.Prize.winAmount, winOutcomeItem.displayReason)
                {
                    WinOutcomeItemName = winOutcomeItem.name,
                    BonusNames = GetBonusTriggerNames(winOutcomeItem)
                };
                winCycleEntryList.WinEntryList.Add(entry);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve the bonus trigger names that are involved in the given win.
        /// </summary>
        /// <param name="win">The win the bonus trigger names should be retrieved from.</param>
        /// <returns>List of bonus trigger names involved in the given win.</returns>
        protected virtual List<string> GetBonusTriggerNames(WinOutcomeItem win)
        {
            return win.Prize.Trigger.Select(t => t.name).ToList();
        }
        #endregion
    }
}
