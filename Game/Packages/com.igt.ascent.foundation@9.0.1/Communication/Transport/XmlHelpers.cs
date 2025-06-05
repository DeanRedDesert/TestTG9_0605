//-----------------------------------------------------------------------
// <copyright file = "XmlHelpers.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// Utility class for serializing XML objects to a string.
    /// </summary>
    public static class XmlHelpers
    {
        /// <summary>
        /// Serialize the given object with the given xml serializer.
        /// </summary>
        /// <typeparam name="TXmlObject">Object type to serialize.</typeparam>
        /// <param name="xmlObject">Object to serialize.</param>
        /// <param name="serializer">Serializer to use for serialization.</param>
        /// <returns>The given object serialized into a string.</returns>
        public static string GetXmlString<TXmlObject>(TXmlObject xmlObject, XmlSerializer serializer)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, xmlObject);
                return writer.ToString();
            }
        }
    }
}