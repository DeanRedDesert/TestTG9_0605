//-----------------------------------------------------------------------
// <copyright file = "ExternalJackpots.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using CompactSerialization;

    /// <summary>
    /// This class contains information of the collection of external jackpots.
    /// </summary>
    [Serializable]
    public class ExternalJackpots : ICompactSerializable
    {
        /// <summary>
        /// Gets or sets the flag indicating if the external jackpots is visible for display.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the ID of the icon to use.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// Gets or sets the collection of external jackpots.
        /// </summary>
        public List<ExternalJackpot> Jackpots { get; set; }

        #region Constructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ExternalJackpots"/> class.
        /// </summary>
        public ExternalJackpots()
        {
            Jackpots = new List<ExternalJackpot>();
        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, IsVisible);
            CompactSerializer.Write(stream, IconId);
            CompactSerializer.WriteList(stream, Jackpots);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            IsVisible = CompactSerializer.ReadBool(stream);
            IconId = CompactSerializer.ReadInt(stream);
            Jackpots = CompactSerializer.ReadListSerializable<ExternalJackpot>(stream);
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

            builder.AppendLine("External Jackpots (total:" + Jackpots.Count + ")");
            builder.AppendLine("\t IsVisible = " + IsVisible);
            builder.AppendLine("\t IconId = " + IconId);
            for(var index = 0; index < Jackpots.Count; index++)
            {
                builder.AppendLine("External Jackpot " + index + ":");
                builder.AppendLine(Jackpots[index].ToString());
            }

            return builder.ToString();
        }
    }
}
