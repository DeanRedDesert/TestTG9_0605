//-----------------------------------------------------------------------
// <copyright file = "Outcome.cs" company = "IGT">
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
    /// of the automatically generated type Outcome.
    /// </summary>
    public partial class Outcome : IOutcome, ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Default Parameter-less Constructor for ICompactSerializable
        /// </summary>
        public Outcome()
        {
        }

        /// <summary>
        /// Constructor.  Creates an Outcome from an existing IOutcome.
        /// </summary>
        /// <param name="outcome">An implementation of <see cref="IOutcome"/> to use for initialization.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="outcome"/> is null.
        /// </exception>
        public Outcome(IOutcome outcome)
        {
            if(outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome));
            }

            Origin = (OutcomeOrigin)outcome.Origin;
            Tag = outcome.Tag;
            Source = outcome.Source;
            SourceDetail = outcome.SourceDetail;
        }

        #endregion Constructors

        #region IOutcome Members

        /// <inheritdoc />
        Ascent.OutcomeList.Interfaces.OutcomeOrigin IOutcome.Origin => (Ascent.OutcomeList.Interfaces.OutcomeOrigin)Origin;

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
        /// <returns>A string describing the object.</returns>
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
