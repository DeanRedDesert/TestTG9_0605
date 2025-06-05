//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// Contains progressive win level data.  Used for progressive hits and validation.
    /// </summary>
    [Serializable]
    public class ProgressiveAward : Award, IProgressiveAward
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public ProgressiveAward()
        {
        }

        /// <summary>
        /// Constructor.  Creates a ProgressiveAward from an IProgressiveAward.
        /// </summary>
        /// <param name="award">An implementation of <see cref="IProgressiveAward"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="award"/> is null.
        /// </exception>
        public ProgressiveAward(IProgressiveAward award) : base(award)
        {
            if(award == null)
            {
                throw new ArgumentNullException(nameof(award));
            }

            HitState = award.HitState;
            GameLevel = award.GameLevel;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="ProgressiveAward"/> using passed in arguments.
        /// </summary>
        /// <param name="amount">
        /// The amount of the award in base units.
        /// </param>
        /// <param name="isDisplayable">
        /// Determines if this reward is displayable.
        /// </param>
        /// <param name="reason">
        /// The reason to display to the user for the award.
        /// </param>
        /// <param name="prize">
        /// The displayable prize string
        /// .</param>
        /// <param name="isTopAward">
        /// Optional. Determines if this is the top award available. Defaults to false.
        /// </param>
        /// <param name="hitState">
        /// The hit state for this progressive award.
        /// </param>
        /// <param name="gameLevel">
        /// Optional. The level of this award. If null, this is not specified. Defaults to null.
        /// </param>
        /// <param name="origin">
        /// The <see cref="OutcomeOrigin"/> for this outcome.
        /// </param>
        /// <param name="tag">
        /// Optional. The identifying tag for the outcome. Defaults to null.
        /// </param>
        /// <param name="source">
        /// Optional. The source field of who created this outcome. Defaults to null.
        /// </param>
        /// <param name="sourceDetail">
        /// Optional. Additional details for the source of this outcome. Defaults to null.
        /// </param>
        public ProgressiveAward(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            long amount = 0,
            bool isDisplayable = false,
            string reason = null,
            string prize = null,
            ProgressiveAwardHitState hitState = ProgressiveAwardHitState.PotentialHit,
            uint? gameLevel = null,
            bool isTopAward = false,
            string tag = null,
            string source = null,
            string sourceDetail = null) : base(origin, amount, isDisplayable, reason, prize, isTopAward, tag, source, sourceDetail)
        {
            HitState = hitState;
            GameLevel = gameLevel;
        }

        #endregion Constructors

        #region internal methods

        /// <summary>
        /// Internal method for updating the HitState property.
        /// </summary>
        /// <param name="state">The <see cref="ProgressiveAwardHitState"/> to update to.</param>
        internal void UpdateHitState(ProgressiveAwardHitState state)
        {
            HitState = state;
        }

        #endregion internal methods

        #region IProgressiveAward Members

        /// <inheritdoc />
        public ProgressiveAwardHitState HitState { get; protected set; }

        /// <inheritdoc />
        public uint? GameLevel { get; protected set; }

        #endregion IProgressiveAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, HitState);
            CompactSerializer.Write(stream, GameLevel);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            HitState = CompactSerializer.ReadEnum<ProgressiveAwardHitState>(stream);
            GameLevel = CompactSerializer.ReadNullable<uint>(stream);
        }

        #endregion

        #region ToString Override

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ProgressiveAward -");
            builder.AppendLine("\t  Hit State = " + HitState);
            builder.AppendLine("\t  Game Level = " + GameLevel);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
