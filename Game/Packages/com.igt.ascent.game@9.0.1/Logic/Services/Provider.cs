//-----------------------------------------------------------------------
// <copyright file = "Provider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///    Holds Service Provider information.
    /// </summary>
    public class Provider
    {
        #region Service Info cache

        /// <summary>
        /// A set of flags representing the different properties that a game service can have.
        /// </summary>
        /// <remarks>
        /// This is provided as a relatively intuitive way to manipulate the bit flags since the "BitVector32" type
        /// seems to be missing from Unity's version of Mono.
        /// </remarks>
        [Flags]
        private enum ServiceProperties : byte
        {
            /// <summary>
            /// No properties are defined.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            None = 0x00,
            
            /// <summary>
            /// The service is writable.
            /// </summary>
            Writable = 0x01,
            
            /// <summary>
            /// The service is asynchronous.
            /// </summary>
            Asynchronous = 0x02
        }
        
        /// <summary>
        /// An immutable data type which compactly stores information about a game service.
        /// </summary>
        private struct ServiceInfo
        {
            private readonly ServiceProperties flags;

            /// <summary>
            /// Gets a flag which indicates whether or not a service is writable.
            /// </summary>
            internal bool IsServiceWritable => (flags & ServiceProperties.Writable) == ServiceProperties.Writable;

            /// <summary>
            /// Gets a flag which indicates whether or not a service is asynchronous.
            /// </summary>
            internal bool IsAsynchronous => (flags & ServiceProperties.Asynchronous) == ServiceProperties.Asynchronous;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceProperties"/> struct with the given properties.
            /// </summary>
            /// <param name="isWritable"><b>true</b> if the service is writable.</param>
            /// <param name="isAsynchronous"><b>true</b> if the service is asynchronous.</param>
            internal ServiceInfo(bool isWritable, bool isAsynchronous) : this()
            {
                if(isWritable)
                {
                    flags |= ServiceProperties.Writable;
                }
                if(isAsynchronous)
                {
                    flags |= ServiceProperties.Asynchronous;
                }
            }
        }

        /// <summary>
        /// The cached information for the services managed by this provider.
        /// </summary>
        private readonly Dictionary<string, ServiceInfo> serviceInfo = new Dictionary<string,ServiceInfo>();

        #endregion

        #region Properties

        /// <summary>
        ///    Get the provider object for this ServiceProvider
        /// </summary>
        public object ProviderObject { get; private set; }

        /// <summary>
        ///    Get the providers accessible member information.
        /// </summary>
        public IDictionary<string, MemberInfo> Services => services;

        #endregion

        #region Public Methods

        #region Constructors

        /// <summary>
        ///    Initialize an instance of Provider with a ServiceProvider.
        /// </summary>
        /// <param name = "provider">ServiceProvider Object</param>
        public Provider(object provider)
        {
            ProviderObject = provider;
            FindGameServiceMembers(provider);
        }

        ///<summary>
        /// Make a copy of a provider
        ///</summary>
        ///<param name="copy">provider to copy</param>
        ///<exception cref="ArgumentNullException">Thrown when a null copy is passed.</exception>
        public Provider(Provider copy)
        {
            if (copy == null)
            {
                throw new ArgumentNullException(nameof(copy));
            }
            services = new Dictionary<string, MemberInfo>();
            ProviderObject = copy.ProviderObject;
            foreach (var service in copy.Services)
            {
                services.Add(service.Key, service.Value);
            }
        }

        #endregion

        /// <summary>
        ///    Get the value of a member of the provider.
        /// </summary>
        /// <remarks>
        ///    Overloaded Methods are not allowed.
        /// </remarks>
        /// <param name = "serviceName">Name of the member to get a value for.</param>
        /// <param name = "parameters">Optional parameters that will be passed to the member.</param>
        /// <returns>Object represented by the member.</returns>
        public object GetServiceValue(string serviceName, Dictionary<string, object> parameters)
        {
            var info = GetMemberInformation(serviceName);

            return GetValue(info, parameters);
        }

        /// <summary>
        ///    Asynchronously get the value of a member of the provider.
        /// </summary>
        /// <remarks>
        ///    Overloaded Methods are not allowed.
        /// </remarks>
        /// <param name = "serviceName">Name of the member to get a value for.</param>
        /// <param name = "parameters">Optional parameters that will be passed to the member.</param>
        /// <returns>Object represented by the member.</returns>
        /// <exception cref = "InvalidServiceAttributeException">
        ///    Thrown when the passed member is not defined for asynchronous access.
        /// </exception>
        /// <exception cref="UnknownProviderServiceException">
        /// Thrown if <paramref name="serviceName"/> is not a registered game service.
        /// </exception>
        public object GetAsynchronousServiceValue(string serviceName, Dictionary<string, object> parameters)
        {
            // Determine if this member can be accessed asynchronously.
            bool isAsynchronous;
            if(serviceInfo.ContainsKey(serviceName))
            {
                isAsynchronous = serviceInfo[serviceName].IsAsynchronous;
            }
            else
            {
                const string message = "Service could not be found as part of Service Provider.";
                throw new UnknownProviderServiceException(ProviderObject.GetType().Name, serviceName, message);
            }
            if (!isAsynchronous)
            {
                throw new InvalidServiceAttributeException(ProviderObject.GetType().Name, serviceName,
                    "Service provider member is not defined for asynchronous access.");
            }
            var info = GetMemberInformation(serviceName);
            return GetValue(info, parameters);
        }

        /// <summary>
        ///    Set the value of a member in this provider with a single object value
        /// </summary>
        /// <remarks>
        ///    This is not for use with methods.
        /// </remarks>
        /// <DevDoc>
        ///    This creates a parameters list with the key "value" for setting properties and fields,
        ///    this may not work for methods. It will work for methods if the method takes a single parameter
        ///    called "value".
        /// </DevDoc>
        /// <param name = "serviceName">Name of the member to set a value for.</param>
        /// <param name = "value">New value for the member.</param>
        public void SetServiceValue(string serviceName, object value)
        {
            var parameters = new Dictionary<string, object>
            {
                {"value", value}
            };

            SetServiceValue(serviceName, parameters);
        }

        /// <summary>
        ///    Set the value of a member in this provider.
        /// </summary>
        /// <param name = "serviceName">Name of the member to set a value for.</param>
        /// <param name = "parameters">Parameters to pass into the service provider member.</param>
        /// <exception cref = "ReadOnlyProviderException">
        ///    Thrown when a GameService member is not writable.
        /// </exception>
        /// <exception cref = "InvalidProviderParametersException">
        ///    Thrown when the new value does not match the type expected.
        /// </exception>
        /// <exception cref = "UnknownProviderServiceTypeException">
        ///    Thrown when type of a member is not supported by this method.
        /// </exception>
        public void SetServiceValue(string serviceName, Dictionary<string, object> parameters)
        {
            var member = GetMemberInformation(serviceName);

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");
            }

            // Need to determine if this member is tagged with the writable attribute.
            if (!IsServiceWritable(member))
            {
                var message = member.MemberType +
                    "cannot be written to because it is not defined as Writable Game Service Attribute.";
                throw new ReadOnlyProviderException(ProviderObject.GetType().Name, serviceName, message);
            }

            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        object value;

                        if (parameters.ContainsKey("value"))
                        {
                            value = parameters["value"];
                        }
                        else
                        {
                            throw new InvalidProviderParametersException(
                                ProviderObject.GetType().Name, serviceName,
                                "Parameter list must contain a parameter named \'value\' for properties.");
                        }

                        // Get the property information from the provider
                        var info = ProviderObject.GetType().GetProperty(serviceName);

                        try
                        {
                            // TODO: possible null reference not handled
                            // ReSharper disable once PossibleNullReferenceException
                            info.SetValue(ProviderObject, value, null);
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidProviderParametersException(ProviderObject.GetType().Name,
                                serviceName, e.Message, e);
                        }
                    }
                    break;

                case MemberTypes.Field:
                    {
                        // Get the field information from the provider
                        var info = ProviderObject.GetType().GetField(serviceName);

                        object value;

                        if (parameters.ContainsKey("value"))
                        {
                            value = parameters["value"];
                        }
                        else
                        {
                            throw new InvalidProviderParametersException(
                                ProviderObject.GetType().Name, serviceName,
                                "Parameter list must contain a parameter named \'value\' for fields.");
                        }

                        try
                        {
                            info.SetValue(ProviderObject, value);
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidProviderParametersException(ProviderObject.GetType().Name,
                                serviceName, e.Message, e);
                        }
                    }
                    break;

                case MemberTypes.Method:
                    {
                        var info = ProviderObject.GetType().GetMethod(serviceName);

                        InvokeMethodMember(info, parameters);
                    }
                    break;

                default:
                    throw new UnknownProviderServiceTypeException(ProviderObject.GetType().Name,
                        serviceName, member.MemberType, "This member type is not supported by Set Member Value.");

            }
        }

        /// <summary>
        ///    Determine is a specific member of the provider is defined as an
        ///    <see cref = "AsynchronousGameServiceAttribute" />.
        /// </summary>
        /// <param name = "serviceName">Name of the service provider's member to look up.</param>
        /// <returns>
        ///    Flag indicating if the provider member is an <see cref = "AsynchronousGameServiceAttribute" />.
        /// </returns>
        /// <exception cref="UnknownProviderServiceException">
        /// Thrown if this provider does not contain a service named <paramref name="serviceName"/>.
        /// </exception>
        public bool IsServiceAsynchronous(string serviceName)
        {
            bool isAsynchronous;
            if(serviceInfo.ContainsKey(serviceName))
            {
                isAsynchronous = serviceInfo[serviceName].IsAsynchronous;
            }
            else
            {
                const string message = "Service could not be found as part of Service Provider.";
                throw new UnknownProviderServiceException(ProviderObject.GetType().Name, serviceName, message);
            }
            return isAsynchronous;
        }

        /// <summary>
        /// Updates the object that holds the data for this provider.
        /// </summary>
        /// <param name="newProviderObject">The new provider object to use.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="newProviderObject"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the current provider object is a different type than <paramref name="newProviderObject"/>.
        /// </exception>
        public void UpdateProviderObject(object newProviderObject)
        {
            if(newProviderObject == null)
            {
                throw new ArgumentNullException(nameof(newProviderObject));
            }
            if(ProviderObject.GetType() != newProviderObject.GetType())
            {
                throw new InvalidOperationException("Cannot update provider with different type.");
            }
            ProviderObject = newProviderObject;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///    Find all of the providers members that are tagged with GameServiceAttribute,
        ///    and all members tagged with AsynchronousGameServiceAttribute.
        /// </summary>
        /// <param name = "serviceProvider">Service provider should contain game service members.</param>
        /// <exception cref = "InvalidServiceAttributeException">
        ///    Thrown when a read only property is tagged with the WritableGameServiceAttribute.
        /// </exception>
        private void FindGameServiceMembers(object serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider), "Provider object cannot be null.");
            }

            services = new Dictionary<string, MemberInfo>();
            var providerType = serviceProvider.GetType();

            // Find all of the public members in the passed provider
            foreach (var info in providerType.GetMembers())
            {
                var attributes = info.GetCustomAttributes(false);
                // Find the attributes for the current public member
                foreach (var attribute in attributes)
                {
                    // If the attribute is a GameServiceAttribute or child of GameServiceAttribute
                    // then add this member to the list.
                    if (attribute is GameServiceAttribute)
                    {
                        var isWritableAttribute = attribute is WritableGameServiceAttribute;

                        switch(info.MemberType)
                        {
                            case MemberTypes.Property:
                            {
                                var propertyInfo = (PropertyInfo)info;
                                if (isWritableAttribute && !propertyInfo.CanWrite)
                                {
                                    const string message = "Read only property is tagged with Writable Game Service Attribute.";
                                    throw new InvalidServiceAttributeException(providerType.Name, info.Name, message);
                                }

                                break;
                            }

                            case MemberTypes.Method:
                            {
                                var methodInfo = (MethodInfo)info;
                                if (isWritableAttribute && methodInfo.ReturnType != typeof(void))
                                {
                                    throw new InvalidProviderMethodReturnTypeException(providerType.Name,
                                        info.Name, "Writeable methods cannot return anything.");
                                }
                            
                                if (!isWritableAttribute && methodInfo.ReturnType == typeof(void))
                                {
                                    throw new InvalidProviderMethodReturnTypeException(providerType.Name,
                                        info.Name, "Read only methods must return a value.");
                                }

                                break;
                            }

                            case MemberTypes.Field:
                            {
                                var fieldInfo = (FieldInfo)info;
                                if (isWritableAttribute && (fieldInfo.IsInitOnly || fieldInfo.IsLiteral))
                                {
                                    throw new InvalidServiceAttributeException(providerType.Name,
                                        info.Name, "Read only field is tagged with Writable Game Service Attribute.");
                                }

                                break;
                            }
                        }

                        services.Add(info.Name, info);
                        serviceInfo.Add(
                            info.Name,
                            new ServiceInfo(isWritableAttribute, attribute is AsynchronousGameServiceAttribute));
                    }
                }
            }
        }

        /// <summary>
        ///    Get the member information object for the passed member name.
        /// </summary>
        /// <param name = "serviceName">Name of the member to find</param>
        /// <returns>Member information object for the requested member.</returns>
        /// <exception cref = "UnknownProviderServiceException">
        ///    Thrown when the passed memberName is not a GameService member of the ServiceProvider object.
        /// </exception>
        private MemberInfo GetMemberInformation(string serviceName)
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName), "Service Name cannot be null.");
            }

            MemberInfo member;
            try
            {
                member = services[serviceName];
            }
            catch (KeyNotFoundException e)
            {
                const string message = "Service could not be found as part of Service Provider.";
                throw new UnknownProviderServiceException(ProviderObject.GetType().Name, serviceName, message, e);
            }

            return member;
        }

        /// <summary>
        ///    Get the value of the specified member
        /// </summary>
        /// <remarks>
        ///    Overloaded Methods are not allowed.
        /// </remarks>
        /// <param name = "member">Name of the member to get a value for.</param>
        /// <param name = "parameters">Optional parameters that will be passed to the member.</param>
        /// <returns>Object represented by the member.</returns>
        /// <exception cref = "InvalidServiceAttributeException">
        ///    Thrown if the method is tagged with WritableGameServiceAttribute, meaning that it is
        ///    write only.
        /// </exception>
        private object GetValue(MemberInfo member, Dictionary<string, object> parameters)
        {
            object memberValue = null;

            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        var propertyInfo = (PropertyInfo)member;
                        if (propertyInfo.CanRead)
                        {
                            memberValue = propertyInfo.GetValue(ProviderObject, null);
                        }
                    }
                    break;

                case MemberTypes.Field:
                    {
                        var fieldInfo = (FieldInfo)member;
                        memberValue = fieldInfo.GetValue(ProviderObject);
                    }
                    break;

                case MemberTypes.Method:
                    {
                        // Determine if this method is read only.  If it is not read only
                        // throw an exception because this method cannot be used in this case.
                        if (IsServiceWritable(member))
                        {
                            throw new InvalidServiceAttributeException(ProviderObject.GetType().Name,
                                member.Name, "Method cannot be tagged with Writable Game Service Attribute to be called during read.");
                        }

                        memberValue = InvokeMethodMember((MethodInfo)member, parameters);
                    }
                    break;

                default:
                    throw new UnknownProviderServiceTypeException(ProviderObject.GetType().Name,
                        member.Name, member.MemberType, "This member type is not supported by Get Member Value.");

            }

            return memberValue;
        }

        /// <summary>
        ///    Invoke a member method with the passed parameters.
        /// </summary>
        /// <param name = "method">Information about the method to be invoked.</param>
        /// <param name = "parameters">Parameters that should be passed to the method.</param>
        /// <returns>Any value returned by the method.</returns>
        /// <exception cref = "InvalidProviderParametersException">
        ///    Thrown when the parameters passed do not match the signature of the method being invoked.
        /// </exception>
        private object InvokeMethodMember(MethodInfo method, Dictionary<string, object> parameters)
        {
            object methodValue;

            // Get the parameters required by this method
            var methodParameters = method.GetParameters();
            var invokeParameters = new object[methodParameters.Length];

            if (parameters == null && methodParameters.Length > 0)
            {
                var message = "Method parameter count (" + methodParameters.Length +
                    ") does not match passed parameter count (null).";
                throw new InvalidProviderParametersException(ProviderObject.GetType().Name, method.Name,
                    message);
            }
            if (parameters != null &&
                methodParameters.Length != parameters.Count)
            {
                var message = "Method parameter count (" + methodParameters.Length +
                    ") does not match passed parameter count (" + parameters.Count + ").";
                throw new InvalidProviderParametersException(ProviderObject.GetType().Name, method.Name,
                    message);
            }

            if (parameters != null && parameters.Count > 0)
            {
                // Put all of the parameters into an object array for the method call
                for (var paramIndex = 0; paramIndex < methodParameters.Length; paramIndex++)
                {
                    var parameter = methodParameters[paramIndex];
                    // If the parameter was passed in, add it to the object array
                    if (parameters.ContainsKey(parameter.Name))
                    {
                        invokeParameters[paramIndex] = parameters[parameter.Name];
                    }
                    // If the parameter was not passed in, then there is a problem.
                    else
                    {
                        var message = "Parameter \"" + parameter.Name + "\" for method was not passed.";
                        throw new InvalidProviderParametersException(ProviderObject.GetType().Name,
                            method.Name, message);
                    }
                }
            }
            try
            {
                // Call the method.
                methodValue = method.Invoke(ProviderObject, invokeParameters);
            }
            catch (ArgumentException e)
            {
                throw new InvalidProviderParametersException(ProviderObject.GetType().Name, method.Name,
                    e.Message, e);
            }

            return methodValue;

        }

        /// <summary>
        ///    Determine is the passed member is tagged with the WritableGameServiceAttribute.
        /// </summary>
        /// <param name = "service">Information about the member to access.</param>
        /// <returns>True if member is writable, false otherwise.</returns>
        private bool IsServiceWritable(MemberInfo service)
        {
            var isWritable = false;
            if(serviceInfo.ContainsKey(service.Name))
            {
                isWritable = serviceInfo[service.Name].IsServiceWritable;
            }
            return isWritable;
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            var toString = base.ToString() + "\n";

            toString += "Provider Object: ("  + ProviderObject + ")\n";

            toString += "Services: \n";
            foreach (var service in services)
            {
                toString += "\t Name:\"" + service.Key + "\", Type: ";
                switch (service.Value.MemberType)
                {
                    case MemberTypes.Field:
                        toString += "\"Field\"";
                        break;
                    case MemberTypes.Method:
                        toString += "\"Method\"";
                        break;
                    case MemberTypes.Property:
                        toString += "\"Property\"";
                        break;
                }

                toString += " Accessibility: ";
                var isAsync = IsServiceAsynchronous(service.Key);
                var isWriteable = IsServiceWritable(service.Value);
                if (!isAsync && !isWriteable)
                {
                    toString += "\"Synchronous Read Only\"";
                }
                else if (isAsync && !isWriteable)
                {
                    toString += "\"Asynchronous Read Only\"";
                }
                else
                {
                    toString += "\"Writable\"";
                }

                toString += "\n";
            }

            return toString;
        }

        #endregion

        #region Private Fields

        private Dictionary<string, MemberInfo> services;

        #endregion
    }
}
