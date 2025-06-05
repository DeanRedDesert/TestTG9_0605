//-----------------------------------------------------------------------
// <copyright file = "StandaloneDeviceConfigurator.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using StandaloneDevices;
    using UnityEngine;

    #endregion

    /// <summary>
    /// This class is used as the collector for standalone device interfaces.
    /// </summary>
    public class StandaloneDeviceConfigurator : MonoBehaviour
    {

        #region Protected Fields

        /// <summary>
        /// A list of disposable objects created by this object.
        /// </summary>
        protected readonly List<IDisposable> DisposableObjects = new List<IDisposable>();

        #endregion

        #region MonoBehavior Overrides

        /// <summary>
        /// Called before the application is exited.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            // Dispose all disposable standalone device implementations.
            foreach(var disposableObject in DisposableObjects)
            {
                disposableObject.Dispose();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the interfaces for the configured devices for the standalone cabinet.
        /// </summary>
        /// <returns>Standalone device implementation grouped by interface.</returns>
        public IDictionary<Type, object> GetStandaloneCabinetInterfaces()
        {
            var result = new Dictionary<Type, object>();

            // Check for any standalone devices on or a child of this object to override.
            var standaloneDevices = GetComponentsInChildren<IStandaloneDevice>();
            if(standaloneDevices != null)
            {
                foreach(var device in standaloneDevices)
                {
                    var deviceInterfaces = device.GetInterfaceImplementations();
                    if(deviceInterfaces == null)
                        continue;

                    foreach(var deviceInterface in deviceInterfaces)
                    {
                        // Ignore duplicate registrations.
                        if(result.ContainsKey(deviceInterface.Key))
                        {
                            var warningString =
                                string.Format(
                                    "Standalone device already registered for {0}.  Ignoring additional registrations.",
                                    deviceInterface.Key);
                            Debug.LogWarning(warningString);
                            continue;
                        }

                        result[deviceInterface.Key] = deviceInterface.Value;

                        AddDisposableObject(deviceInterface.Value);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Check if an object is a disposable one, if yes,
        /// add it to the disposable object list.
        /// </summary>
        /// <remarks>
        /// This function must be called immediately after
        /// a disposable object is instantiated.
        /// </remarks>
        /// <param name="candidate">The object to check and add.</param>
        protected void AddDisposableObject(object candidate)
        {
            var disposableObject = candidate as IDisposable;

            if(disposableObject != null)
            {
                DisposableObjects.Add(disposableObject);
            }
        }

        #endregion
    }
}