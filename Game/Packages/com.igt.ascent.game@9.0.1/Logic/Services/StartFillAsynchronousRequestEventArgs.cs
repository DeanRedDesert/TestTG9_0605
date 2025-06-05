//-----------------------------------------------------------------------
// <copyright file = "StartFillAsynchronousRequestEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///    Event arguments for notifying that an asynchronous service provider value has been updated.
    ///    and a FillAsynchronousRequest should be started.
    /// </summary>
    public class StartFillAsynchronousRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of StartFillAsynchronousRequestEventArgs with a provider name, list of service signatures, and
        /// a transactional flag.
        /// </summary>
        /// <param name = "providerName">Name of the ServiceProvider that was asynchronously updated.</param>
        /// <param name = "serviceSignatures">List of pairings of, service names and their service arguemnts, of the
        ///  provider member that were asynchronously updated.</param>
        /// <param name="transactional">Flag which indicates if this update is backed by a transaction.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="serviceSignatures"/> list or the <paramref name="providerName"/> 
        /// is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="serviceSignatures"/> list is empty
        /// or when any item in the service signature list is null.</exception>
        public StartFillAsynchronousRequestEventArgs(string providerName, IList<ServiceSignature> serviceSignatures,
            bool transactional)
        {
            if(providerName == null)
            {
                throw new ArgumentNullException("providerName", "Argument may not be null.");
            }
            if(serviceSignatures == null)
            {
                throw new ArgumentNullException("serviceSignatures", "Argument may not be null");
            }
            if(!serviceSignatures.Any())
            {
                throw new ArgumentException("The updated service signatures list may not be empty", "serviceSignatures");
            }
            if(serviceSignatures.Any(serviceSignature => serviceSignature == null))
            {
                throw new ArgumentException("No item in the service signatures list is allowed to be null.",
                    "serviceSignatures");
            }

            ServiceProviderName = providerName;
            ServiceSignatures = serviceSignatures;
            Transactional = transactional;
        }

        /// <summary>
        ///  Initialize a new instance of StartFillAsynchronousRequestEventArgs with a provider name, and list of 
        ///  ServiceSignatures. This creates the instance as transactional.
        /// </summary>
        /// <param name = "providerName">Name of the ServiceProvider that was asynchronously updated.</param>
        /// <param name = "serviceSignatures">List of pairings of, service names and their service arguemnts, of the
        ///  provider member that were asynchronously updated.</param>
        public StartFillAsynchronousRequestEventArgs(string providerName, IList<ServiceSignature> serviceSignatures)
            : this(providerName, serviceSignatures, true)
        {
        }

        /// <summary>
        ///    Get the name of the service provider that contains the asynchronous update.
        /// </summary>
        public string ServiceProviderName { get; private set; }

        /// <summary>
        ///    Get the name of the asynchronous provider member that was updated.
        /// </summary>
        public IList<ServiceSignature> ServiceSignatures { get; private set; }

        /// <summary>
        /// Flag which indicates if this update is transactional.
        /// </summary>
        public bool Transactional { private set; get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine(string.Format("Service Provider Name: \"{0}\"", ServiceProviderName));
            foreach(var service in ServiceSignatures)
            {
                builder.AppendLine(service.ToString());
            }

            return builder.ToString();
        }
    }
}