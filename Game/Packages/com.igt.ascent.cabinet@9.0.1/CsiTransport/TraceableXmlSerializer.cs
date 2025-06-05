// -----------------------------------------------------------------------
// <copyright file = "TracingXmlSerializer.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System.Xml.Serialization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;

    /// <summary>
    /// This class serializes objects into xml string and traces the serializations.
    /// </summary>
    internal static class TraceableXmlSerializer
    {
        /// <summary>
        /// Serialize the given object with the given xml serializer.
        /// </summary>
        /// <typeparam name="TXmlObject">Object type to serialize.</typeparam>
        /// <param name="xmlObject">Object to serialize.</param>
        /// <param name="serializer">Serializer to use for serialization.</param>
        /// <returns>The given object serialized into a string.</returns>
        public static string GetXmlString<TXmlObject>(TXmlObject xmlObject, XmlSerializer serializer)
            where TXmlObject : Csi
        {
            CsiSerializationTracing.Log.SerializeCsiContainerStart(xmlObject.Category);
            var xml = XmlHelpers.GetXmlString(xmlObject, serializer);
            CsiSerializationTracing.Log.SerializeCsiContainerStop(xmlObject.Category, xml.Length);
            return xml;
        }
    }
}