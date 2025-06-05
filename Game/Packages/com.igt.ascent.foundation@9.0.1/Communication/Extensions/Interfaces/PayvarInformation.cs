// -----------------------------------------------------------------------
// <copyright file = "PayvarInformation.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    ///  This class represents the paytable variants associated to a specific denomination.
    /// </summary>
    [Serializable]
    public class PayvarInformation : ICompactSerializable
    {
        #region Properties
        
        /// <summary>
        /// The enabled denomination that is linked to current payvar.
        /// </summary>
        public long EnabledDenom { get; private set; }
        
        /// <summary>
        /// Custom payvar related file for use with the game.
        /// </summary>
        public string TagFilePath { get;  private set; }
        
        /// <summary>
        /// Custom payvar related data for use with the game.
        /// </summary>
        public string Tag { get;  private set; }

        /// <summary>
        /// The maximum bet amount(in base units) for current payvar.
        /// </summary>
        public long MaxBet { get;  private set; }
        
        /// <summary>
        /// The minimum bet amount(in base units) for current payvar.
        /// </summary>
        public long MinBet { get;  private set; }
        
        /// <summary>
        /// The minimum bet amount(in base units) the button panel allows for current payvar.
        /// </summary>
        public long ButtonPanelMinBet { get;  private set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// This parameterless constructor is used for serialization purposes.
        /// </summary>
        public PayvarInformation()
        {
        }

        /// <summary>
        ///  Construct the instance with the paytable variants.
        /// </summary>
        /// <param name="enabledDenom">The enabled denomination that is linked to current payvar.</param>
        /// <param name="tagFilePath">Custom payvar related file for use with the game.</param>
        /// <param name="tag">Custom payvar related data for use with the game.</param>
        /// <param name="maxBet">The maximum bet amount(in base units) for current payvar.</param>
        /// <param name="minBet">The minimum bet amount(in base units) for current payvar.</param>
        /// <param name="buttonPanelMinBet">
        /// The minimum bet amount(in base units) the button panel allows for current payvar.
        /// </param>
        public PayvarInformation(long enabledDenom, string tagFilePath, string tag, long maxBet, long minBet,
            long buttonPanelMinBet)
        {
            EnabledDenom = enabledDenom;
            TagFilePath = tagFilePath;
            Tag = tag;
            MaxBet = maxBet;
            MinBet = minBet;
            ButtonPanelMinBet = buttonPanelMinBet;
        }

        #endregion
        
        #region Implementation of ICompactSerializable
        
        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, EnabledDenom);
            CompactSerializer.Write(stream, TagFilePath);
            CompactSerializer.Write(stream, Tag);
            CompactSerializer.Write(stream, MaxBet);
            CompactSerializer.Write(stream, MinBet);
            CompactSerializer.Write(stream, ButtonPanelMinBet);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            EnabledDenom = CompactSerializer.ReadLong(stream);
            TagFilePath = CompactSerializer.ReadString(stream);
            Tag = CompactSerializer.ReadString(stream);
            MaxBet = CompactSerializer.ReadLong(stream);
            MinBet = CompactSerializer.ReadLong(stream);
            ButtonPanelMinBet = CompactSerializer.ReadLong(stream);
        }
        
        #endregion
    }
}