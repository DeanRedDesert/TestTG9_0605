// -----------------------------------------------------------------------
// <copyright file = "UsbLightBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Linq;
    using System.Text;
    using CabinetServices;
    using Communication.Cabinet;

    /// <summary>
    /// The abstract base of all USB light classes.
    /// </summary>
    public abstract class UsbLightBase : IDeviceInformation
    {
        #region Constants

        /// <summary>
        /// A constant representing the flag for applying a command to all light groups.
        /// </summary>
        public const byte AllGroups = 0xFF;

        #endregion

        #region Fields

        /// <summary>
        /// The flag indicating if the light device has a valid content set by the client.
        /// </summary>
        protected bool ClientContentPresent;

        /// <summary>
        /// The flag indicating if the undergoing light control command is
        /// for setting a content that is not from the client.
        /// </summary>
        protected bool SettingNonClientContent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets the feature name of this light device.
        /// </summary>
        public string FeatureName { get; protected set; }

        /// <summary>
        /// Gets the feature description of this light device.
        /// </summary>
        public LightFeatureDescription FeatureDescription { get; protected set; }

        /// <summary>
        /// Gets the flag indicating whether the light device has a valid content set by the client.
        /// </summary>
        internal bool HasClientContent => ClientContentPresent;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UsbLightBase"/>.
        /// </summary>
        /// <param name="featureName">The feature name of the light device.</param>
        /// <param name="featureDescription">The feature description of the light device.</param>
        protected UsbLightBase(string featureName, LightFeatureDescription featureDescription)
        {
            FeatureName = featureName;
            FeatureDescription = featureDescription;

            GroupCount = FeatureDescription != null ? Convert.ToUInt16(FeatureDescription.Groups.Count()) : (ushort)0;
        }

        #endregion

        #region Abstract Members

        /// <summary>
        /// Sets all the lights in a particular group to a single color.
        /// </summary>
        /// <param name="groupId">The light group to set color for.</param>
        /// <param name="color">The light color to set to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is not valid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="color"/> is empty.
        /// </exception>
        public abstract void SetColor(byte groupId, Color color);

        /// <summary>
        /// Turns on/off all the lights in a particular group.
        /// </summary>
        /// <param name="groupId">The light group to turn on/off.</param>
        /// <param name="lightOn">The flag indicating if the lights should be on or off.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is not valid.
        /// </exception>
        public abstract void SetLightState(byte groupId, bool lightOn);

        /// <summary>
        /// Sets all the lights to a universal color.
        /// Universal color is not counted as a valid light content.
        /// </summary>
        /// <param name="universalColor">The universal color to set to.</param>
        internal abstract void SetUniversalColor(Color universalColor);

        /// <summary>
        /// Determines if a given exception thrown by the light category should be reported.
        /// </summary>
        /// <param name="exception">
        /// Exception thrown when an error status is received from the light category.
        /// </param>
        /// <returns>True if the given exception should be reported; False otherwise.</returns>
        protected abstract bool ShouldLightCategoryErrorBeReported(Exception exception);

        #endregion

        #region Virtual Members

        /// <summary>
        /// Sets all the lights in a particular group to a single color,
        /// and all the lights on the blank light devices to a universal color.
        /// </summary>
        /// <param name="groupId">The light group to set color for.</param>
        /// <param name="color">The light color to set the light group to.</param>
        /// <param name="universalColor">The universal color to set the blank light devices to.</param>
        public virtual void SetColor(byte groupId, Color color, Color universalColor)
        {
            SetColor(groupId, color);

            // Setting universal color only after setting color for this device first,
            // so that this device is excluded from the blank light filtering.
            CabinetServiceLocator.Instance.GetService<IPeripheralLightService>()?.SetUniversalColor(universalColor);
        }

        /// <summary>
        /// Sets the feature description of this light device.
        /// </summary>
        /// <param name="featureDescription">The feature description to set.</param>
        internal virtual void SetFeatureDescription(LightFeatureDescription featureDescription)
        {
            FeatureDescription = featureDescription;
            if(FeatureDescription != null)
            {
                GroupCount = Convert.ToUInt16(FeatureDescription.Groups.Count());
            }
        }

        /// <summary>
        /// Sets the flag indicating whether the light device has client content.
        /// The flag is not set if the undergoing command is for setting non client content.
        /// </summary>
        protected virtual void FlagClientContentPresent()
        {
            if(!ClientContentPresent && !SettingNonClientContent)
            {
                ClientContentPresent = true;
            }
        }

        /// <summary>
        /// Checks if a given group id is valid when AllGroups is not allowed.
        /// </summary>
        /// <param name="groupId">The group id to check.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of light groups available.
        /// </exception>
        protected void ValidateGroupId(byte groupId)
        {
            ValidateGroupId(groupId, false);
        }

        /// <summary>
        /// Checks if a given group id is valid or not.
        /// </summary>
        /// <param name="groupId">The group id to check.</param>
        /// <param name="allowAllGroups">The flag indicating if AllGroups is allowed.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of light groups available,
        /// or it is AllGroups but AllGroups is not allowed.
        /// </exception>
        protected virtual void ValidateGroupId(byte groupId, bool allowAllGroups)
        {
            // If the feature description is not set, the GroupCount is not properly initialized.
            if(FeatureDescription == null)
            {
                return;
            }

            if(groupId == AllGroups)
            {
                if(!allowAllGroups)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId), "AllGroups is not allowed.");
                }
            }
            else if(groupId >= GroupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"Group ID {groupId} is out of range. Only {GroupCount} groups found on device {HardwareType}.");
            }
        }

        #endregion

        #region IDeviceInformation Members

        /// <inheritdoc/>
        public virtual bool DeviceAcquired { get; internal set; }

        /// <inheritdoc/>
        public DeviceAcquisitionFailureReason? AcquireFailureReason { get; internal set; }

        /// <inheritdoc/>
        public ushort GroupCount { get; protected internal set; }

        /// <inheritdoc/>
        public Hardware HardwareType { get; internal set; }

        /// <inheritdoc/>
        public bool WasFeatureFoundAtCreation { get; internal set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the information on a specific light group.
        /// </summary>
        /// <param name="groupId">The light group to get the information for.</param>
        /// <returns>
        /// The information on the given light group.  Null the light device is not connected.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of light groups available.
        /// </exception>
        public ILightGroup GetGroupInformation(byte groupId)
        {
            ValidateGroupId(groupId);

            return FeatureDescription?.Groups.ElementAt(groupId);
        }

        #endregion

        #region Object Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine($"Device: {HardwareType}");
            text.AppendLine($"Acquired: {DeviceAcquired}");
            text.AppendLine($"Found at creation: {WasFeatureFoundAtCreation}");

            return text.ToString();
        }

        #endregion
    }
}