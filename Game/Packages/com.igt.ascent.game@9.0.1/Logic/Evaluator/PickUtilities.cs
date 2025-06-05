//-----------------------------------------------------------------------
// <copyright file = "PickUtilities.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication.Foundation;
    using Schemas;

    /// <summary>
    /// Class which contains utilities that are useful when working with pick games.
    /// </summary>
    public static class PickUtilities
    {
        /// <summary>
        /// Randomly draw a set of picks from the given list of picks.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">The replacement strategy used to select the picks.</param>
        /// <param name="availablePicks">A pool of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// /// <exception cref="ArgumentNullException">
        /// Thrown if either availablePicks or randomNumbers is null.
        /// Also thrown if pickWeights is null and replacement strategy is Exhaustive.
        /// </exception>
        /// /// <exception cref="PickCountException">
        /// Thrown if the requested number of picks are picked without replacement and 
        /// exceeds the available picks.
        /// </exception>
        /// <exception cref="PickWeightException">
        /// Thrown if any of the picks in <paramref name="availablePicks"/> has a weight of zero.
        /// </exception>
        /// <remarks>
        /// This method may mutate the pick weights in <paramref name="availablePicks"/>.
        /// </remarks>
        private static IList<T> GetPicks<T>(uint numberOfPicks,
            PickPaytableSectionReplacementStrategy replacement,
            IList<T> availablePicks,
            IRandomNumbers randomNumbers) where T : IPick
        {
            if(availablePicks == null)
            {
                throw new ArgumentNullException("availablePicks", "Argument may not be null");
            }

            if(randomNumbers == null)
            {
                throw new ArgumentNullException("randomNumbers", "Argument may not be null");
            }

            if(numberOfPicks > availablePicks.Count
               && replacement == PickPaytableSectionReplacementStrategy.WithoutReplacement)
            {
                throw new PickCountException(numberOfPicks, availablePicks.Count);
            }

            if(availablePicks.Any(pick => pick.Weight == 0))
            {
                throw new PickWeightException("Pick items cannot have a weight of 0.");
            }

            var pickedItems = new List<T>();

            checked
            {
                for(var currentPick = 0; currentPick < numberOfPicks; currentPick++)
                {
                    var pickedNumber =
                        randomNumbers.GetRandomNumbers(new RandomValueRequest(1, 0,
                            (int)availablePicks.GetTotalWeight() - 1));
                    var pick = availablePicks.GetPickByWeight((uint)pickedNumber.First());
                    pickedItems.Add(pick);

                    switch(replacement)
                    {
                        case PickPaytableSectionReplacementStrategy.Exhaustive:
                            // Reduce the weight from the dictionary when a pick is selected.
                            pick.Weight--;
                            if(pick.Weight <= 0)
                            {
                                //Removes a pick from available picks if the temp weight goes down to 0.
                                availablePicks.Remove(pick);
                            }
                            break;

                        case PickPaytableSectionReplacementStrategy.WithoutReplacement:
                            availablePicks.Remove(pick);
                            break;

                        case PickPaytableSectionReplacementStrategy.WithReplacement:
                            break;
                    }
                }
            }

            return pickedItems;
        }

        /// <summary>
        /// Randomly draw a set of picks from the given list of picks.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">The replacement strategy used to select the picks.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled to
        ///  prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="initialPicks">A pool of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either initialPicks is null.
        /// </exception>
        public static IList<Pick> GetPicks(uint numberOfPicks,
            PickPaytableSectionReplacementStrategy replacement,
            bool shuffle,
            IEnumerable<Pick> initialPicks,
            IRandomNumbers randomNumbers)
        {
            if(initialPicks == null)
            {
                throw new ArgumentNullException("initialPicks", "Argument may not be null");
            }

            var pickedItems = GetPicks(
                numberOfPicks,
                replacement,
                shuffle,
                initialPicks.Select(pick => new PickAdapter(pick)),
                randomNumbers);

            return pickedItems.Select(pick => pick.PickObject).ToList();
        }

        /// <summary>
        /// Randomly draw a set of picks from the given list of picks.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">The replacement strategy used to select the picks.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled to
        ///  prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="initialPicks">A pool of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either initialPicks is null.
        /// </exception>
        /// <remarks>
        /// This method may mutate the pick weights in <paramref name="initialPicks"/>.
        /// </remarks>
        public static IList<T> GetPicks<T>(uint numberOfPicks,
            PickPaytableSectionReplacementStrategy replacement,
            bool shuffle,
            IEnumerable<T> initialPicks,
            IRandomNumbers randomNumbers) where T : IPick
        {
            if(initialPicks == null)
            {
                throw new ArgumentNullException("initialPicks", "Argument may not be null");
            }

            //List of the available picks which will be drawn from.
            var availablePicks = new List<T>(initialPicks);

            var pickedItems = GetPicks(numberOfPicks, replacement, availablePicks, randomNumbers);

            //Shuffles for single item lists would result in an extra random number request.
            return shuffle && numberOfPicks > 1 ? ShufflePicks(pickedItems, randomNumbers) : pickedItems;
        }

        /// <summary>
        /// Randomly draw a set of picks from the specified pick paytable section.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled to
        ///  prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="pickPaytableSection">Source of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if pickPaytableSection is null.
        /// </exception>
        /// <exception cref="PaytableException">
        /// Thrown if the replacement strategy is not specifed in the paytable.
        /// </exception>
        public static IList<Pick> GetPicks(uint numberOfPicks,
            bool shuffle,
            PickPaytableSection pickPaytableSection,
            IRandomNumbers randomNumbers)
        {
            if(pickPaytableSection == null)
            {
                throw new ArgumentNullException("pickPaytableSection", "Argument may not be null");
            }

            if(!pickPaytableSection.ReplacementStrategySpecified)
            {
                throw new PaytableException("PickPaytableSection", "ReplacementStrategy not specified");
            }

            var pickedItems = GetPicks(
                numberOfPicks,
                pickPaytableSection.ReplacementStrategy,
                shuffle,
                pickPaytableSection.Pick.Select(pick => new PickAdapter(pick)),
                randomNumbers);

            // Return a deep copy list of the picked items because we don't expect the game to modify
            // the data from paytable accidently by adjusting the evaluation result.
            return pickedItems.Select(pick => new Pick(pick.PickObject)).ToList();
        }

        /// <summary>
        /// Randomly draw a set of picks from the specified pick paytable section.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="pickPaytableSectionReplacementStrategy">The replacement strategy used to select the picks.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled to
        ///  prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="pickPaytableSection">Source of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if pickPaytableSection is null.
        /// </exception>
        public static IList<Pick> GetPicks(uint numberOfPicks,
            PickPaytableSectionReplacementStrategy pickPaytableSectionReplacementStrategy,
            bool shuffle,
            PickPaytableSection pickPaytableSection,
            IRandomNumbers randomNumbers)
        {
            if(pickPaytableSection == null)
            {
                throw new ArgumentNullException("pickPaytableSection", "Argument may not be null");
            }

            var pickedItems = GetPicks(
                numberOfPicks,
                pickPaytableSectionReplacementStrategy,
                shuffle,
                pickPaytableSection.Pick.Select(pick => new PickAdapter(pick)),
                randomNumbers);

            // Return a deep copy list of the picked items because we don't expect the game to modify
            // the data from paytable accidently by adjusting the evaluation result.
            return pickedItems.Select(pick => new Pick(pick.PickObject)).ToList();
        }

        /// <summary>
        /// Randomly draw a set of picks from the given list of picks.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">Should the picks be drawn with replacement.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled 
        /// to prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="initialPicks">A pool of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        public static IList<Pick> GetPicks(uint numberOfPicks,
            bool replacement,
            bool shuffle,
            IEnumerable<Pick> initialPicks,
            IRandomNumbers randomNumbers)
        {
            var replacementStrategy = replacement
                ? PickPaytableSectionReplacementStrategy.WithReplacement
                : PickPaytableSectionReplacementStrategy.WithoutReplacement;

            var pickedItems = GetPicks(numberOfPicks,
                replacementStrategy,
                shuffle,
                initialPicks.Select(pick => new PickAdapter(pick)),
                randomNumbers);

            return pickedItems.Select(pick => pick.PickObject).ToList();
        }

        /// <summary>
        /// Randomly draw a set of picks from the given list of picks.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">Should the picks be drawn with replacement.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled 
        /// to prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="initialPicks">A pool of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        public static IList<T> GetPicks<T>(uint numberOfPicks,
            bool replacement,
            bool shuffle,
            IEnumerable<T> initialPicks,
            IRandomNumbers randomNumbers) where T : IPick
        {
            var replacementStrategy = replacement
                ? PickPaytableSectionReplacementStrategy.WithReplacement
                : PickPaytableSectionReplacementStrategy.WithoutReplacement;

            return GetPicks(numberOfPicks, replacementStrategy, shuffle, initialPicks, randomNumbers);
        }

        /// <summary>
        /// Randomly draw a set of picks from the specified pick paytable section.
        /// </summary>
        /// <param name="numberOfPicks">The number of picks to draw.</param>
        /// <param name="replacement">If true the picks can have duplicate entries. Otherwise if false.</param>
        /// <param name="shuffle">
        /// If true the picks in the list will be shuffled. If picks are weighted they should normally be shuffled 
        /// to prevent ordering bias caused by the weight of the picks.
        /// </param>
        /// <param name="pickPaytableSection">Source of picks to draw from.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <returns>A list of picks.</returns>
        /// /// <exception cref="ArgumentNullException">
        /// Thrown if pickPaytableSection is null.
        /// </exception>
        public static IList<Pick> GetPicks(uint numberOfPicks,
            bool replacement,
            bool shuffle,
            PickPaytableSection pickPaytableSection,
            IRandomNumbers randomNumbers)
        {
            if(pickPaytableSection == null)
            {
                throw new ArgumentNullException("pickPaytableSection", "Argument may not be null");
            }

            var pickedItems = GetPicks(numberOfPicks,
                replacement,
                shuffle,
                pickPaytableSection.Pick.Select(pick => new PickAdapter(pick)),
                randomNumbers);

            // Return a deep copy list of the picked items because we don't expect the game to modify
            // the data from paytable accidently by adjusting the evaluation result.
            return pickedItems.Select(pick => new Pick(pick.PickObject)).ToList();
        }

        /// <summary>
        /// Get a win outcome item for the specified pick.
        /// </summary>
        /// <param name="pick">The pick to get a win outcome item for.</param>
        /// <param name="denomination">The denomination for the pick win value passed in.</param>
        /// <returns>
        /// The win outcome item for the specified pick. If the pick does not have a win, then null will be returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when pick parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the passed denomination is less than or equal to 0.
        /// </exception>
        public static WinOutcomeItem GetWinForPick(Pick pick, long denomination)
        {
            if(pick == null)
            {
                throw new ArgumentNullException("pick", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            WinOutcomeItem winOutcomeItem = null;

            if(pick.Win != null)
            {
                winOutcomeItem = new WinOutcomeItem
                {
                    name = pick.name,
                    displayReason = pick.name,
                    Prize = new Prize()
                };

                if(pick.Win.valueSpecified)
                {
                    winOutcomeItem.Prize.winAmount = Utility.ConvertToCents(pick.Win.value, denomination);
                }

                winOutcomeItem.Prize.prizeName = pick.name;

                //Use AddRangeCopy to separate the triggers from the original triggers.
                winOutcomeItem.Prize.Trigger.AddRangeCopy(pick.Win.Trigger);
                winOutcomeItem.winLevelIndex = pick.Win.winLevelIndex;
                winOutcomeItem.Prize.ProgressiveLevels.AddRange(pick.Win.ProgressiveLevel);
            }

            return winOutcomeItem;
        }

        /// <summary>
        /// Shuffles a list of picks.
        /// </summary>
        /// <param name="picks">The list of picks to shuffle.</param>
        /// <param name="randomNumbers">The random number source.</param>
        /// <returns>A list with the shuffled picks from <paramref name="picks"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="picks"/> or <paramref name="randomNumbers"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="picks"/> contains zero items.
        /// </exception>
        public static IList<T> ShufflePicks<T>(IList<T> picks, IRandomNumbers randomNumbers)
        {
            if(picks == null)
            {
                throw new ArgumentNullException("picks");
            }

            if(picks.Count == 0)
            {
                throw new ArgumentException("The picks list must contain at least one item.");
            }

            if(randomNumbers == null)
            {
                throw new ArgumentNullException("randomNumbers");
            }

            IList<T> shuffledPicks;

            checked
            {
                var shuffleNumbers = randomNumbers.GetRandomNumbers(
                    new RandomValueRequest((uint)picks.Count, 0, picks.Count - 1, 0));

                shuffledPicks = shuffleNumbers.Select(randomNumber => picks[randomNumber]).ToList();
            }

            return shuffledPicks;
        }
    }
}
