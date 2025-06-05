//-----------------------------------------------------------------------
// <copyright file = "UgpMessageStripCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
	using System.Collections.Generic;
	using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpMessageStripMessage
	{
		/// <remarks/>
		public string MessageText { get; set; }

		/// <remarks/>
		public bool IsPriority { get; set; }
	}

	/// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpMessageStripCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryInternal : UgpCategoryBase<UgpMessageStripCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("AddMessageSend", typeof(UgpMessageStripCategoryAddMessageSend))]
        [XmlElement("AddMessageReply", typeof(UgpMessageStripCategoryAddMessageReply))]
        [XmlElement("RemoveMessageSend", typeof(UgpMessageStripCategoryRemoveMessageSend))]
        [XmlElement("RemoveMessageReply", typeof(UgpMessageStripCategoryRemoveMessageReply))]
		[XmlElement("GetMessagesSend", typeof(UgpMessageStripCategoryGetMessagesSend))]
		[XmlElement("GetMessagesReply", typeof(UgpMessageStripCategoryGetMessagesReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryAddMessageSend : UgpCategorySend
    {
        /// <remarks/>
        public string MessageText { get; set; }

        /// <remarks/>
        public bool IsPriority { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryAddMessageReply : UgpCategoryReply
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryRemoveMessageSend : UgpCategorySend
    {
        /// <remarks/>
        public string MessageText { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpMessageStripCategoryRemoveMessageReply : UgpCategoryReply
    {
    }
	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpMessageStripCategoryGetMessagesSend : UgpCategorySend
	{

	}

	/// <remarks/>
	[Serializable]
	[XmlType(AnonymousType = true)]
	[DisableCodeCoverageInspection]
	public class UgpMessageStripCategoryGetMessagesReply : UgpCategoryReply
	{
		private List<UgpMessageStripMessage> messages = new List<UgpMessageStripMessage>();

		/// <remarks/>
		public List<UgpMessageStripMessage> Messages
		{
			get { return messages; }
		}
	}
}