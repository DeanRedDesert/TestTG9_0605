//-----------------------------------------------------------------------
// <copyright file = "RequiredBetRange.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class represents a required bet and tells whether a given bet meets its requirements.
    /// </summary>
    public class RequiredBetRange : IComparable<RequiredBetRange>
    {
        /// <summary>
        /// Value which indicates that any possible bet is valid.
        /// </summary>
        public const string BetAll = "ALL";

        /// <summary>
        /// Value which indicates the game must have been played at maximum bet.
        /// </summary>
        public const string BetMax = "MAX";

        /// <summary>
        /// The minimum value of the required bet range.
        /// </summary>
        private readonly uint rangeMin;

        /// <summary>
        /// The maximum value of the required bet range.
        /// If the bet must be MAX, then rangeMax is uint.Max
        /// </summary>
        private readonly uint rangeMax;

        private const string InvalidRequiredBet = "Required bet must be empty, ALL, MAX, #, #-#, or #-MAX, where # is a positive integer.";
        private const string InvalidRequiredBetMin = "Cannot parse the required bet into a valid value.";
        private const string InvalidRequiredBetRange = "Cannot parse the required bet into a valid range.";
        private const string InvalidRequiredBetOrder = "Required bet minimum cannot be greater than required bet maximum.";

        /// <summary>
        /// For validating the required total bet and the required bet within a required pattern..
        /// </summary>
        // TODO: unused private variable
        // ReSharper disable once UnusedMember.Local
        private static readonly Regex RequiredBetRegex =
            new Regex("^(" +                                  // Match beginning of line, so no unmatched content is accepted.  Then match one of:
                      "(" + BetAll + ")" + "|" +              //   "ALL" 
                      "(" + BetMax + ")" + "|" +              //   "MAX"
                      "(\\d+(\\-((\\d+)|(" + BetMax + ")))?)" //   #, or #-#, or #-MAX, where # is a number.
                      + ")$", RegexOptions.IgnoreCase);       // Finally, match end of line, so no unmatched content is accepted.

        /// <summary>
        /// Return the minimum value of the required bet range.
        /// </summary>
        public uint RangeMin => rangeMin;

        /// <summary>
        /// Return the maximum value of the required bet range.
        /// </summary>
        public uint RangeMax => rangeMax;

        /// <summary>
        /// Construct an instance of a RequiredBetRange from a single integer.
        /// </summary>
        public RequiredBetRange()
        {
            // No range was specified, so allow all possible bets.
            rangeMin = 0;
            rangeMax = uint.MaxValue;
        }

        /// <summary>
        /// Construct an instance of a RequiredBetRange from a single integer.
        /// </summary>
        /// <param name="requiredBet">The integer value of the required bet.</param>
        public RequiredBetRange(uint requiredBet)
        {
            rangeMin = requiredBet;

            // When instantiating from a string (for RequiredTotalBet or RequiredPattern.BetAmountRequired)
            // All numeric values, including "0" are interpreted as a literal requirement.  When instantiating
            // from a single integer (for RequiredBetOnPattern), 0 is interpreted as no requirement, allowing
            // any bet. This is to support the legacy behavior, prior to the creation of RequiredBetRanges.
            rangeMax = requiredBet == 0 ? uint.MaxValue : requiredBet;
        }

        /// <summary>
        /// Construct an instance of a RequiredBetRange from two integers specifying a range.
        /// </summary>
        /// <param name="requiredBetMin">The minimum value of the range.</param>
        /// <param name="requiredBetMax">The maximum value of the range.</param>
        public RequiredBetRange(uint requiredBetMin, uint requiredBetMax)
        {
            rangeMin = requiredBetMin;
            rangeMax = requiredBetMax;

            if(rangeMin > rangeMax)
            {
                throw new ArgumentException(InvalidRequiredBetOrder, nameof(requiredBetMin));
            }
        }

        /// <summary>
        /// Construct an instance of a RequiredBetRange from a required bet string.
        /// </summary>
        /// <param name="requiredBet">
        /// A string value from a paytable, containing one of the following:
        /// "ALL" - any bet is acceptable.
        /// "MAX" - The bet must be the max bet.
        /// an integer - the bet must exactly match that value.
        /// two integers, separated by a dash ('-') - The bet must be in the range given.
        /// an integer followed by "-MAX" - The bet must be the given value or higher.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="requiredBet"/> is not in an accepted format.</exception>
        public RequiredBetRange(string requiredBet)
        {
            if(string.IsNullOrEmpty(requiredBet) || string.Equals(requiredBet, BetAll, StringComparison.OrdinalIgnoreCase))
            {
                rangeMin = 0;
                rangeMax = uint.MaxValue;
            }

            // Check for "MAX".
            else if(string.Equals(requiredBet, BetMax, StringComparison.OrdinalIgnoreCase))
            {
                rangeMin = uint.MaxValue;
                rangeMax = uint.MaxValue;
            }

            // Check for a number or a range.
            else
            {
                var rangeLimits = requiredBet.Split('-');
                switch(rangeLimits.Length)
                {
                    // Check for a single integer
                    case 1:
                        if(uint.TryParse(rangeLimits[0], out rangeMin))
                        {
                            // When instantiating from a string (for RequiredTotalBet or RequiredPattern.BetAmountRequired)
                            // All numeric values, including "0" are interpreted as a literal requirement.  When instantiating
                            // from a single integer (for RequiredBetOnPattern), 0 is interpreted as no requirement, allowing
                            // any bet. This is to support the legacy behavior, prior to the creation of RequiredBetRanges.
                            rangeMax = rangeMin;
                        }
                        else
                        {
                            throw new ArgumentException(InvalidRequiredBetMin, requiredBet);
                        }
                        break;

                    // Check for "integer-integer" or "integer-max"
                    case 2:
                        // First value must be an integer.
                        if(!uint.TryParse(rangeLimits[0], out rangeMin))
                        {
                            throw new ArgumentException(InvalidRequiredBetMin, requiredBet);
                        }

                        // Second value can be "MAX" or an integer.
                        if(string.Equals(rangeLimits[1], BetMax, StringComparison.OrdinalIgnoreCase))
                        {
                            rangeMax = uint.MaxValue;
                        }
                        else if(!uint.TryParse(rangeLimits[1], out rangeMax))
                        {
                            throw new ArgumentException(InvalidRequiredBetRange, requiredBet);
                        }
                        if(rangeMin > rangeMax)
                        {
                            throw new ArgumentException(InvalidRequiredBetOrder, requiredBet);
                        }
                        break;
                    default:
                        throw new ArgumentException(InvalidRequiredBet, requiredBet);
                }
            }
        }

        /// <summary>
        /// Tell if the given bet meets the required bet.
        /// </summary>
        /// <param name="betAmount">The bet to compare against this <see cref="RequiredBetRange"/>.</param>
        /// <param name="isMax">A flag indicating whether the bet is the max bet.</param>
        /// <returns>
        /// True if <paramref name="betAmount"/> meets this <see cref="RequiredBetRange"/>'s requirements.  False if not.
        /// </returns>
        public bool RequiredBetMet(uint betAmount, bool isMax)
        {
            if(IsMax())
            {
                return isMax;
            }
            return rangeMin <= betAmount && betAmount <= rangeMax;
        }

        /// <summary>
        /// Tell if the required bet range represents a max bet only.
        /// </summary>
        /// <returns>True if the required bet range represents a max bet only; false otherwise.</returns>
        public bool IsMax()
        {
            return rangeMin == uint.MaxValue;
        }

        /// <summary>
        /// Tell if the required bet range accepts all bets.
        /// </summary>
        /// <returns>True if the required bet range accepts all bets; false otherwise.</returns>
        public bool IsAll()
        {
            return rangeMin == 0 && rangeMax == uint.MaxValue;
        }

        /// <summary>
        /// Convert the required bet to a string.
        /// </summary>
        /// <returns>A string representation of the <see cref="RequiredBetRange"/>.</returns>
        public override string ToString()
        {
            if(IsAll())
            {
                return BetAll;
            }
            
            if(IsMax())
            {
                return BetMax;
            }
            
            if(rangeMin == rangeMax)
            {
                return $"{rangeMin}";
            }

            return rangeMax == uint.MaxValue
                ? $"{rangeMin}-MAX"
                : $"{rangeMin}-{rangeMax}";
        }

        /// <summary>
        /// Attempt to parse a string into a <see cref="RequiredBetRange"/>.
        /// </summary>
        /// <param name="requiredBet">The <see cref="string"/> to parse.</param>
        /// <param name="errorMessage">
        /// A <see cref="string"/> containing an error message, if parsing <paramref name="requiredBet"/> failed.
        /// If parsing <paramref name="requiredBet"/> succeeded, <paramref name="errorMessage"/> will be empty.
        /// </param>
        /// <returns>
        /// True if parsing <paramref name="requiredBet"/> succeeded; false otherwise.
        /// </returns>
        public static bool TryParse(string requiredBet, out string errorMessage)
        {
            return TryParse(requiredBet, out _, out errorMessage);
        }

        /// <summary>
        /// Attempt to parse a string into a <see cref="RequiredBetRange"/>.
        /// </summary>
        /// <param name="requiredBet">The <see cref="string"/> to parse into a <see cref="RequiredBetRange"/>.</param>
        /// <param name="requiredBetRange">
        /// The <see cref="RequiredBetRange"/> to store the parsed value in.
        /// If parsing fails, <paramref name="requiredBetRange"/> will be null.
        /// </param>
        /// <param name="errorMessage">
        /// A <see cref="string"/> containing an error message, if parsing <paramref name="requiredBet"/> failed.
        /// If parsing <paramref name="requiredBet"/> succeeded, <paramref name="errorMessage"/> will be empty.
        /// </param>
        /// <returns>
        /// True if parsing <paramref name="requiredBet"/> succeeded; false otherwise.
        /// </returns>
        public static bool TryParse(string requiredBet, out RequiredBetRange requiredBetRange, out string errorMessage)
        {
            try
            {
                requiredBetRange = new RequiredBetRange(requiredBet);
                errorMessage = string.Empty;
                return true;
            }
            catch(Exception exception)
            {
                requiredBetRange = null;
                errorMessage = exception.Message;
                return false;
            }
        }

        #region IComparable Implementation

        /// <inheritDoc/>
        public virtual int CompareTo(RequiredBetRange other)
        {
            if(rangeMax > other.rangeMax)
            {
                return 1;
            }
            if(rangeMax < other.rangeMax)
            {
                return -1;
            }

            if(rangeMin > other.rangeMin)
            {
                return 1;
            }
            if(rangeMin < other.rangeMin)
            {
                return -1;
            }

            return 0;
        }

        #endregion    
    }
}
