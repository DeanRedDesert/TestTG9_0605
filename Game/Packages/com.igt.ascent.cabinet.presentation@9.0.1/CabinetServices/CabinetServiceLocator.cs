// -----------------------------------------------------------------------
// <copyright file = "CabinetServiceLocator.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Singleton service locator for cabinet devices services.
    /// </summary>
    public sealed class CabinetServiceLocator : ICabinetServiceLocator, ICabinetServiceLocatorRestricted, 
                                                IDiscoverableCabinetServiceLocator, IDiscoverableCabinetServiceLocatorRestricted
    {
        /// <summary>
        /// All services that have been set in the locator.
        /// </summary>
        private readonly Dictionary<Type, ICabinetService> services = new Dictionary<Type, ICabinetService>();

        /// <summary>
        /// All service listeners that have been registered on the locator.
        /// </summary>
        private readonly Dictionary<Type, IList<ICabinetServiceListener>> listeners = new Dictionary<Type, IList<ICabinetServiceListener>>();

        /// <summary>
        /// The single instance of the locator. Used to insure there is only one.
        /// </summary>
        private static CabinetServiceLocator instance;

        /// <summary>
        /// Synchronization object to lock when accessing <see cref="services"/> or <see cref="instance"/>.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Private constructor for the singleton instance.
        /// </summary>
        private CabinetServiceLocator() { }

        #region Public Methods

        /// <summary>
        /// Gets the singleton instance, creating it first if necessary.
        /// </summary>
        public static ICabinetServiceLocator Instance
        {
            get
            {
                lock(InstanceLock)
                {
                    return instance ?? (instance = new CabinetServiceLocator());
                }
            }
        }

        #region IDeviceServiceLocatorRestricted

        /// <inheritdoc/>
        public void AddService<TService>(TService service) where TService : class, ICabinetService
        {
            var type = GetAndVerifyInterfaceType<TService>();
            lock(InstanceLock)
            {
                if(services.ContainsKey(type))
                {
                    RemoveService<TService>();
                }

                services[type] = service;

                if(listeners.TryGetValue(type, out var serviceListeners))
                {
                    // Make a copy of the listeners, in case any OnServiceAvailable callback
                    // tries to register/unregister a listener.
                    var copyList = serviceListeners.ToList();
                    foreach(var listener in copyList)
                    {
                        ((ICabinetServiceListener<TService>)listener).OnServiceAvailable(service);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveService<TService>() where TService : class, ICabinetService
        {
            var type = GetAndVerifyInterfaceType<TService>();
            lock(InstanceLock)
            {
                if(services.TryGetValue(type, out var serviceValue))
                {
                    var service = (TService)serviceValue;
                    if(listeners.TryGetValue(type, out var serviceListeners))
                    {
                        // Make a copy of the listeners, in case any OnServiceRemoved callback
                        // tries to register/unregister a listener.
                        var copyList = serviceListeners.ToList();
                        foreach(var listener in copyList)
                        {
                            ((ICabinetServiceListener<TService>)listener).OnServiceRemoved(service);
                        }
                    }
                    services.Remove(type);
                }
            }
        }

        #endregion

        #region IDiscoverableCabinetServiceLocatorRestricted

        /// <inheritdoc/>
        public void AddDiscoveredService(IMefDiscoverableService service)
        {
            var runTimeDiscoveredService = service.ExportedType;

            lock(InstanceLock)
            {
                if(services.ContainsKey(runTimeDiscoveredService))
                {
                    RemoveDiscoveredService(runTimeDiscoveredService);
                }

                var serviceAsCabinetService = service as ICabinetService;
                services[runTimeDiscoveredService] = serviceAsCabinetService;

                if(listeners.ContainsKey(runTimeDiscoveredService))
                {
                    foreach(var listener in listeners[runTimeDiscoveredService])
                    {
                        ((IDiscoverableCabinetServiceListener)listener).OnServiceAvailable(runTimeDiscoveredService);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveDiscoveredService(Type runTimeDiscoveredService)
        {
            lock(InstanceLock)
            {
                if(services.ContainsKey(runTimeDiscoveredService))
                {
                    if(listeners.ContainsKey(runTimeDiscoveredService))
                    {
                        foreach(var listener in listeners[runTimeDiscoveredService])
                        {
                            ((IDiscoverableCabinetServiceListener)listener).OnServiceRemoved(runTimeDiscoveredService);
                        }
                    }
                    services.Remove(runTimeDiscoveredService);
                }
            }
        }

        #endregion

        #region ICabinetServiceLocator

        /// <inheritdoc/>
        public TService GetService<TService>() where TService : class, ICabinetService
        {
            var type = GetAndVerifyInterfaceType<TService>();
            lock(InstanceLock)
            {
                return (TService)(services.ContainsKey(type) ? services[type] : null);
            }
        }

        /// <inheritdoc/>
        public void RegisterListener<TService>(ICabinetServiceListener<TService> listener) where TService : class, ICabinetService
        {
            var type = GetAndVerifyInterfaceType<TService>();
            lock(InstanceLock)
            {
                if(listeners.ContainsKey(type))
                {
                    var registered = listeners[type];
                    if(!registered.Contains(listener))
                    {
                        registered.Add(listener);
                    }
                }
                else
                {
                    listeners[type] = new List<ICabinetServiceListener> { listener };
                }
            }
        }

        /// <inheritdoc/>
        public void UnregisterListener<TService>(ICabinetServiceListener<TService> listener) where TService : class, ICabinetService
        {
            var type = GetAndVerifyInterfaceType<TService>();
            lock(InstanceLock)
            {
                if(listeners.ContainsKey(type))
                {
                    listeners[type].Remove(listener);
                }
            }
        }

        #endregion

        #region IDiscoverableCabinetServiceLocator
        
        /// <inheritdoc/>
        public void RegisterDiscoverableListener(IMefDiscoverableService mefDiscoverableService, IDiscoverableCabinetServiceListener listener)
        {
            lock(InstanceLock)
            {
                if(mefDiscoverableService == null)
                {
                    throw new ArgumentNullException(nameof(mefDiscoverableService));
                }

                if(listener == null)
                {
                    throw new ArgumentNullException(nameof(listener));
                }

                if(listeners.ContainsKey(mefDiscoverableService.ExportedType))
                {
                    var registered = listeners[mefDiscoverableService.ExportedType];
                    if(!registered.Contains(listener))
                    {
                        registered.Add(listener);
                    }
                }
                else
                {
                    listeners[mefDiscoverableService.ExportedType] = new List<ICabinetServiceListener> { listener };
                }
            }
        }

        /// <inheritdoc/>
        public void UnregisterDiscoverableListener(IMefDiscoverableService mefDiscoverableService, IDiscoverableCabinetServiceListener listener)
        {
            lock(InstanceLock)
            {
                if(mefDiscoverableService == null)
                {
                    throw new ArgumentNullException(nameof(mefDiscoverableService));
                }

                if(listener == null)
                {
                    throw new ArgumentNullException(nameof(listener));
                }

                if(listeners.ContainsKey(mefDiscoverableService.ExportedType))
                {
                    listeners[mefDiscoverableService.ExportedType].Remove(listener);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Return the parameterized type after ensuring it is an interface.
        /// </summary>
        /// <typeparam name="TService">Specified type to get and verify.</typeparam>
        /// <returns>Type of <typeparamref name="TService"/>.</returns>
        /// <exception cref="CabinetServiceTypeException">Thrown if <typeparamref name="TService"/> isn't an interface.</exception>
        private static Type GetAndVerifyInterfaceType<TService>()
        {
            var type = typeof(TService);
            if(!type.IsInterface)
            {
                throw new CabinetServiceTypeException(type);
            }
            return type;
        }

        #endregion
    }
}