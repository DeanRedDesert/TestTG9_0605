//-----------------------------------------------------------------------
// <copyright file = "XmlElementSerializationSurrogate.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    /// <summary>
    /// Adds support for serializing and deserializing the XmlElement type.
    /// </summary>
    public sealed class XmlElementSerializationSurrogate : ISerializationSurrogate
    {
        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">
        /// Thrown if obj is not castable to XmlElement.
        /// </exception>
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var xmlObj = obj as XmlElement;

            if(xmlObj == null)
            {
                throw new ArgumentException("obj must be castable to XmlElement.");
            }

            info.AddValue("OuterXml", xmlObj.OuterXml);
        }

        /// <inheritdoc />
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var xmlData = info.GetString("OuterXml");

            var doc = new XmlDocument();
            doc.LoadXml(xmlData);
            var xmlObj = doc.DocumentElement;

            return xmlObj;
        }
    }
}
