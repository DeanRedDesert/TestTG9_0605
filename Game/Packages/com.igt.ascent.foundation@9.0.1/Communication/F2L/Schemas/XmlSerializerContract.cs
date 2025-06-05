// -----------------------------------------------------------------------
// <copyright file = "XmlSerializerContract.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Serializers
{
    /// <summary>
    /// The singleton implementation of <see cref="XmlSerializerContract"/>.
    /// </summary>
    public partial class XmlSerializerContract
    {
        /// <summary>
        /// The back field of the singleton instance of this class.
        /// </summary>
        private static XmlSerializerContract contract;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static XmlSerializerContract Instance
        {
            get { return contract ?? (contract = new XmlSerializerContract()); }
        }
    }
}