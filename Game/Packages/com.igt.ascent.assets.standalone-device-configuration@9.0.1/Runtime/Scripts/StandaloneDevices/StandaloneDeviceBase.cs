//-----------------------------------------------------------------------
// <copyright file = "StandaloneDeviceBase.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    #endregion

    public abstract class StandaloneDeviceBase<TDeviceInterface> : MonoBehaviour, IStandaloneDevice
    {
        /// <summary>
        /// Type of support for the standalone device.
        /// </summary>
        public StandaloneDeviceType SupportType = StandaloneDeviceType.NotUsed;

        /// <inheritdoc/>
        public virtual IDictionary<Type, object> GetInterfaceImplementations()
        {
            // Early out if device isn't used.
            if(SupportType== StandaloneDeviceType.NotUsed)
                return null;

            var interfaces = new Dictionary<Type, object>();

            var deviceType = typeof(TDeviceInterface);
            var deviceObject = (SupportType == StandaloneDeviceType.VirtualImplementation)
                ? CreateVirtualImplementation()
                : null;
            interfaces.Add(deviceType, deviceObject);

            return interfaces;
        }

        /// <summary>
        /// Creates the standalone virtual implementation of the TDeviceInterface.
        /// </summary>
        /// <returns>Implementation of hte TDeviceInterface.</returns>
        protected abstract object CreateVirtualImplementation();
    }
}