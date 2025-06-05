//-----------------------------------------------------------------------
// <copyright file = "Outcome.cs" company = "IGT">
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
    /// of the automatically generated type Outcome.
    /// </summary>
    public partial class Outcome : ICompactSerializable, IOutcome
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
                throw new ArgumentNullException("outcome");
            }

            originField = (OutcomeOrigin)outcome.Origin;
            tagField = outcome.Tag;
            sourceField = outcome.Source;
            source_detailField = outcome.SourceDetail;
        }

        #endregion Constructors

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Outcome -");
            builder.AppendLine("\t  Origin = " + origin);
            builder.AppendLine("\t  Tag = " + tag);
            builder.AppendLine("\t  Source = " + source);
            builder.AppendLine("\t  Source Detail = " + source_detail);
            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public virtual void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, (int)origin);
            CompactSerializer.Write(stream, tag);
            CompactSerializer.Write(stream, source);
            CompactSerializer.Write(stream, source_detail);
        }

        /// <inheritdoc />
        public virtual void Deserialize(Stream stream)
        {
            origin = (OutcomeOrigin)CompactSerializer.ReadInt(stream);
            tag = CompactSerializer.ReadString(stream);
            source = CompactSerializer.ReadString(stream);
            source_detail = CompactSerializer.ReadString(stream);
        }

        #endregion

        #region IOutcome Members

        /// <inheritdoc />
        public Ascent.OutcomeList.Interfaces.OutcomeOrigin Origin
        {
            get { return (Ascent.OutcomeList.Interfaces.OutcomeOrigin)originField; }
        }

        /// <inheritdoc />
        public string Tag
        {
            get { return tagField; }
        }

        /// <inheritdoc />
        public string Source
        {
            get { return sourceField; }
        }

        /// <inheritdoc />
        public string SourceDetail
        {
            get { return source_detailField; }
        }

        #endregion IOutcome Members
    }
}
