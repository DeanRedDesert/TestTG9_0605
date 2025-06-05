//-----------------------------------------------------------------------
// <copyright file = "ServiceSignature.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///    Provides data for the asynchronous provider changed event.
    /// </summary>
    [Serializable]
    public class ServiceSignature
    {
        /// <summary>
        ///    Initialize an instance with a service name and service arguments.
        /// </summary>
        /// <param name = "serviceName">
        ///    This is the name of the service provider member that was updated.
        /// </param>
        /// <param name = "serviceArguments">
        ///    These are the arguments updated with the provider.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceName"/> string is null.</exception>
        public ServiceSignature(string serviceName, IDictionary<string, object> serviceArguments)
        {
            if(serviceName == null)
            {
                throw new ArgumentNullException("serviceName", "Argument may not be null.");
            }
            ServiceName = serviceName;
            ServiceArguments = serviceArguments;
        }

        /// <summary>
        ///    Initialize an instance with a service name.
        /// </summary>
        /// <param name = "serviceName">
        ///    This is the name of the service provider member that was updated.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceName"/> string is null.</exception>
        public ServiceSignature(string serviceName) : this(serviceName, null)
        {
        }

        /// <summary>
        ///    Get the name of the service provider member that was asynchronously updated
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        ///    Get the arguments that will be passed into the service provider member defined by <see cref = "ServiceName" />.
        /// </summary>
        /// <remarks>
        ///    The arguments defined in this property must exactly match the arguments defined by a presentation's
        ///    <see cref = "IGT.Game.Core.Communication.CommunicationLib.ServiceAccessor" />.
        /// </remarks>
        public IDictionary<string, object> ServiceArguments { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("Service Name: \"{0}\"", ServiceName));
            if(ServiceArguments != null)
            {
                builder.AppendLine(string.Format("Service Arguments: "));
                foreach(var arg in ServiceArguments)
                {
                        builder.AppendLine(string.Format("\tName:\"{0}\" Object: \"{1}\"",
                            arg.Key,
                            arg.Value));
                }
            }

            return builder.ToString();
        }
    }
}
