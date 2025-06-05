// -----------------------------------------------------------------------
// <copyright file = "ExternalJackpot.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// This class contains information of an external jackpot.
    /// </summary>
    [Serializable]
    public class ExternalJackpot : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Jackpot's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Jackpot's value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the jackpot is visible for display.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the Jackpot's Icon Id.
        /// </summary>
        public int IconId { get; set; }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, Value);
            CompactSerializer.Write(stream, IsVisible);
            CompactSerializer.Write(stream, IconId);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Name = CompactSerializer.ReadString(stream);
            Value = CompactSerializer.ReadString(stream);
            IsVisible = CompactSerializer.ReadBool(stream);
            IconId = CompactSerializer.ReadInt(stream);
        }

        #endregion

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("External Jackpot -");
            builder.AppendLine("\t Name = " + Name);
            builder.AppendLine("\t Value = " + Value);
            builder.AppendLine("\t IsVisible = " + IsVisible);
            builder.AppendLine("\t IconId = " + IconId);

            return builder.ToString();
        }
    }
}
