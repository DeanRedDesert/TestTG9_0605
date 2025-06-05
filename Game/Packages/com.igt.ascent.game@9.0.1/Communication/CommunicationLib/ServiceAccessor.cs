//-----------------------------------------------------------------------
// <copyright file = "ServiceAccessor.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Linq;
    using System.Security.Permissions;
    using CompactSerialization;

    /// <summary>
    /// The ServiceAccessor is used to find a specific data item within the Game Logic.
    /// </summary>
    [Serializable]
    public class ServiceAccessor: ISerializable, ICompactSerializable, IEquatable<ServiceAccessor>
    {
        #region Constructors

        /// <summary>
        /// Constructor required for certain types of serialization. Public for compact serializer.
        /// </summary>
        public ServiceAccessor(){}

        /// <summary>
        /// Initialize an instance of ServiceAccessor.
        /// </summary>
        /// <param name="serviceAttribute">Name of the data in the service provider.</param>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public ServiceAccessor CreateServiceAccessor(string serviceName)
        /// {
        ///     var serviceAccessor = new ServiceAccessor (serviceName);
        ///     return serviceAccessor;
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public ServiceAccessor(string serviceAttribute)
            : this(serviceAttribute, null, NotificationType.Disabled) {}

        /// <summary>
        /// Initialize an instance of ServiceAccessor.
        /// </summary>
        /// <param name="serviceAttribute">Name of the data in the service provider.</param>
        /// <param name="serviceArguments">
        /// Optional arguments to be passed to the service attribute.
        /// The objects in the serviceArguments dictionary must overload the getHashCode function to 
        /// represent the contents of the file to generate a reliable Identifier.
        /// </param>
        /// <example>
        /// <code><![CDATA[
        /// public ServiceAccessor CreateServiceAccessor(string serviceName, Dictionary<string, object> serviceArguments)
        /// {
        ///     var serviceAccessor = new ServiceAccessor (serviceName, serviceArguments);
        ///     return serviceAccessor;
        /// }
        /// ]]></code>
        /// </example>
        public ServiceAccessor(string serviceAttribute, Dictionary<string, object> serviceArguments)
            : this(serviceAttribute, serviceArguments, NotificationType.Disabled) { }

        /// <summary>
        /// Initialize an instance of ServiceAccessor.
        /// </summary>
        /// <param name="serviceAttribute">Name of the data in the service provider.</param>
        /// <param name="notificationType">Type of notification requested </param>
        /// <example>
        /// <code><![CDATA[
        /// public ServiceAccessor CreateServiceAccessor(string serviceName, NotificationType notificationType)
        /// {
        ///     var serviceAccessor = new ServiceAccessor (serviceName, notificationType);
        ///     return serviceAccessor;
        /// }
        /// ]]></code>
        /// </example>
        public ServiceAccessor(string serviceAttribute, NotificationType notificationType)
            : this(serviceAttribute, null, notificationType) { }

        /// <summary>
        /// Initialize an instance of ServiceAccessor.
        /// </summary>
        /// <param name="serviceAttribute">Name of the data in the service provider.</param>
        /// <param name="serviceArguments">
        /// Optional arguments to be passed to the service attribute.
        /// The objects in the serviceArguments dictionary must overload the getHashCode function to 
        /// generate a reliable Identifier.
        /// </param>
        /// <param name="notificationType">Type of notification requested </param>
        /// <example>
        /// <code><![CDATA[
        /// public ServiceAccessor CreateServiceAccessor(string serviceName, Dictionary<string, object> serviceArguments, NotificationType notificationType)
        /// {
        ///     var serviceAccessor = new ServiceAccessor (serviceName, serviceArguments, notificationType);
        ///     return serviceAccessor;
        /// }
        /// ]]></code>
        /// </example>
        public ServiceAccessor(string serviceAttribute, Dictionary<string, object> serviceArguments, NotificationType notificationType)
        {
            InitializeServiceLocator(serviceAttribute, serviceArguments, notificationType);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Identifier that is used to locate a specific data item within the data source.
        /// </summary>
        public string Service
        {
            get { return service; }
        }

        /// <summary>
        /// Data item list that specifies arguments used for this service.
        /// </summary>
        /// <devdoc>This dictionary is not supposed to be changed after constructed.</devdoc>
        public Dictionary<string, object> ServiceArguments
        {
            get { return serviceArguments; }
        }

        /// <summary>
        /// Type of notification required for this data.
        /// </summary>
        public NotificationType NotificationType
        {
            get { return notificationType; }
        }

        /// <summary>
        /// Determine Identifier of the ServiceAccessor
        /// </summary>
        /// <devdoc>If the arguments list changed at runtime, we might have conflict identifiers.</devdoc>
        public int Identifier
        {
            get { return hashIdentifier; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize members of this service locator.
        /// </summary>
        /// <param name="serviceAttribute">Name of the data in the service provider.</param>
        /// <param name="newServiceArguments">Optional arguments to be passed to the service attribute.</param>
        /// <param name="newNotificationType">Type of notification requested </param>
        /// <exception cref="ArgumentException">Thrown when serviceAttribure is null or empty string.</exception>
        private void InitializeServiceLocator(string serviceAttribute, Dictionary<string, object> newServiceArguments,
            NotificationType newNotificationType)
        {
            if (string.IsNullOrEmpty(serviceAttribute))
            {
                throw new ArgumentException("Cannot be null or empty string", "serviceAttribute");
            }
            service = serviceAttribute;
            serviceArguments = newServiceArguments;
            notificationType = newNotificationType;
            RefreshIdentifier();
        }

        /// <summary>
        /// Refresh the identifier when the internal state is changed.
        /// </summary>
        private void RefreshIdentifier()
        {
            unchecked
            {
                hashIdentifier = service.GetHashCode();
                if(serviceArguments != null)
                {
                    foreach(var arg in serviceArguments)
                    {
                        hashIdentifier = (hashIdentifier * 23) ^ arg.Key.GetHashCode();
                        hashIdentifier = (hashIdentifier * 23) ^ (arg.Value == null ? 0 : arg.Value.GetHashCode());
                    }
                }

                if(notificationType.IsHistory())
                {
                    hashIdentifier = (hashIdentifier * 23) ^ HashHistoryMode;
                }
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// The service name.
        /// </summary>
        private string service;

        /// <summary>
        /// The argument list to retrieve the service data.
        /// </summary>
        private Dictionary<string, object> serviceArguments;

        /// <summary>
        /// The notification type to update the consumer data.
        /// </summary>
        private NotificationType notificationType;

        /// <summary>
        /// The hash identifier to identify this service accessor instance.
        /// </summary>
        private int hashIdentifier;

        /// <summary>
        /// The hashcode of string "HistoryMode".
        /// </summary>
        private static readonly int HashHistoryMode = "HistoryMode".GetHashCode();

        #endregion

        #region ISerializable

        /// <summary>
        /// Serialization constructor, deserialize ServiceAccessor.
        /// </summary>
        /// <param name="info">Serialization info to construct the object from.</param>
        /// <param name="context">Serialization context to use.</param>
        /// <DevDoc>
        /// Mono and Microsoft's implementation of binary dictionary serialization is not
        /// compatible. Therefore if a type implements dictionary and it needs to be
        /// serialized and sent between the different .net implementations then the
        /// serialization routines must be overridden.
        /// </DevDoc>
        protected ServiceAccessor(SerializationInfo info, StreamingContext context)
        {
            // Retrieve lists used to store ServiceArguments Dictionary
            var keyList = (List<string>)info.GetValue("KeyList", typeof(List<string>));
            var valueList = (List<object>)info.GetValue("ValueList", typeof(List<object>));

            if (keyList == null || valueList == null)
            {
                serviceArguments = null;
            }
            else
            {
                serviceArguments = new Dictionary<string, object>();
                for (int dictCount = 0; dictCount < keyList.Count && dictCount < valueList.Count; dictCount++)
                {
                    serviceArguments.Add(keyList[dictCount], valueList[dictCount]);
                }
            }

            // Populate non-dictionary data members.
            service = (string)info.GetValue("Service", typeof(string));
            notificationType = (NotificationType)info.GetValue("NotificationType", typeof(NotificationType));
            hashIdentifier = (int)info.GetValue("Identifier", typeof(int));
        }

        /// <summary>
        /// ISerializable implementation of GetObjectData, serialize ServiceAccessor.
        /// </summary>
        /// <param name="info">Serialization info to construct the object from.</param>
        /// <param name="context">Serialization context to use.</param>
        /// <DevDoc>
        /// Mono and Microsoft's implementation of binary dictionary serialization is not
        /// compatible. Therefore if a type implements dictionary and it needs to be
        /// serialized and sent between the different .net implementations then the
        /// serialization routines must be overridden.
        /// </DevDoc>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Create Lists to store Dictionary Data.
            List<string> keyList = null;
            List<object> valueList = null;

            if (serviceArguments != null)
            {
                keyList = new List<string>();
                valueList = new List<object>();

                // Populate Lists.
                foreach (var pair in serviceArguments)
                {
                    keyList.Add(pair.Key);
                    valueList.Add(pair.Value);
                }
            }

            // Add created lists to serialization info.
            info.AddValue("KeyList", keyList);
            info.AddValue("ValueList", valueList);

            // Add non-dictionary data members that need to be serialized.
            info.AddValue("Service", Service);
            info.AddValue("NotificationType", NotificationType);
            info.AddValue("Identifier", Identifier);
        }

        #endregion

        /// <summary>
        /// Display contents of object as string.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            string stringValue = "\nService: " + Service + "\nNotificationType: " + NotificationType;
            //Add service arguments
            if (serviceArguments != null)
            {
                stringValue = serviceArguments.Aggregate(stringValue,
                                                         (current, serviceArgument) =>
                                                         current +
                                                         ("\n" + serviceArgument.Key + ": " + serviceArgument.Value));
            }
            stringValue += "\nIdentifier: " + Identifier;

            return base.ToString() + stringValue;
        }

        #region ICompactSerializable Implementation

        /// <summary>
        /// This enum type lists the service argument types that are supported by compact serialization.
        /// </summary>
        /// <remarks>
        /// Using byte based enum type to minimize the footprint of the serialized data.
        /// </remarks>
        [Serializable]
        private enum ArgumentTypeQualifier : byte
        {
            /// <summary>
            /// The string type.
            /// </summary>
            StringType,

            /// <summary>
            /// The integer type.
            /// </summary>
            IntegerType,

            /// <summary>
            /// The float type.
            /// </summary>
            FloatType,

            /// <summary>
            /// The bool type.
            /// </summary>
            BoolType,
        }

        /// <inheritdoc/>
        /// <remarks>
        /// For now compact serialization of service accessors only supports integers, strings and floats.
        /// </remarks>
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, serviceArguments != null);
            if(serviceArguments != null)
            {
                // Create Lists to store Dictionary Data.
                var keyList = new List<string>();
                var valueList = new List<object>();

                // Populate Lists.
                foreach(var pair in serviceArguments)
                {
                    keyList.Add(pair.Key);
                    valueList.Add(pair.Value);
                }

                CompactSerializer.Serialize(stream, keyList);

                foreach(var value in valueList)
                {
                    if(value is string)
                    {
                        CompactSerializer.Serialize(stream, (byte)ArgumentTypeQualifier.StringType);
                        CompactSerializer.Serialize(stream, value);
                    }
                    else if(value is int)
                    {
                        CompactSerializer.Serialize(stream, (byte)ArgumentTypeQualifier.IntegerType);
                        CompactSerializer.Serialize(stream, value);
                    }
                    else if(value is float)
                    {
                        CompactSerializer.Serialize(stream, (byte)ArgumentTypeQualifier.FloatType);
                        CompactSerializer.Serialize(stream, value);
                    }
                    else if (value is bool)
                    {
                        CompactSerializer.Serialize(stream, (byte)ArgumentTypeQualifier.BoolType);
                        CompactSerializer.Serialize(stream, value);
                    }
                    else
                    {
                        throw new Exception("Unsupported Type: " + value.GetType());
                    }
                    
                }
            }

            CompactSerializer.Serialize(stream, Service);
            CompactSerializer.Serialize(stream, (int)NotificationType);
        }

        /// <inheritdoc/>
        public void Deserialize(System.IO.Stream stream)
        {
            var parametersNotNull = CompactSerializer.ReadBool(stream);
            if(parametersNotNull)
            {
                // Retrieve lists used to store serviceArguments Dictionary
                var keyList = CompactSerializer.Deserialize<List<string>>(stream);

                var keyCount = keyList.Count;
                var valueList = new List<object>(keyCount);
                for(var keyIndex = 0; keyIndex < keyCount; keyIndex++)
                {
                    var type = (ArgumentTypeQualifier)CompactSerializer.Deserialize<byte>(stream);
                    switch(type)
                    {
                        case ArgumentTypeQualifier.StringType:
                            valueList.Add(CompactSerializer.Deserialize<string>(stream));
                            break;
                        case ArgumentTypeQualifier.IntegerType:
                            valueList.Add(CompactSerializer.Deserialize<int>(stream));
                            break;
                        case ArgumentTypeQualifier.FloatType:
                            valueList.Add(CompactSerializer.Deserialize<float>(stream));
                            break;
                        case ArgumentTypeQualifier.BoolType:
                            valueList.Add(CompactSerializer.Deserialize<bool>(stream));
                            break;
                        default:
                            throw new SerializationException(
                                "Type not supported for compact serialized ServiceAccessor: " + type);
                    }
                }

                serviceArguments = new Dictionary<string, object>(keyCount);
                for(var dictCount = 0; dictCount < keyList.Count && dictCount < valueList.Count; dictCount++)
                {
                    serviceArguments.Add(keyList[dictCount], valueList[dictCount]);
                }
            }

            // Populate non-dictionary data members.
            service = CompactSerializer.Deserialize<string>(stream);

            notificationType = (NotificationType)CompactSerializer.Deserialize<int>(stream);
            RefreshIdentifier();
        }

        #endregion

        #region Equality members

        /// <summary>
        /// The argument comparer shared crossing instances.
        /// </summary>
        private static readonly ArgumentComparer ArgumentEqualityComparer = new ArgumentComparer();

        /// <inheritdoc/>
        public bool Equals(ServiceAccessor other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }

            if(!ReferenceEquals(serviceArguments, other.serviceArguments))
            {
                if(serviceArguments == null ||
                   serviceArguments.Count != other.serviceArguments.Count)
                {
                    return false;
                }

                if(!serviceArguments.SequenceEqual(other.serviceArguments, ArgumentEqualityComparer))
                {
                    return false;
                }

            }
            return service == other.service && notificationType == other.notificationType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ServiceAccessor);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Identifier;
        }

        /// <summary>
        /// The custom comparer for <see cref="KeyValuePair{TKey, TValue}"/> to avoid boxing.
        /// </summary>
        private class ArgumentComparer : EqualityComparer<KeyValuePair<string, object>>
        {
            #region Overrides of EqualityComparer<KeyValuePair<string,object>>

            /// <inheritdoc/>
            public override bool Equals(KeyValuePair<string, object> x, KeyValuePair<string, object> y)
            {
                return x.Equals(y);
            }

            /// <inheritdoc/>
            public override int GetHashCode(KeyValuePair<string, object> obj)
            {
                var hash = obj.Key == null ? 0 : obj.Key.GetHashCode();
                if(obj.Value != null)
                {
                    hash = hash * 23 ^ obj.Value.GetHashCode();
                }
                return hash;
            }

            #endregion
        }

        #endregion
    }
}
