//-----------------------------------------------------------------------
// <copyright file = "UgpRunTimeGameEventCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RunTimeGameEvent
{
    using System;
    using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpGameEventCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpRunTimeGameEventCategoryInternal : UgpCategoryBase<UgpRunTimeGameEventsCategoryMessage>
    {
        /// <summary>
        /// Specifies the game input type.
        /// </summary>
        public enum GameInputType
        {
            /// <summary>
            /// Take Win.
            /// </summary>
            TakeWin,

            /// <summary>
            /// Start feature.
            /// </summary>
            StartFeature,

            /// <summary>
            /// Player selection.
            /// </summary>
            PlayerSelection,

            /// <summary>
            /// Generic input.
            /// </summary>
            Generic,
        }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpRunTimeGameEventsCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("GameEventWaitingForInputSend", typeof(UgpGameEventCategoryWaitingForInputSend))]
        [XmlElement("GameEventWaitingForInputReply", typeof(UgpGameEventCategoryWaitingForInputReply))]
        [XmlElement("GameEventPlayerChoiceSend", typeof(UgpGameEventCategoryPlayerChoiceSend))]
        [XmlElement("GameEventPlayerChoiceReply", typeof(UgpGameEventCategoryPlayerChoiceReply))]
        [XmlElement("GameEventDenomSelectionActiveSend", typeof(UgpGameEventCategoryDenomSelectionActiveSend))]
        [XmlElement("GameEventDenomSelectionActiveReply", typeof(UgpGameEventCategoryDenomSelectionActiveReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryWaitingForInputSend : UgpCategorySend
    {
        /// <summary>
        /// True if it is waiting , false if the waiting is finished.
        /// </summary>
        public bool IsWaiting { get; set; }

        /// <summary>
        /// Input type that game is waiting for.
        /// </summary>
        public UgpRunTimeGameEventCategoryInternal.GameInputType InputType { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryWaitingForInputReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryPlayerChoiceSend : UgpCategorySend
    {
        /// <summary>
        /// The index of the selected player choice.
        /// </summary>
        public uint PlayerChoiceIndex { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryPlayerChoiceReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryDenomSelectionActiveSend : UgpCategorySend
    {
        /// <summary>
        /// Indicates that the game selection menu is active.
        /// </summary>
        public bool Active { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpGameEventCategoryDenomSelectionActiveReply : UgpCategoryReply
    {
    }
}