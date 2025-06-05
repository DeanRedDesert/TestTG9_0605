//-----------------------------------------------------------------------
// <copyright file = "AsynchronousProviderChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///    Provides data for the asynchronous provider changed event.
    /// </summary>
    /// <remarks>
    ///    AsynchronousProviderChangedEventArgs provides the <see cref = "ServiceSignatures" />
    ///    properties to get the needed data for accessing the correct provider member.
    /// </remarks>
    /// <example>
    ///    See the <see cref = "INotifyAsynchronousProviderChanged" /> documentation for an example.
    /// </example>
    public class AsynchronousProviderChangedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///    Initialize an instance with a member name.
        /// </summary>
        /// <param name = "serviceName">
        ///    This is the name of the service provider member that was updated.
        /// </param>
        /// <remarks>All services backed by foundation events are intended to be transactional.</remarks>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceName"/> is null.</exception>
        public AsynchronousProviderChangedEventArgs(string serviceName)
            : this(new ServiceSignature(serviceName, null))
        {
        }

        /// <summary>
        ///    Initialize an instance with a service signature.
        /// </summary>
        /// <param name = "serviceSignature">
        ///    This is a pairing of the service name and event args of the service provider member that was updated.
        /// </param>
        /// <remarks>All services backed by foundation events are intended to be transactional.</remarks>
        ///<exception cref="ArgumentNullException">Thrown when the <paramref name="serviceSignature"/>
        /// is null.</exception>
        public AsynchronousProviderChangedEventArgs(ServiceSignature serviceSignature) 
            : this(serviceSignature, true)
        {
        }

        /// <summary>
        ///    Initialize an instance with a service signature and transactional flag.
        /// </summary>
        /// <param name = "serviceSignature">
        ///    This is a pairing of the service name and event args of the service provider member that was updated.
        /// </param>
        /// <param name="transactional">Flag indicating if the update is transactional.</param>
        /// <remarks>All services backed by foundation events are intended to be transactional.</remarks>
        ///<exception cref="ArgumentNullException">Thrown when the <paramref name="serviceSignature"/>
        /// is null.</exception>
        public AsynchronousProviderChangedEventArgs(ServiceSignature serviceSignature, bool transactional)
            : this(new List<ServiceSignature> {serviceSignature}, transactional)
        {
        }

        /// <summary>
        ///    Initialize an instance with a member name, it's arguments, and a transactional flag.
        /// </summary>
        /// <param name = "serviceName">
        ///    This is the name of the service provider member that was updated.
        /// </param>
        /// <param name = "serviceArguments">
        ///    These are arguments that would be passed into a service provider. This should be used for methods.
        ///    These arguments must exactly match a
        ///    <see cref = "IGT.Game.Core.Communication.CommunicationLib.ServiceAccessor" /> argument list defined by the
        ///    presentation.
        /// </param>
        /// <param name="transactional">Flag indicating if the update is transactional.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceName"/> or the 
        /// <paramref name="serviceArguments"/> are null.</exception> 
        public AsynchronousProviderChangedEventArgs(string serviceName,
            IDictionary<string, object> serviceArguments, bool transactional)
            : this(new ServiceSignature(serviceName, serviceArguments), transactional)
        {
        }

        /// <summary>
        ///    Initialize an instance with the service signatures and a transactional flag.
        /// </summary>
        /// <param name = "serviceSignatures">
        ///    This is a list of pairings of, service names and event args, of the service provider member that was updated.
        /// </param>
        /// <param name="transactional">Flag indicating if the update is transactional.</param>
        /// <remarks>All services backed by foundation events are intended to be transactional.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceSignatures"/>
        /// is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceSignatures"/> list is empty
        /// or when any item in the service signature list is null.</exception>
        public AsynchronousProviderChangedEventArgs(IList<ServiceSignature> serviceSignatures, bool transactional)
        {
            if(serviceSignatures == null)
            {
                throw new ArgumentNullException("serviceSignatures", "Argument may not be null.");
            }
            if (!serviceSignatures.Any())
            {
                throw new ArgumentException("The updated service signatures list may not be empty.", "serviceSignatures");
            }

            if (serviceSignatures.Any(service => service == null))
            {
                throw new ArgumentException("No item in the service signature list is allowed to be null.", "serviceSignatures");
            }
            ServiceSignatures = serviceSignatures;
            Transactional = transactional;
        }

        #endregion

        #region Properties

        /// <summary>
        ///    Get the arguments that will be passed into the service provider member defined by <see cref = "ServiceSignatures" />.
        /// </summary>
        /// <remarks>
        ///    The arguments defined in this property must exactly match the arguments defined by a presentation's
        ///    <see cref = "IGT.Game.Core.Communication.CommunicationLib.ServiceAccessor" />.
        /// </remarks>
        public IList<ServiceSignature> ServiceSignatures { get; private set; }

        /// <summary>
        /// Flag which indicates if this updated is transactional. Only transactional updates are eligible for being
        /// stored in history.
        /// </summary>
        public bool Transactional { private set; get; }

        #endregion
    }
}