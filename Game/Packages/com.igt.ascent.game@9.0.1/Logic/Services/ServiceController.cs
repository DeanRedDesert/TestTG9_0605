//-----------------------------------------------------------------------
// <copyright file = "ServiceController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.CommunicationLib;

    /// <summary>
    ///    The ServiceController maintains a list of service providers and fills
    ///    Presentation data requests using those providers.
    /// </summary>
    /// <remarks>
    /// Adding/updating/removing providers in the service controller by multiple threads is not guaranteed
    /// thread-safe; updating the services by multiple threads is not thread-safe either.
    /// It's the caller's obligation to implement the synchronization in multi-thread situations.
    /// </remarks>
    public class ServiceController : IServiceController
    {
        /// <summary>
        /// Returns a Copy of the current list of Service Providers and their services.
        /// </summary>
        public IDictionary<string, Provider> ServiceProviders
        {
            get
            {
                var providersCopy = new Dictionary<string, Provider>();
                foreach(var serviceProvider in serviceProviders)
                {
                    var provider = new Provider(serviceProvider.Value);

                    providersCopy.Add(serviceProvider.Key, provider);
                }

                return providersCopy;
            }
        }

        /// <summary>
        ///    Represents the method that will handle the AsynchronousProviderUpdated event that is raised when
        ///    a service provider value is changed.
        /// </summary>
        public event EventHandler<StartFillAsynchronousRequestEventArgs> StartFillAsynchronousRequest;

        #region Public Methods

        /// <summary>
        ///    Initialize an instance of ServiceController.
        /// </summary>
        public ServiceController()
        {
            serviceProviders = new Dictionary<string, Provider>();
        }

        /// <summary>
        ///    Get the data requested by the presentation.
        ///    DataItems is suitable for use in the StartState message or an asynchronous update message.
        /// </summary>
        /// <param name = "request">A list of data required by the Presentation.</param>
        /// <returns>Data Item list that contains the requested data.</returns>
        /// <remarks>
        /// Adding/updating/removing providers in the service controller by multiple threads is not guaranteed
        /// thread-safe; updating the services by multiple threads is not thread-safe either.
        /// It's the caller's obligation to implement the synchronization in multi-thread situations.
        /// </remarks>
        public DataItems FillRequest(ServiceRequestData request)
        {
            return GetRequestedData(request, false);
        }

        /// <summary>
        ///    Asynchronously get the data requested by the presentation.
        ///    DataItems is suitable for use in the StartState message or an asynchronous update message.
        /// </summary>
        /// <param name = "request">A list of data required by the Presentation.</param>
        /// <returns>Data Item list that contains the requested data.</returns>
        /// <remarks>
        /// Adding/updating/removing providers in the service controller by multiple threads is not guaranteed
        /// thread-safe; updating the services by multiple threads is not thread-safe either.
        /// It's the caller's obligation to implement the synchronization in multi-thread situations.
        /// </remarks>
        public DataItems FillAsynchronousRequest(ServiceRequestData request)
        {
            return GetRequestedData(request, true);
        }

        #endregion

        #region IServiceController Implementation

        /// <inheritdoc/>
        public void AddProvider(object provider, string providerName)
        {
            if(provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if(providerName == null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            var serviceProvider = new Provider(provider);
            if(serviceProviders.ContainsKey(providerName))
            {
                throw new DuplicateServiceProviderException(providerName,
                                                            "Provider already exists, please choose a different provider name.");
            }

            serviceProviders.Add(providerName, serviceProvider);

            // If this service provider implements the INotifyPropertyChanged interface
            if(provider is INotifyAsynchronousProviderChanged notifyAsynchronousProviderChanged)
            {
                // Register with the property changed event handler
                notifyAsynchronousProviderChanged.AsynchronousProviderChanged += OnProviderChangedEventHandler;
            }
            else
            {
                foreach(var service in serviceProvider.Services)
                {
                    if(serviceProvider.IsServiceAsynchronous(service.Key))
                    {
                        // If this provider has any asynchronous services and doesn't implement the
                        // INotifyAsynchronousProviderChanged interface, make sure the developer knows it won't work.
                        throw new InvalidServiceAttributeException(provider.GetType().Name, service.Key,
                                                                   "Provider doesn't support INotifyAsynchronousProviderChanged interface.");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AddProvider(INamedProvider provider)
        {
            AddProvider(provider, provider.Name);
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            string toString = base.ToString() + "\n";

            toString += "Providers: \n";
            foreach(var serviceProvider in serviceProviders)
            {
                toString += "\tProvider Name: \"" + serviceProvider.Key + "\"\n";
                toString += "\tProvider Info: \n\t--Start--\n" + serviceProvider.Value;
                toString += "\t--End--\n\n";
            }

            return toString;
        }

        #endregion

        #region Protected Event Handlers

        /// <summary>
        ///    Handle notification that a service provider's property value has changed asynchronously.
        /// </summary>
        /// <param name = "sender">Service provider that triggered this event.</param>
        /// <param name = "eventArgs">Arguments for the event that tell which provider member changed.</param>
        /// <exception cref = "ArgumentNullException">Parameters of this method cannot be null.</exception>
        protected void OnProviderChangedEventHandler(object sender, AsynchronousProviderChangedEventArgs eventArgs)
        {
            // Verify that the sender and event arguments are not null
            if(sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if(eventArgs == null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            string providerName = string.Empty;
            var serviceSignatures = eventArgs.ServiceSignatures;
            Provider updatedProvider = null;

            // Find the service provider in the provider list.
            foreach(var providerValue in serviceProviders)
            {
                if(providerValue.Value.ProviderObject == sender)
                {
                    providerName = providerValue.Key;
                    updatedProvider = providerValue.Value;
                    break;
                }
            }

            // If the sender passed was not a registered service provider
            if(updatedProvider == null)
            {
                throw new UnknownServiceProviderException(sender.GetType().Name,
                                                          "Sender of asynchronous provider changed event is not a service provider registered with service controller.");
            }

            // Make sure that the member is a game service member of the service provider and
            // make sure the member that was updated is actually an AsynchronousGameService member.
            var invalidList = (from service in serviceSignatures
                               where !updatedProvider.IsServiceAsynchronous(service.ServiceName)
                               select service.ServiceName).ToList();

            if(invalidList.Any())
            {
                var errorString = invalidList.Aggregate((a, b) => a + ", " + b);
                throw new InvalidServiceAttributeException(providerName, errorString,
                                                           "Updated service provider member(s) is not an asynchronous game service member. " +
                                                           "Cannot use this value(s) for asynchronous data updates.");
            }

            // If the provider was found raise the asynchronous provider updated event.
            if(!string.IsNullOrEmpty(providerName))
            {
                StartFillAsynchronousRequest?.Invoke(this,
                                                     new StartFillAsynchronousRequestEventArgs(providerName, serviceSignatures,
                                                                                               eventArgs.Transactional));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///    Fill the data request.
        /// </summary>
        /// <param name = "request">Data requested by the presentation.</param>
        /// <param name = "asynchronous">
        ///    Flag indicating if the request was made asynchronously or not. True for asynchronous.
        /// </param>
        /// <returns>Data Item list that contains the requested data.</returns>
        /// <exception cref = "UnknownServiceProviderException">
        ///    Thrown when a service provider requested is not found in the list of providers.
        /// </exception>
        /// <exception cref = "ArgumentNullException">
        ///    Thrown when required data is null.
        /// </exception>
        private DataItems GetRequestedData(ServiceRequestData request, bool asynchronous)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request), "Required data may not be null.");
            }

            var requestedData = new DataItems();

            // For every item requested
            foreach(var requestedProviderData in request)
            {
                Provider serviceProvider;

                // Get the requested service provider object.
                try
                {
                    serviceProvider = serviceProviders[requestedProviderData.Key];
                }
                catch(KeyNotFoundException e)
                {
                    throw new UnknownServiceProviderException(requestedProviderData.Key,
                                                              "Service provider could not be found.", e);
                }

                if(requestedProviderData.Value == null)
                {
                    throw new InvalidProviderParametersException(requestedProviderData.Key,
                                                                 "", "List of service accessors cannot be null.");
                }

                var providerResults = new Dictionary<int, object>();

                // find all of the requested data.
                foreach(var locator in requestedProviderData.Value)
                {
                    object serviceValue;
                    if(asynchronous)
                    {
                        // This verifies that the service member is allowed to be accessed
                        // asynchronously.
                        serviceValue = serviceProvider.GetAsynchronousServiceValue(
                            locator.Service, locator.ServiceArguments);
                    }
                    else
                    {
                        serviceValue = serviceProvider.GetServiceValue(locator.Service, locator.ServiceArguments);
                    }

                    // Get the data from the service provider
                    providerResults.Add(locator.Identifier, serviceValue);
                }

                // Add the provider results to the requested data list.
                requestedData.Add(requestedProviderData.Key, providerResults);
            }

            return requestedData;
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<string, Provider> serviceProviders;

        #endregion
    }
}