//-----------------------------------------------------------------------
// <copyright file = "ProgressiveNearHit.cs" company = "IGT">
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
    /// Used to communicate progressive near-hit outcomes.
    /// </summary>
    [Serializable]
    public class ProgressiveNearHit : Outcome, IProgressiveNearHit
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public ProgressiveNearHit()
        {
        }

        /// <summary>
        /// Constructor.  Creates a ProgressiveNearHit from an IProgressiveNearHit.
        /// </summary>
        /// <param name="nearHit">
        /// An implementation of <see cref="IProgressiveNearHit"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nearHit"/> is null.
        /// </exception>
        public ProgressiveNearHit(IProgressiveNearHit nearHit) : base(nearHit)
        {
            if(nearHit == null)
            {
                throw new ArgumentNullException(nameof(nearHit));
            }

            GameLevel = nearHit.GameLevel;
        }

        /// <summary>
        /// Constructor.  Initializes a new instance of <see cref="ProgressiveNearHit"/> using passed in arguments.
        /// </summary>
        /// <param name="origin">
        /// The <see cref="OutcomeOrigin"/> for this outcome.
        /// </param>
        /// <param name="tag">
        /// The identifying tag for the outcome.
        /// </param>
        /// <param name="source">
        /// Optional. The source field of who created this outcome. Defaults to null.
        /// </param>
        /// <param name="sourceDetail">
        /// Optional. Additional details for the source of this outcome. Defaults to null.
        /// </param>
        /// <param name="gameLevel">
        /// Optional. The award level of this near hit. If null, this is not specified. Defaults to null.
        /// </param>
        public ProgressiveNearHit(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            string tag = null,
            uint? gameLevel = null,
            string source = null,
            string sourceDetail = null) : base(origin, tag, source, sourceDetail)
        {
            GameLevel = gameLevel;
        }

        #endregion Constructors

        #region IProgressiveNearHit Members

        /// <inheritdoc />
        public uint? GameLevel { get; private set; }

        #endregion IProgressiveNearHit Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            CompactSerializer.Write(stream, GameLevel);
        }

        /// <inheritdoc />
        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

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

            builder.AppendLine("ProgressiveNearHit -");
            builder.AppendLine("\t  Game Level = " + GameLevel);
            builder.Append("\t\t  - " + base.ToString());

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
