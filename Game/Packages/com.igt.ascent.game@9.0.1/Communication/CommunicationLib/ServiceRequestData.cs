//-----------------------------------------------------------------------
// <copyright file = "ServiceRequestData.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// The ServiceRequestData type is a dictionary of String keys to a list of ServiceAccessor values.
    /// The key is the target service provider.
    /// </summary>
    [Serializable]
    public class ServiceRequestData : Dictionary<string, List<ServiceAccessor>>, ISerializable, ICompactSerializable
    {
        /// <summary>
        /// Create an instance of the <see cref="ServiceRequestData"/> class.
        /// </summary>
        public ServiceRequestData() : base() { }

        /// <summary>
        /// Serialization constructor, deserialize ServiceRequestData.
        /// </summary>
        /// <param name="info">Serialization info to construct the object from.</param>
        /// <param name="context">Serialization context to use.</param>
        /// <DevDoc>
        /// Mono and Microsoft's implementation of binary dictionary serialization is not
        /// compatible. Therefore if a type implements dictionary and it needs to be
        /// serialized and sent between the different .net implementations then the
        /// serialization routines must be overridden.
        /// </DevDoc>
        protected ServiceRequestData(SerializationInfo info, StreamingContext context)
        {
            // Retrieve lists used to store dictionary.
            var keyList = (List<string>)info.GetValue("KeyList", typeof(List<string>));
            var valueList = (List<List<ServiceAccessor>>)info.GetValue("ValueList", typeof(List<List<ServiceAccessor>>));
            
            // Populate dictionary from lists.
            for (int i = 0; i < keyList.Count && i < valueList.Count; i++)
            {
                Add(keyList[i], valueList[i]);
            }
        }

        /// <summary>
        /// ISerializable implementation of GetObjectData, serialize ServiceRequestData.
        /// </summary>
        /// <param name="info">Serialization info to construct the object from.</param>
        /// <param name="context">Serialization context to use.</param>
        /// <DevDoc>
        /// Mono and Microsoft's implementation of binary dictionary serialization is not
        /// compatible. Therefore if a type implements dictionary and it needs to be
        /// serialized and sent between the different .net implementations then the
        /// serialization routines must be overridden.
        /// </DevDoc>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Create Lists to store Dictionary Data.
            var keyList = new List<string>();
            var valueList = new List<List<ServiceAccessor>>();

            foreach (var pair in this)
            {
                keyList.Add(pair.Key);
                valueList.Add(pair.Value);
            }

            // Add created lists to serialization info.
            info.AddValue("KeyList", keyList);
            info.AddValue("ValueList", valueList);
        }

        /// <summary>
        /// Provides a string representation of the contents of this object.
        /// </summary>
        /// <returns>A string representing the contents of this object.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            foreach(var item in this)
            {
                sb.AppendFormat("Key: {0}\n", item.Key);
                foreach(var serviceAccessor in item.Value)
                {
                    sb.AppendFormat(
                        "\tService Accessor ID: {0}, Service: {1}, Notification Type: {2}\n",
                        serviceAccessor.Identifier,
                        serviceAccessor.Service,
                        serviceAccessor.NotificationType);
                }
            }
            return sb.ToString();
        }

        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        /// <remarks>
        /// Serialization is somewhat manual at this time because of limitations in the CompactSerializer's ability
        /// to handle null values in lists. If the functionality of the CompactSerializer is extended, then this
        /// serialization can be reduced.
        /// </remarks>
        public void Serialize(System.IO.Stream stream)
        {
            // Create Lists to store Dictionary Data.
            var keyList = new List<string>();
            var valueList = new List<List<ServiceAccessor>>();

            foreach (var pair in this)
            {
                keyList.Add(pair.Key);
                valueList.Add(pair.Value);
            }

            CompactSerializer.Serialize(stream, keyList);
            foreach(var value in valueList)
            {
                CompactSerializer.Write(stream, value != null);
                if(value != null)
                {
                    CompactSerializer.Write(stream, value.Count);
                    foreach(var item in value)
                    {
                        CompactSerializer.Write(stream, item != null);
                        if(item != null)
                        {
                            CompactSerializer.Write(stream, item);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void Deserialize(System.IO.Stream stream)
        {
            // Retrieve lists used to store dictionary.
            var keyList = CompactSerializer.Deserialize<List<string>>(stream);
            
            // Populate dictionary from lists.
            foreach(var key in keyList)
            {
                var isNotNull = CompactSerializer.ReadBool(stream);
                if(isNotNull)
                {
                    var newList = new List<ServiceAccessor>();
                    var count = CompactSerializer.ReadInt(stream);

                    for(var valueIndex = 0; valueIndex < count; valueIndex++)
                    {
                        var valueNotNull = CompactSerializer.ReadBool(stream);
                        newList.Add(valueNotNull ? CompactSerializer.Deserialize<ServiceAccessor>(stream) : null);
                    }
                    Add(key, newList);
                }
                else
                {
                    Add(key, null);
                }
            }
        }

        #endregion
    }
}
