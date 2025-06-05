//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.GameCyclePlay
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using IGT.Ascent.OutcomeList.Interfaces;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type ProgressiveAward.
    /// </summary>
    public partial class ProgressiveAward : IProgressiveAward
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

            HitState = (ProgressiveAwardHitState)award.HitState;

            if(award.GameLevel != null)
            {
                GameLevel = (uint)award.GameLevel;
                GameLevelSpecified = true;
            }
            else
            {
                GameLevel = 0;
                GameLevelSpecified = false;
            }
        }

        #endregion Constructors

        #region IProgressiveAward Members
        
        /// <inheritdoc />
        Ascent.OutcomeList.Interfaces.ProgressiveAwardHitState IProgressiveAward.HitState => (Ascent.OutcomeList.Interfaces.ProgressiveAwardHitState)HitState;

        /// <inheritdoc />
        uint? IProgressiveAward.GameLevel => GameLevelSpecified ? (uint?)GameLevel : null;

        #endregion IProgressiveAward Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, HitState);
            CompactSerializer.Write(stream, GameLevel);
            CompactSerializer.Write(stream, GameLevelSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            HitState = CompactSerializer.ReadEnum<ProgressiveAwardHitState>(stream);
            GameLevel = CompactSerializer.ReadUint(stream);
            GameLevelSpecified = CompactSerializer.ReadBool(stream);
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
            builder.AppendLine("\t  Game Level Specified = " + GameLevelSpecified);
            builder.AppendLine("\t  Game Level = " + GameLevel);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
