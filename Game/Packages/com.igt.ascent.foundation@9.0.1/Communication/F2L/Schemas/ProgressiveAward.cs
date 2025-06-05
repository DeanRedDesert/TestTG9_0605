//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.OutcomeList.Interfaces;
    using CompactSerialization;

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
                throw new ArgumentNullException("award");
            }

            hit_stateField = (ProgressiveAwardHit_state)award.HitState;
            game_levelFieldSpecified = award.GameLevel.HasValue;
            game_levelField = award.GameLevel.GetValueOrDefault();
        }

        #endregion Constructors

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ProgressiveAward -");
            builder.AppendLine("\t  Hit State = " + hit_state);
            builder.AppendLine("\t  Game Level Specified = " + game_levelSpecified);
            builder.AppendLine("\t  Game Level = " + game_level);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, (int)hit_state);
            CompactSerializer.Write(stream, game_level);
            CompactSerializer.Write(stream, game_levelSpecified);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            hit_state = (ProgressiveAwardHit_state)CompactSerializer.ReadInt(stream);
            game_level = CompactSerializer.ReadUint(stream);
            game_levelSpecified = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IProgressiveAward Members

        /// <inheritdoc />
        public ProgressiveAwardHitState HitState
        {
            get { return (ProgressiveAwardHitState)hit_stateField; }
        }

        /// <inheritdoc />
        public uint? GameLevel
        {
            get { return game_levelField; }
        }

        #endregion IProgressiveAward Members
    }
}
