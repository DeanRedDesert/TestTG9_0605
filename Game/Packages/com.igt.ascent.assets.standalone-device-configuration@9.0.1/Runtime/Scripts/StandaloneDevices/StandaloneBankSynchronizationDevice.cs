//-----------------------------------------------------------------------
// <copyright file = "StandaloneBankSynchronizationDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using Core.Communication.Cabinet;
    using Core.Communication.Cabinet.Standalone;
    using StandaloneDeviceConfiguration;
    using UnityEngine;

    #endregion

    public class StandaloneBankSynchronizationDevice : StandaloneDeviceBase<IBankSynchronization>
    {
        /// <summary>
        /// Peripheral lights device.
        /// </summary>
        public StandalonePeripheralLightsDevice PeripheralLightsDevice;

        /// <summary>
        /// Streaming lights device.
        /// </summary>
        public StandaloneStreamingLightsDevice StreamingLightsDevice;

        /// <inheritdoc />
        public override IDictionary<Type, object> GetInterfaceImplementations()
        {
            // Early out if not used.
            if(SupportType == StandaloneDeviceType.NotUsed)
                return null;

            var interfaces = new Dictionary<Type, object>();

            if(PeripheralLightsDevice == null ||
               StreamingLightsDevice == null ||
               PeripheralLightsDevice.SupportType == StandaloneDeviceType.NotUsed ||
               StreamingLightsDevice.SupportType == StandaloneDeviceType.NotUsed)
            {
                Debug.LogWarning("Bank synchronization requires peripheral lights and streaming lights support.");
            }
            else
            {
                var deviceType = typeof(IBankSynchronization);
                var deviceObject = (SupportType == StandaloneDeviceType.VirtualImplementation)
                    ? CreateVirtualImplementation()
                    : null;
                interfaces.Add(deviceType, deviceObject);
                
            }

            return interfaces;
        }

        /// <summary>
        /// Called when added in editor.  Used to auto hook up components if on same object.
        /// </summary>
        protected void Reset()
        {
            var pld = GetComponent<StandalonePeripheralLightsDevice>();
            var sld = GetComponent<StandaloneStreamingLightsDevice>();
            if(PeripheralLightsDevice == null && pld != null)
            {
                PeripheralLightsDevice = pld;
            }

            if(StreamingLightsDevice == null && sld != null)
            {
                StreamingLightsDevice = sld;
            }
        }

        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            var bankObject = new BankSynchronization();

            var standaloneController = GetComponent<StandaloneBankSynchronization>();
            if(standaloneController != null)
            {
                standaloneController.SetBankSynchronizerObject(bankObject);
            }

            return bankObject;
        }
    }
}