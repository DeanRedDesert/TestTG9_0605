//-----------------------------------------------------------------------
// <copyright file = "OutcomeListFeatureEntryFeatureRngNumbers.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type OutcomeListFeatureEntryFeatureRngNumbers.
    /// </summary>
    public partial class OutcomeListFeatureEntryFeatureRngNumbers : ICompactSerializable
    {
        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeListFeatureEntryFeatureRngNumbers -");

            builder.AppendFormat("\t  < {0} Random Numbers >{1}", Number.Count, Environment.NewLine);
            for (var i = 0; i < Number.Count; i++)
            {
                builder.AppendFormat("\t  #{0}", i + 1);
                builder.AppendFormat("\t  {0}{1}", Number[i], Environment.NewLine);
            }
            builder.AppendLine("\t  < Random Numbers End >");
            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Number);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Number = CompactSerializer.ReadListInt(stream);
        }

        #endregion
    }
}
