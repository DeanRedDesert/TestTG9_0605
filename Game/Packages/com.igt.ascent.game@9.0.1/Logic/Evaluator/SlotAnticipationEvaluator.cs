//-----------------------------------------------------------------------
// <copyright file = "SlotAnticipationEvaluator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Schemas;

    //These using declarations are conditionally compiled to prevent warnings from unused declarations.
    #if !WEB_MOBILE
        using System.Runtime.Serialization;
        using System.Runtime.Serialization.Formatters.Binary;
        using System.Xml;
    #else
        using System.Xml.Serialization;
    #endif

    /// <summary>
    /// Class which contains methods for evaluating slot patterns that are
    /// used for win anticipation.
    /// </summary>
    public static class SlotAnticipationEvaluator
    {
        /// <summary>
        /// Enumeration of the different possible Prize Scale Sections
        /// in a paytable.
        /// </summary>
        private enum PrizeScaleSection
        {
            /// <summary>
            /// Line Prize Scale Section.
            /// </summary>
            LinePrizeScale,

            /// <summary>
            /// Scatter Prize Scale Section.
            /// </summary>
            ScatterPrizeScale,

            /// <summary>
            /// Multiway Prize Scale Section.
            /// </summary>
            MultiwayPrizeScale,
        }

        /// <summary>
        /// Locates the first prize that matches a passed in prize name in a
        /// specific Prize Scale Section.
        /// </summary>
        /// <param name="slotPaytableSection">
        /// Slot Paytable Section to search for a prize.
        /// </param>
        /// <param name="prizeScaleSection">
        /// Prize Scale Section in the Slot Paytable Section to search for a prize.
        /// </param>
        /// <param name="prizeName">Prize name to search for.</param>
        /// <returns>SlotPrize if found, if the SlotPrize is not found returns null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if slotPaytableSection is null.</exception>
        /// <exception cref="ArgumentException">Thrown if prizeName is empty or null.</exception>
        private static SlotPrize FindSlotPrize(SlotPaytableSection slotPaytableSection, PrizeScaleSection prizeScaleSection, string prizeName)
        {
            if (slotPaytableSection == null)
            {
                throw new ArgumentNullException("slotPaytableSection");
            }

            if (string.IsNullOrEmpty(prizeName))
            {
                throw new ArgumentException("prizeName cannot be a null or empty string.", "prizeName");
            }

            SlotPrizeScale slotPrizeScale = null;
            switch (prizeScaleSection)
            {
                case PrizeScaleSection.LinePrizeScale:
                    slotPrizeScale = slotPaytableSection.LinePrizeScale;
                    break;

                case PrizeScaleSection.ScatterPrizeScale:
                    slotPrizeScale = slotPaytableSection.ScatterPrizeScale;
                    break;

                case PrizeScaleSection.MultiwayPrizeScale:
                    slotPrizeScale = slotPaytableSection.MultiwayPrizeScale;
                    break;
            }

            return slotPrizeScale != null ? slotPrizeScale.Prize.FirstOrDefault(prize => prize.name == prizeName) : null;
        }

        /// <summary>
        /// Creates a copy of the Slot Prize that is passed in and adds the
        /// copy to the passed in Slot Prize Scale. The copied Slot Prize
        /// contains Prize Pays with counts from 1 to the maximum Prize Pay
        /// Count in the original Slot Prize.
        /// </summary>
        /// <param name="slotPrize">
        /// Slot Prize to copy and add to the Slot Prize Scale.
        /// </param>
        /// <param name="betAmountRequired">
        /// Bet Amount Required field to use in created Prize Entries.
        /// </param>
        /// <param name="requiredActivePattern">
        /// Required Active Pattern field to use in created Prize Entries.
        /// </param>
        /// <param name="slotPrizeScale">
        /// Slot Prize Scale to add a Slot Prize Entry.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if slotPrize is null.</exception>
        /// <exception cref="ArgumentException">Thrown if betAmountRequired is empty or null.</exception>
        /// <exception cref="ArgumentException">Thrown if betAmountRequired is empty or null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if slotPrizeScale is null.</exception>
        private static void AddSlotPrizeToSlotPrizeScale(SlotPrize slotPrize,
                                                         string betAmountRequired,
                                                         string requiredActivePattern,
                                                         SlotPrizeScale slotPrizeScale)
        {
            if (slotPrize == null)
            {
                throw new ArgumentNullException("slotPrize");
            }

            if (string.IsNullOrEmpty(betAmountRequired))
            {
                throw new ArgumentException("betAmountRequired cannot be a null or empty string.", "betAmountRequired");
            }

            if (string.IsNullOrEmpty(requiredActivePattern))
            {
                throw new ArgumentException("requiredActivePattern cannot be a null or empty string.", "requiredActivePattern");
            }

            if (slotPrizeScale == null)
            {
                throw new ArgumentNullException("slotPrizeScale");
            }

            var modifiedSlotPrize = slotPrizeScale.Prize.FirstOrDefault(prize => prize.name == slotPrize.name);

            if(modifiedSlotPrize == null)
            {
                //Make a copy of the passed in Slot Prize.
                modifiedSlotPrize = BinarySerialCopy(slotPrize);
                modifiedSlotPrize.PrizePay = new List<PrizePay>();
                slotPrizeScale.Prize.Add(modifiedSlotPrize);
            }

            //Find the highest Win Count in the original paytable
            var highestcount = 1;
            foreach (var prizePay in slotPrize.PrizePay.Where(prizePay => prizePay.count > highestcount))
            {
                highestcount = prizePay.count;
            }

            //Reset the required count down to zero so there will be anticipation on near misses
            foreach (var prizeSymbol in modifiedSlotPrize.PrizeSymbol)
            {
                prizeSymbol.requiredCount = 0;
                prizeSymbol.requiredCountSpecified = true;
            }

            for (var prizePayCount = 1; prizePayCount <= highestcount; prizePayCount++)
            {
                var prizePayEntry = modifiedSlotPrize.PrizePay.FirstOrDefault(prizePay => prizePay.count == prizePayCount);
                if(prizePayEntry == null)
                {
                    prizePayEntry = new PrizePay { count = prizePayCount };
                    modifiedSlotPrize.PrizePay.Add(prizePayEntry);
                }
                prizePayEntry.WinAmount.Add(new WinAmount
                                            {
                                                value = 0,
                                                requiredTotalBet = "ALL",
                                                RequiredPatterns = new List<RequiredPattern>
                                                                   {
                                                                        new RequiredPattern
                                                                        {
                                                                            BetAmountRequired = betAmountRequired,
                                                                            RequiredActivePattern = requiredActivePattern
                                                                        }
                                                                   }
                                            });
            }
        }

        /// <summary>
        /// Finds Slot Prizes in the passed in Slot Paytable Section that correspond
        /// to the passed in Slot Anticipation Patterns, and then adds them to
        /// the classes internal lists of Slot Prizes.
        /// </summary>
        /// <param name="slotPaytableSection">Slot Paytable Section to search for prizes.</param>
        /// <param name="prizeScaleSection"> Prize Scale Section in the Slot Paytable Section to search for a prize.</param>
        /// <param name="slotAnticipationPatterns">Win Anticipation Patterns.</param>
        /// <param name="slotPrizeScale"> Slot Prize Scale to add a Slot Prize Entry.</param>
        /// <exception cref="ArgumentNullException">Thrown if slotPaytableSection is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if slotPrizeScale is null.</exception>
        private static void AddSlotPrizes(SlotPaytableSection slotPaytableSection,
                                          PrizeScaleSection prizeScaleSection,
                                          IEnumerable<SlotAnticipationPrize> slotAnticipationPatterns,
                                          SlotPrizeScale slotPrizeScale)
        {
            if (slotPaytableSection == null)
            {
                throw new ArgumentNullException("slotPaytableSection");
            }

            if (slotPrizeScale == null)
            {
                throw new ArgumentNullException("slotPrizeScale");
            }

            if (slotAnticipationPatterns != null)
            {
                foreach (var winAnticipationPattern in slotAnticipationPatterns)
                {
                    //Add Slot Line Prize Pattern if it exists.
                    var slotPrize = FindSlotPrize(slotPaytableSection, prizeScaleSection,
                                                  winAnticipationPattern.PrizeName);
                    if (slotPrize != null)
                    {
                        AddSlotPrizeToSlotPrizeScale(slotPrize,
                                                     winAnticipationPattern.BetAmountRequired,
                                                     winAnticipationPattern.RequiredActivePattern,
                                                     slotPrizeScale);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates Win Anticipation Patterns that correspond to Prizes in the
        /// passed in Slot Paytable Section. The corresponding Prizes are copied and a new
        /// Prize Pay section is created containing Prize Pay Entries with counts from 1 to
        /// the maximum Prize Pay Count in the original Prize. These new copied prizes are then
        /// evaluated using the normal game evaluation class SlotEvaluator. This may not cover
        /// all combinations of PayStrategy and reel stop order.
        /// </summary>
        /// <param name="slotPaytableSection">Slot Paytable Section to locate prizes.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <param name="betDefinitions">Bet to use for the evaluation.</param>
        /// <param name="winAnticipationPatterns">Win Anticipation Patterns to evaluate.</param>
        /// <param name="denomination">The denomination for bets passed in.</param>
        /// <param name="anticipationCache">
        /// The cache to use for storing the temporary data used for anticipation evaluations.
        /// This allows for faster evaluations of the same anticipation patterns on the same
        /// paytable.
        /// </param>
        /// <returns>A EvaluationResult containing the results of the evaluation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if slotPaytableSection is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when passed denomination is less than or equal to 0.</exception>
        public static EvaluationResult EvaluateWinAnticipation(SlotPaytableSection slotPaytableSection,
                                                                   IRandomNumbers randomNumbers,
                                                                   BetDefinitionList betDefinitions,
                                                                   List<SlotAnticipationPrize> winAnticipationPatterns,
                                                                   long denomination,
                                                                   SlotAnticipationCache anticipationCache)
        {
            if (slotPaytableSection == null)
            {
                throw new ArgumentNullException("slotPaytableSection", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            var populatedWindow = StripBasedPopulator.CreateCellPopulationOutcome(slotPaytableSection.StripList,
                                                                                  slotPaytableSection.SymbolWindow,
                                                                                  randomNumbers);

            return EvaluateWinAnticipation(slotPaytableSection, populatedWindow, betDefinitions,
                                            winAnticipationPatterns, denomination, anticipationCache);
        }

        /// <summary>
        /// Evaluates Win Anticipation Patterns that correspond to Prizes in the
        /// passed in Slot Paytable Section. The corresponding Prizes are copied and a new
        /// Prize Pay section is created containing Prize Pay Entries with counts from 1 to
        /// the maximum Prize Pay Count in the original Prize. These new copied prizes are then
        /// evaluated using the normal game evaluation class SlotEvaluator. This may not cover
        /// all combinations of PayStrategy and reel stop order.
        /// </summary>
        /// <param name="slotPaytableSection">Slot Paytable Section to locate prizes.</param>
        /// <param name="populatedWindow">Populated symbol window.</param>
        /// <param name="betDefinitions">Bet to use for the evaluation.</param>
        /// <param name="winAnticipationPatterns">Win Anticipation Patterns to evaluate.</param>
        /// <param name="denomination">The denomination for bets passed in.</param>
        /// <param name="anticipationCache">
        /// The cache to use for storing the temporary data used for anticipation evaluations.
        /// This allows for faster evaluations of the same anticipation patterns on the same
        /// paytable.
        /// </param>
        /// <returns>A EvaluationResult containing the results of the evaluation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if slotPaytableSection is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when passed populatedWindow is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when passed denomination is less than or equal to 0.</exception>
        public static EvaluationResult EvaluateWinAnticipation(SlotPaytableSection slotPaytableSection,
                                                            CellPopulationOutcome populatedWindow,
                                                            BetDefinitionList betDefinitions,
                                                            List<SlotAnticipationPrize> winAnticipationPatterns,
                                                            long denomination,
                                                            SlotAnticipationCache anticipationCache)
        {
            if (slotPaytableSection == null)
            {
                throw new ArgumentNullException("slotPaytableSection", "Argument may not be null");
            }

            if (populatedWindow == null)
            {
                throw new ArgumentNullException("populatedWindow", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            if(anticipationCache == null)
            {
                throw new ArgumentNullException("anticipationCache", "Argument may not be null");
            }

            var anticipationPaytableSection = anticipationCache.GetCachedPaytableSection(slotPaytableSection, winAnticipationPatterns);

            if(anticipationPaytableSection == null)
            {
                //Add Slot Line Prize Patterns if they exists.
                var slotLinePrizeScale = new SlotPrizeScale();
                AddSlotPrizes(slotPaytableSection, PrizeScaleSection.LinePrizeScale, winAnticipationPatterns, slotLinePrizeScale);

                //Add Slot Scatter Prize Patterns if they exists.
                var slotScatterPrizeScale = new SlotPrizeScale();
                AddSlotPrizes(slotPaytableSection, PrizeScaleSection.ScatterPrizeScale, winAnticipationPatterns, slotScatterPrizeScale);

                //Add Slot Multi-Way Prize Patterns if they exists.
                var slotMultiWayPrizeScale = new SlotPrizeScale();
                AddSlotPrizes(slotPaytableSection, PrizeScaleSection.MultiwayPrizeScale, winAnticipationPatterns, slotMultiWayPrizeScale);

                //Make a copy of the passed in Slot Paytable Section.
                anticipationPaytableSection = BinarySerialCopy(slotPaytableSection);
                anticipationPaytableSection.LinePrizeScale = slotLinePrizeScale;
                anticipationPaytableSection.ScatterPrizeScale = slotScatterPrizeScale;
                anticipationPaytableSection.MultiwayPrizeScale = slotMultiWayPrizeScale;

                anticipationCache.SaveToCache(slotPaytableSection, winAnticipationPatterns, anticipationPaytableSection);
            }

            return SlotEvaluator.EvaluatePaytableSection(anticipationPaytableSection,
                                                          populatedWindow,
                                                          betDefinitions,
                                                          null,
                                                          denomination);
        }

        /// <summary>
        /// Make a deep copy of a serializable object.
        /// </summary>
        /// <typeparam name="T">Type being copied.</typeparam>
        /// <param name="data">Data to copy.</param>
        /// <returns>Copy of the passed data. If null is passed in, then null will be returned.</returns>
        private static T BinarySerialCopy<T>(T data)
        {
            if (data == null)
            {
                return default;
            }

            using(var memoryStream = new MemoryStream())
            {
                #if !WEB_MOBILE
                    var binaryFormatter = new BinaryFormatter();
                    var selector = new SurrogateSelector();
                    selector.AddSurrogate(typeof(XmlElement), new StreamingContext(StreamingContextStates.All),
                        new XmlElementSerializationSurrogate());
                    binaryFormatter.SurrogateSelector = selector;

                    binaryFormatter.Serialize(memoryStream, data);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return (T)binaryFormatter.Deserialize(memoryStream);
                #else
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(memoryStream, data);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return (T)serializer.Deserialize(memoryStream);
                #endif
            }
        }
    }
}
