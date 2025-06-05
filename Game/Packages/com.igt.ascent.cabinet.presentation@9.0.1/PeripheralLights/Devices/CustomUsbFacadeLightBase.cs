//-----------------------------------------------------------------------
// <copyright file = "CustomUsbFacadeLightBase.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CabinetServices;

    /// <summary>
    ///     The generic base for creating a custom USB facade light controller.
    /// </summary>
    /// <typeparam name="TDerived">The type implementing this class.</typeparam>
    public abstract class CustomUsbFacadeLightBase<TDerived>
        where TDerived : CustomUsbFacadeLightBase<TDerived>
    {
        // Disable the warning here because the name needs to be different to not conflict
        // with the Instance property.
        // ReSharper disable InconsistentNaming
        private static readonly TDerived instance = CreateInstance();
        // ReSharper restore InconsistentNaming

        private bool initialized;

        /// <summary>
        ///     Gets the instance of the singleton.
        /// </summary>
        public static TDerived Instance
        {
            get
            {
                if(!instance.initialized)
                {
                    instance.Initialize();
                }

                return instance;
            }
        }

        /// <summary>
        ///     Indicates if the device appears to be valid and is reporting the
        ///     correct number of light groups.
        /// </summary>
        public bool IsValidDevice { get; private set; }

        /// <summary>
        ///     The USB facade light object.
        /// </summary>
        protected UsbFacadeLight UsbLight { get; private set; }

        /// <summary>
        ///     The number of groups the hardware device requires and must report to be
        ///     considered valid.
        /// </summary>
        protected abstract byte RequiredGroupCount { get; }

        /// <summary>
        ///     Initializes the light device.
        /// </summary>
        protected virtual void Initialize()
        {
            var peripheralLightService = CabinetServiceLocator.Instance.GetService<IPeripheralLightService>();

            if(peripheralLightService != null)
            {
                UsbLight = peripheralLightService.GetPeripheralLight(FacadeHardware.Facade);
                IsValidDevice = UsbLight.GroupCount == RequiredGroupCount;

                initialized = true;
            }
        }

        /// <summary>
        ///     Splits a 16-bit word into two bytes and adds it to a list.
        /// </summary>
        /// <param name="list">The list to add to.</param>
        /// <param name="word">The data to add.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown if <paramref name="list" /> is null.
        /// </exception>
        protected void AddUInt16(IList<byte> list, ushort word)
        {
            if(list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var lower = Convert.ToByte(word & 0xFF);
            var upper = Convert.ToByte((word & 0xFF00) >> 8);

            list.Add(lower);
            list.Add(upper);
        }

        /// <summary>
        ///     Determines if the passed in group id is valid.
        /// </summary>
        /// <param name="groupId">The light group id to check.</param>
        /// <returns>True if <paramref name="groupId" /> is valid.</returns>
        protected bool IsValidGroupId(byte groupId)
        {
            return groupId < RequiredGroupCount || groupId == UsbLightBase.AllGroups;
        }

        /// <summary>
        ///     Creates an instance of <typeparamref name="TDerived" />.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TDerived" />.</returns>
        /// <exception cref="System.TypeLoadException">
        ///     Thrown if <typeparamref name="TDerived" /> does not have a non-public parameter-less constructor.
        /// </exception>
        private static TDerived CreateInstance()
        {
            var templateType = typeof(TDerived);
            TDerived newInstance;

            try
            {
                newInstance = (TDerived)templateType.InvokeMember(templateType.Name,
                    BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, null, null);
            }
            catch(MissingMethodException ex)
            {
                throw new TypeLoadException(
                    $"The data type {templateType.FullName} must have a non public parameter-less constructor in order to be used as a singleton.",
                    ex);
            }

            return newInstance;
        }
    }
}