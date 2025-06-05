//-----------------------------------------------------------------------
// <copyright file = "Outcome.cs" company = "IGT">
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
    /// Base type for all outcomes and awards.
    /// </summary>
    [Serializable]
    public class Outcome : IOutcome, ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable.
        /// </summary>
        public Outcome()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Outcome"/> using 
        /// an <see cref="IOutcome"/> implementation object.
        /// </summary>
        /// <param name="outcome">
        /// An implementation of <see cref="IOutcome"/> to use for initialization.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="outcome"/> is null.
        /// </exception>
        public Outcome(IOutcome outcome)
        {
            if(outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            Origin = outcome.Origin;
            Tag = outcome.Tag;
            Source = outcome.Source;
            SourceDetail = outcome.SourceDetail;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Outcome"/> using passed in arguments.
        /// </summary>
        /// <param name="origin">
        /// Optional. The <see cref="OutcomeOrigin"/> for this outcome.
        /// </param>
        /// <param name="tag">
        /// Optional. The identifying tag for the outcome.
        /// </param>
        /// <param name="source">
        /// Optional. The source field of who created this outcome.
        /// </param>
        /// <param name="sourceDetail">
        /// Optional. Additional details for the source of this outcome.
        /// </param>
        public Outcome(
            OutcomeOrigin origin = OutcomeOrigin.Client,
            string tag = null,
            string source = null,
            string sourceDetail = null)
        {
            Origin = origin;
            Tag = tag;
            Source = source;
            SourceDetail = sourceDetail;
        }

        #endregion Constructors

        #region internal methods

        /// <summary>
        /// Internal method for updating the Tag property.
        /// </summary>
        /// <param name="tag">The <see cref="string"/> to update to.</param>
        internal void UpdateTag(string tag)
        {
            Tag = tag;
        }

        #endregion internal methods

        #region IOutcome Members

        /// <inheritdoc />
        public OutcomeOrigin Origin { get; protected set; }

        /// <inheritdoc />
        public string Tag { get; protected set; }

        /// <inheritdoc />
        public string Source { get; protected set; }

        /// <inheritdoc />
        public string SourceDetail { get; protected set; }

        #endregion IOutcome Members

        #region ICompactSerializable Members

        /// <inheritdoc />
        public virtual void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Origin);
            CompactSerializer.Write(stream, Tag);
            CompactSerializer.Write(stream, Source);
            CompactSerializer.Write(stream, SourceDetail);
        }

        /// <inheritdoc />
        public virtual void Deserialize(Stream stream)
        {
            Origin = CompactSerializer.ReadEnum<OutcomeOrigin>(stream);
            Tag = CompactSerializer.ReadString(stream);
            Source = CompactSerializer.ReadString(stream);
            SourceDetail = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region ToString Override

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Outcome -");
            builder.AppendLine("\t  Origin = " + Origin);
            builder.AppendLine("\t  Tag = " + Tag);
            builder.AppendLine("\t  Source = " + Source);
            builder.AppendLine("\t  Source Detail = " + SourceDetail);
            builder.AppendLine();

            return builder.ToString();
        }

        #endregion ToString Override
    }
}
