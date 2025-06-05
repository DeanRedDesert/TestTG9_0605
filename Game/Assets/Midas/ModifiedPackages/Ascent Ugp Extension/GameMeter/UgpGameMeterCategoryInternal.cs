//-----------------------------------------------------------------------
// <copyright file = "UgpGameMeterCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter
{
    using System;
    using System.Xml.Serialization;
    using F2L.Schemas.Internal;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpGameMeterCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryInternal : UgpCategoryBase<UgpGameMeterCategoryMessage>
    {
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryMessage : ICategoryMessage
    {
        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("AspGameMeterSend", typeof(UgpGameMeterCategoryAspGameMeterSend))]
        [XmlElement("AspGameMeterReply", typeof(UgpGameMeterCategoryAspGameMeterReply))]
        [XmlElement("UpdateGameBetMeterSend", typeof(UgpGameMeterCategoryGameBetMeterUpdateSend))]
        [XmlElement("UpdateGameBetMeterReply", typeof(UgpGameMeterCategoryGameBetMeterUpdateReply))]
        [XmlElement("UpdateGameCreditDenomSend", typeof(UgpGameMeterCategoryGameCreditDenomSend))]
        [XmlElement("UpdateGameCreditDenomReply", typeof(UgpGameMeterCategoryGameCreditDenomReply))]
        public object Item { get; set; }
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryAspGameMeterSend : UgpCategorySend
    {
        /// <summary>
        /// The current bet per line.
        /// </summary>
        public AmountType CurrentBetPerLine { get; set; }

        /// <summary>
        /// The number of lines currently selected.
        /// </summary>
        public int CurrentlySelectedLines { get; set; }
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryAspGameMeterReply : UgpCategoryReply
    {
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryGameBetMeterUpdateSend : UgpCategorySend
    {
        /// <summary>
        /// The Horizontal Key name in game bet meter that matches this game bet action.
        /// </summary>
        public string HorizontalKey { get; set; }

        /// <summary>
        /// The Vertical Key name in game bet meter that matches this game bet action.
        /// </summary>
        public string VerticalKey { get; set; }
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryGameBetMeterUpdateReply : UgpCategoryReply
    {
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryGameCreditDenomSend : UgpCategorySend
    {
        /// <summary>
        /// The current credit denomination set by the game.
        /// </summary>
        public AmountType GameCreditDenom { get; set; }
    }

    /// <summary/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameMeterCategoryGameCreditDenomReply : UgpCategoryReply
    {
    }
}