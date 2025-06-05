//-----------------------------------------------------------------------
// <copyright file = "UgpRandomNumberCategoryInternal.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using F2XTransport;
    using Ugp;

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "UgpRandomNumberCategory", IsNullable = false)]
    [DisableCodeCoverageInspection]
    public class UgpRandomNumberCategoryInternal : UgpCategoryBase<UgpRandomNumberCategoryMessage>
    {
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpRandomNumberCategoryMessage : ICategoryMessage
    {
        #region Implementation of ICategoryMessage

        /// <summary>
        /// The message for the category. All categories have a main type and that type then contains a choice item.
        /// Each different type the choice represents a different message for that category.
        /// </summary>
        [XmlElement("GetRandomNumbersSend", typeof(UgpRandomNumbersSend))]
        [XmlElement("GetRandomNumbersReply", typeof(UgpRandomNumbersReply))]
        public object Item { get; set; }

        #endregion
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpRandomNumbersSend : UgpCategorySend
    {
        /// <remarks/>
        public uint RequiredNumberOfRandomNumbers { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(AnonymousType = true)]
    [DisableCodeCoverageInspection]
    public class UgpRandomNumbersReply : UgpCategoryReply
    {
        /// <remarks/>
        public List<double> RandomNumbers { get; set; }
    }
}
