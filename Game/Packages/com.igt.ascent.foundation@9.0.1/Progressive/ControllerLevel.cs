//-----------------------------------------------------------------------
// <copyright file = "ControllerLevel.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// The ControllerLevel encapsulates all the information needed
    /// for a controller level, including the settings, the amount
    /// tracking, and the hit metering.
    /// </summary>
    [Serializable]
    public class ControllerLevel
    {
        #region Constants

        /// <summary>
        /// Format string to generate the critical data paths for
        /// this controller level's amount, hit record and hit flag.
        /// It would read as something like:
        /// "ProgressiveController/Level3/Amount"
        /// </summary>
        protected const string CriticalDataPathFormat = "{0}/Level{1}/{2}";

        #endregion

        #region Fields and Properties

        /// <summary>
        /// Reference to the Game Lib needed for accessing
        /// critical data.
        /// </summary>
        protected readonly IGameLib GameLibReference;

        /// <summary>
        /// Critical data path for the Amount field.
        /// </summary>
        protected readonly string AmountPath;

        /// <summary>
        /// Critical data path for the Hit Record field.
        /// </summary>
        protected readonly string HitRecordPath;

        /// <summary>
        /// Critical data path for the Hit Flag field.
        /// </summary>
        protected readonly string HitFlagPath;

        /// <summary>
        /// Configuration for this controller level.
        /// </summary>
        public ProgressiveConfiguration Configuration { get; set; }

        /// <summary>
        /// Amount tracker for this controller level.
        /// </summary>
        public ProgressiveAmount Amount { get; protected set; }

        /// <summary>
        /// Hit meters for this controller level.
        /// </summary>
        public ProgressiveHitRecord HitRecord { get; protected set; }

        /// <summary>
        /// Flag indicating if this controller level has been hit.
        /// </summary>
        public bool HitFlag { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an instance of Controller Level for the specified level.
        /// Cache the reference to the game lib, and initialize paths for
        /// critical data.
        /// </summary>
        /// <param name="levelId">The 0-based controller level id.</param>
        /// <param name="iGameLib">Reference to a game lib needed for critical
        ///                        data operations.</param>
        /// <param name="controllerPath">Critical data path of the parent
        ///                              progressive controller.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="levelId"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> interface is null, or
        /// <paramref name="controllerPath"/> is null or empty.
        /// </exception>
        public ControllerLevel(int levelId, IGameLib iGameLib, string controllerPath)
        {
            if(levelId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelId), "Level Id cannot be negative.");
            }

            if(string.IsNullOrEmpty(controllerPath))
            {
                throw new ArgumentNullException(nameof(controllerPath), "Controller path cannot be null or empty.");
            }

            GameLibReference = iGameLib ?? throw new ArgumentNullException(nameof(iGameLib));

            // Critical data path will read as ControllerPath\LevelX\FieldName.
            AmountPath = string.Format(CriticalDataPathFormat, controllerPath, levelId, "Amount");
            HitRecordPath = string.Format(CriticalDataPathFormat, controllerPath, levelId, "HitRecord");
            HitFlagPath = string.Format(CriticalDataPathFormat, controllerPath, levelId, "HitFlag");

            Amount = new ProgressiveAmount();
            HitRecord = new ProgressiveHitRecord();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reload the fields from the critical data.
        /// </summary>
        public virtual void Reload()
        {
            ReloadAmount();
            ReloadHitRecord();
            ReloadHitFlag();

            // Adjust the displayable amount to be in the acceptable
            // range specified by the configuration.
            // This adjustment might not be needed if the menu
            // page is implemented with some validation rules.
            var newAmount = Math.Max(Amount.Displayable, Configuration.StartingAmount);
            newAmount = Math.Min(newAmount, Configuration.MaximumAmount);

            if(newAmount != Amount.Displayable)
            {
                Amount.Displayable = newAmount;
                Amount.Remainder = 0;
                // Leave Overflow intact since it is modifiable via
                // menu page as well, and we leave it to the operator's
                // judgment as whether this meter should be cleared out.

                SaveAmount();
            }
        }

        /// <summary>
        /// Apply wager-based contribution to the progressive amount of this controller level.
        /// </summary>
        /// <param name="betAmount">
        /// Player bet amount, in base units.
        /// </param>
        /// <param name="saveToCriticalData">
        /// A flag indicating if this contribution should be saved to critical data.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="betAmount"/> is less than 0.
        /// </exception>
        public virtual void Contribute(long betAmount, bool saveToCriticalData)
        {
            if(betAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betAmount), "Bet amount cannot be negative.");
            }

            // Only proceed if the level is NOT event-based.
            if(!Configuration.IsEventBased)
            {
                ApplyContribution(betAmount, saveToCriticalData);
            }
        }

        /// <summary>
        /// Apply an event-based contribution to the progressive amount of this controller level.
        /// </summary>
        /// <remarks>
        /// The contribution amount is in base units.  The value is calculated by dividing <paramref name="amountNumerator"/>
        /// by <paramref name="amountDenominator"/>. The result could contain a fractional amount of the base unit.
        ///
        /// Assuming currency is denoted in US dollars, if <paramref name="amountNumerator"/> is 99999,
        /// <paramref name="amountDenominator"/> is 30000, then the contribution amount would be
        /// 99999/30000 = 3.3333 cents.
        /// </remarks>
        /// <param name="amountNumerator">
        /// The numerator of the contribution amount.
        /// </param>
        /// <param name="amountDenominator">
        /// The denominator of the contribution amount, needed for specifying a fractional amount.
        /// </param>
        /// <param name="saveToCriticalData">
        /// A flag indicating if this contribution should be saved to critical data.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amountNumerator"/> is negative, or when <paramref name="amountDenominator"/> is 0 or negative.
        /// </exception>
        public virtual void ContributeEventBased(long amountNumerator, long amountDenominator,  bool saveToCriticalData)
        {
            if(amountNumerator < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amountNumerator), "Amount numerator cannot be negative.");
            }

            if(amountDenominator <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amountDenominator), "Amount denominator cannot be zero or negative.");
            }

            // Only proceed if the level is event-based.
            if(Configuration.IsEventBased)
            {
                ApplyContribution((decimal)amountNumerator / amountDenominator, saveToCriticalData);
            }
        }

        /// <summary>
        /// Perform metering for the progressive hit, flag the level
        /// to be reset later, and fill in the hit state, amount,
        /// and prize string fields in the passed in progressive award.
        /// </summary>
        /// <param name="progressiveAward">The progressive award to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="progressiveAward"/> is null.
        /// </exception>
        public virtual void MarkHit(ProgressiveAward progressiveAward)
        {
            if(progressiveAward == null)
            {
                throw new ArgumentNullException(nameof(progressiveAward));
            }

            // Meter the hit, and flag it to be reset later.
            checked
            {
                HitRecord.HitCount++;

                // If a prize is offered, the player is awarded with
                // the prize instead of the progressive amount.
                if(string.IsNullOrEmpty(Configuration.PrizeString))
                {
                    HitRecord.TotalHitAmount += Amount.Displayable;
                    HitFlag = true;
                }
                else
                {
                    // Don't reset the displayable amount later.
                    HitFlag = false;
                }
            }

            SaveHitRecord();
            SaveHitFlag();

            progressiveAward.UpdateAmountValue(HitFlag ? Amount.Displayable : 0);
            progressiveAward.UpdateHitState(ProgressiveAwardHitState.Hit);
            progressiveAward.UpdatePrizeString(Configuration.PrizeString);
        }

        /// <summary>
        /// Reset the displayable amount to the starting amount
        /// specified by the configuration, plus appropriate amount
        /// from the overflow meter.
        /// </summary>
        public virtual void ResetHit()
        {
            if(HitFlag)
            {
                // Reset the displayable amount to the starting amount
                // plus some overflow amount, up to the maximum amount.
                var transferAmount = Math.Min(Amount.Overflow,
                                              Configuration.MaximumAmount - Configuration.StartingAmount);

                Amount.Displayable = Configuration.StartingAmount + transferAmount;
                Amount.Overflow -= transferAmount;

                // Reset the hit flag.
                HitFlag = false;

                // Save to the critical data.
                SaveAmount();
                SaveHitFlag();
            }
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ControllerLevel -");
            builder.AppendLine("\t  Progressive Configuration = " + Configuration);
            builder.AppendLine("\t  Progressive Amount = " + Amount);
            builder.AppendLine("\t  Progressive Hit Record = " + HitRecord);

            return builder.ToString();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Read the progressive amount from the critical data.
        /// </summary>
        protected virtual void ReloadAmount()
        {
            Amount = GameLibReference.ReadCriticalData<ProgressiveAmount>(CriticalDataScope.Theme, AmountPath) ??
                     new ProgressiveAmount();
        }

        /// <summary>
        /// Read the progressive hit record from the critical data.
        /// </summary>
        protected virtual void ReloadHitRecord()
        {
            HitRecord = GameLibReference.ReadCriticalData<ProgressiveHitRecord>(CriticalDataScope.Theme, HitRecordPath) ??
                        new ProgressiveHitRecord();
        }

        /// <summary>
        /// Read the hit flag from the critical data.
        /// </summary>
        protected virtual void ReloadHitFlag()
        {
            HitFlag = GameLibReference.ReadCriticalData<bool>(CriticalDataScope.Theme, HitFlagPath);
        }

        /// <summary>
        /// Write the progressive amount to the critical data.
        /// </summary>
        protected virtual void SaveAmount()
        {
            GameLibReference.WriteCriticalData(CriticalDataScope.Theme, AmountPath, Amount);
        }

        /// <summary>
        /// Write the progressive hit record to the critical data.
        /// </summary>
        protected virtual void SaveHitRecord()
        {
            GameLibReference.WriteCriticalData(CriticalDataScope.Theme, HitRecordPath, HitRecord);
        }

        /// <summary>
        /// Write the hit flag to the critical data.
        /// </summary>
        protected virtual void SaveHitFlag()
        {
            GameLibReference.WriteCriticalData(CriticalDataScope.Theme, HitFlagPath, HitFlag);
        }

        /// <summary>
        /// Add the given contribution amount to the progressive amount, and escrow fractional amount if any.
        /// Write the new progressive amount to critical data.
        /// </summary>
        /// <param name="amount">The contribution amount to apply.</param>
        /// <param name="saveToCriticalData">A flag indicating if the contribution should be persisted to safe storage.</param>
        protected virtual void ApplyContribution(decimal amount, bool saveToCriticalData)
        {
            checked
            {
                var contribution = amount * (decimal)Configuration.ContributionPercentage + Amount.Remainder;

                // Get the rounded contribution amount.
                var roundedContribution = (long)Math.Floor(contribution);

                // Record the remainder.
                Amount.Remainder = contribution - roundedContribution;

                // Apply the contribution if the progressive displayable
                // amount is not maxed out yet.
                if(Amount.Displayable < Configuration.MaximumAmount)
                {
                    Amount.Displayable += roundedContribution;

                    if(Amount.Displayable > Configuration.MaximumAmount)
                    {
                        Amount.Overflow = Amount.Displayable - Configuration.MaximumAmount;
                        Amount.Displayable = Configuration.MaximumAmount;
                    }
                }
                // Add the contribution to the overflow if the displayable
                // is maxed out already.  Make sure the overflow is under
                // the data type limit.
                else if(roundedContribution <= long.MaxValue - Amount.Overflow)
                {
                    Amount.Overflow += roundedContribution;
                }
                // Cap the overflow at the data type limit.  Discard the rest contribution.
                else
                {
                    Amount.Overflow = long.MaxValue;
                }
            }

            if(saveToCriticalData)
            {
                SaveAmount();
            }
        }

        #endregion
    }
}
