// -----------------------------------------------------------------------
// <copyright file = "TracingXmlSerializer.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System.Xml.Serialization;
    using Transport;

    /// <summary>
    /// This class serializes objects into xml string and traces the serializations.
    /// </summary>
    internal static class TraceableXmlSerializer
    {
        /// <summary>
        /// Serialize the given object with the given xml serializer.
        /// </summary>
        /// <typeparam name="TXmlObject">Object type to serialize.</typeparam>
        /// <param name="category">Category of the message.</param>
        /// <param name="xmlObject">Object to serialize.</param>
        /// <param name="serializer">Serializer to use for serialization.</param>
        /// <returns>The given object serialized into a string.</returns>
        public static string GetXmlString<TXmlObject>(MessageCategory category, TXmlObject xmlObject,
            XmlSerializer serializer) where TXmlObject : ICategory
        {
            F2XSerializationTracing.Log.SerializeMessageStart(category);
            var xml = XmlHelpers.GetXmlString(xmlObject, serializer);
            F2XSerializationTracing.Log.SerializeMessageStop(category, xml.Length);
            return xml;
        }
    }
}