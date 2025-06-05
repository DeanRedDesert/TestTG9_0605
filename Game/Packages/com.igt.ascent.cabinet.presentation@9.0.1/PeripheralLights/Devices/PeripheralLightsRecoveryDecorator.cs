//-----------------------------------------------------------------------
// <copyright file = "PeripheralLightsRecoveryDecorator.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas.Internal;

    /// <summary>
    /// Implements the peripheral lights interface and adds a recovery feature. When a device is recovered
    /// the last command sent to the groups on the device is re-sent.
    /// </summary>
    internal class PeripheralLightsRecoveryDecorator : IPeripheralLights, IDeviceRecovery
    {
        private readonly Dictionary<string, Dictionary<byte, ILightCommand>> lastCommand;
        private readonly IPeripheralLights peripheralLights;
        private readonly ILightDeviceInquiry lightDeviceInquiry;
        private const string ButtonEdgeLightFeatureId = " Dynamic Button Light Bezel";

        /// <summary>
        /// Construct a new instance given the light interface and the device list.
        /// </summary>
        /// <param name="lightsInterface">The peripheral lights interface.</param>
        /// <param name="inquiryInterface">The light device inquiry interface.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="lightsInterface"/> or <paramref name="inquiryInterface"/> is null.
        /// </exception>
        public PeripheralLightsRecoveryDecorator(IPeripheralLights lightsInterface,
                                                 ILightDeviceInquiry inquiryInterface)
        {
            lastCommand = new Dictionary<string, Dictionary<byte, ILightCommand>>();
            peripheralLights = lightsInterface ?? throw new ArgumentNullException(nameof(lightsInterface));
            lightDeviceInquiry = inquiryInterface ?? throw new ArgumentNullException(nameof(inquiryInterface));
        }

        /// <inheritdoc />
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the group ID being recovered is not valid for the device.
        /// </exception>
        public void RecoverDevice(string featureId)
        {
            if(!lastCommand.ContainsKey(featureId))
            {
                return;
            }

            var groups = lastCommand[featureId];
            foreach(var group in groups.OrderByDescending(pair => pair.Key))
            {
                try
                {
                    RestoreCommand(featureId, group.Key, group.Value);
                }
                catch(LightCategoryException ex)
                {
                    if(ex.ErrorCode != LightErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE.ToString())
                    {
                        if(ex.ErrorCode == LightErrorCode.INVALID_GROUP.ToString())
                        {
                            // The reason the button edge light feature ID is excluded from this exception is that
                            // when unplugging and plugging in the panel the Foundation will send the data for the VBP
                            // which can cause the number of light groups to fluctuate. This can cause a crash due to a race
                            // condition in the Foundation. Thus for this device, just ignore the groups that couldn't
                            // be recovered.
                            if(featureId != ButtonEdgeLightFeatureId)
                            {
                                throw new InvalidOperationException(
                                    $"Recovery was attempted on {featureId} but the device reported that group ID {@group.Key} was not valid.", ex);
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

            }
        }

        #region IPeripheralLights Members

        /// <inheritdoc />
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            return peripheralLights.GetLightDevices();
        }

        /// <inheritdoc />
        public bool RequiresDeviceAcquisition => false;

        /// <inheritdoc />
        public void TurnOffGroup(string featureId, byte groupNumber, TransitionMode transitionMode)
        {
            var command = new LightCommand<TransitionMode>(Command.TurnOffGroup, transitionMode);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.TurnOffGroup(featureId, groupNumber, transitionMode);
            }
        }

        /// <inheritdoc />
        public void ControlLightsMonochrome(string featureId, byte groupNumber, IEnumerable<MonochromeLightState> lightStates)
        {
            var states = lightStates.ToList();
            var command = new LightCommand<IEnumerable<MonochromeLightState>>(Command.ControlLightsMonochrome, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.ControlLightsMonochrome(featureId, groupNumber, states);
            }
        }

        /// <inheritdoc />
        public void ControlLightsMonochrome(string featureId, byte groupNumber, ushort startingLight, IEnumerable<byte> brightnesses)
        {
            var states = brightnesses.ToList();
            var command = new LightCommand<ushort, IEnumerable<byte>>(Command.ControlLightsMonochrome, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.ControlLightsMonochrome(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void ControlLightsRgb(string featureId, byte groupNumber, IEnumerable<RgbLightState> lightStates)
        {
            var states = lightStates.ToList();
            var command = new LightCommand<IEnumerable<RgbLightState>>(Command.ControlLightsRgb, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.ControlLightsRgb(featureId, groupNumber, states);
            }
        }

        /// <inheritdoc />
        public void ControlLightsRgb(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors)
        {
            var states = colors.ToList();
            var command = new LightCommand<ushort, IEnumerable<Rgb16>>(Command.ControlLightsRgb, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.ControlLightsRgb(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void StartSequence(string featureId, byte groupNumber, uint sequenceNumber, TransitionMode transitionMode, byte[] parameters)
        {
            var command = new LightCommand<uint, TransitionMode, byte[]>(Command.StartSequence, sequenceNumber,
                transitionMode, parameters);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.StartSequence(featureId, groupNumber, sequenceNumber, transitionMode, parameters);
            }
        }

        /// <inheritdoc />
        public bool IsSequenceRunning(string featureId, byte groupNumber, uint sequenceNumber)
        {
            return IsDeviceAcquired(featureId) && peripheralLights.IsSequenceRunning(featureId, groupNumber, sequenceNumber);
        }

        /// <inheritdoc />
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, IEnumerable<bool> lightStates)
        {
            var states = lightStates.ToList();
            var command = new LightCommand<ushort, IEnumerable<bool>>(Command.BitwiseLightControl, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.BitwiseLightControl(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, IEnumerable<BitwiseLightIntensity> lightIntensities)
        {
            var states = lightIntensities.ToList();
            var command = new LightCommand<ushort, IEnumerable<BitwiseLightIntensity>>(Command.BitwiseLightControl, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.BitwiseLightControl(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, IEnumerable<BitwiseLightColor> lightColors)
        {
            var states = lightColors.ToList();
            var command = new LightCommand<ushort, IEnumerable<BitwiseLightColor>>(Command.BitwiseLightControl, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.BitwiseLightControl(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb6> lightColors)
        {
            var states = lightColors.ToList();
            var command = new LightCommand<ushort, IEnumerable<Rgb6>>(Command.BitwiseLightControl, startingLight, states);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.BitwiseLightControl(featureId, groupNumber, startingLight, states);
            }
        }

        /// <inheritdoc />
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, byte bitsPerLight, byte[] lightData)
        {
            var command = new LightCommand<ushort, byte, byte[]>(Command.BitwiseLightControl, startingLight,
                bitsPerLight, lightData);
            SaveCommand(featureId, groupNumber, command);
            if(IsDeviceAcquired(featureId))
            {
                peripheralLights.BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, lightData);
            }
        }

        #endregion

        #region Device Commands

        private enum Command
        {
            TurnOffGroup,
            ControlLightsMonochrome,
            ControlLightsRgb,
            StartSequence,
            BitwiseLightControl
        }

        private interface ILightCommand
        {
            Command Command { get; }
            int ParameterCount { get; }
        }

        private class LightCommand : ILightCommand
        {
            protected LightCommand(Command command)
            {
                Command = command;
            }

            #region ILightCommand Members

            public Command Command { get; }
            public int ParameterCount { get; protected set; }

            #endregion
        }

        private class LightCommand<TParam> : LightCommand
        {
            public LightCommand(Command command, TParam param)
                : base(command)
            {
                Parameter = param;
                ParameterCount = 1;
            }

            public TParam Parameter { get; }
        }

        private class LightCommand<TParam1, TParam2> : LightCommand
        {
            public LightCommand(Command command, TParam1 param1, TParam2 param2)
                : base(command)
            {
                Parameter1 = param1;
                Parameter2 = param2;
                ParameterCount = 2;
            }

            public TParam1 Parameter1 { get; }
            public TParam2 Parameter2 { get; }
        }

        private class LightCommand<TParam1, TParam2, TParam3> : LightCommand
        {
            public LightCommand(Command command, TParam1 param1, TParam2 param2, TParam3 param3)
                : base(command)
            {
                Parameter1 = param1;
                Parameter2 = param2;
                Parameter3 = param3;
                ParameterCount = 3;
            }

            public TParam1 Parameter1 { get; }
            public TParam2 Parameter2 { get; }
            public TParam3 Parameter3 { get; }
        }

        #endregion

        #region Save and Restore Commands

        /// <summary>
        /// Saves a command into the cache.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <param name="groupNumber">The group number on the device.</param>
        /// <param name="command">The command to save.</param>
        private void SaveCommand(string featureId, byte groupNumber, ILightCommand command)
        {
            if(!lastCommand.ContainsKey(featureId))
            {
                lastCommand.Add(featureId, new Dictionary<byte, ILightCommand>());
            }

            var groups = lastCommand[featureId];
            if(groupNumber == UsbLightBase.AllGroups)
            {
                // If the command is for all groups, clear any saved commands for other groups.
                // This will cause it better match the behavior of the device.
                groups.Clear();
            }

            if(!groups.ContainsKey(groupNumber))
            {
                groups.Add(groupNumber, command);
            }
            else
            {
                groups[groupNumber] = command;
            }
        }

        /// <summary>
        /// Restores a command to a feature and group.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <param name="groupNumber">The group number on the device to apply the command.</param>
        /// <param name="command">The command to restore.</param>
        private void RestoreCommand(string featureId, byte groupNumber, ILightCommand command)
        {
            switch(command.Command)
            {
                case Command.TurnOffGroup:
                    {
                        var concreteCommand = (LightCommand<TransitionMode>)command;
                        peripheralLights.TurnOffGroup(featureId, groupNumber, concreteCommand.Parameter);
                    }
                    break;
                case Command.ControlLightsMonochrome:
                    {
                        if(command.ParameterCount == 1)
                        {
                            var concreteCommand = (LightCommand<IEnumerable<MonochromeLightState>>)command;
                            peripheralLights.ControlLightsMonochrome(featureId, groupNumber, concreteCommand.Parameter);
                        }
                        else
                        {
                            var concreteCommand = (LightCommand<ushort, IEnumerable<byte>>)command;
                            peripheralLights.ControlLightsMonochrome(featureId, groupNumber, concreteCommand.Parameter1,
                                concreteCommand.Parameter2);
                        }
                    }
                    break;
                case Command.ControlLightsRgb:
                    {
                        if(command.ParameterCount == 1)
                        {
                            var concreteCommand = (LightCommand<IEnumerable<RgbLightState>>)command;
                            peripheralLights.ControlLightsRgb(featureId, groupNumber, concreteCommand.Parameter);
                        }
                        else
                        {
                            var concreteCommand = (LightCommand<ushort, IEnumerable<Rgb16>>)command;
                            peripheralLights.ControlLightsRgb(featureId, groupNumber, concreteCommand.Parameter1,
                                concreteCommand.Parameter2);
                        }
                    }
                    break;
                case Command.StartSequence:
                    {
                        var concreteCommand = (LightCommand<uint, TransitionMode, byte[]>)command;
                        peripheralLights.StartSequence(featureId, groupNumber, concreteCommand.Parameter1,
                            concreteCommand.Parameter2, concreteCommand.Parameter3);
                    }
                    break;
                case Command.BitwiseLightControl:
                    {
                        if(command.ParameterCount == 3)
                        {
                            var concreteCommand = (LightCommand<ushort, byte, byte[]>)command;
                            peripheralLights.BitwiseLightControl(featureId, groupNumber, concreteCommand.Parameter1,
                                concreteCommand.Parameter2, concreteCommand.Parameter3);
                        }
                        else
                        {
                            switch(command)
                            {
                                case LightCommand<ushort, IEnumerable<bool>> concreteCommand1:
                                    peripheralLights.BitwiseLightControl(featureId, groupNumber, concreteCommand1.Parameter1,
                                        concreteCommand1.Parameter2);
                                    break;
                                case LightCommand<ushort, IEnumerable<BitwiseLightIntensity>> concreteCommand2:
                                    peripheralLights.BitwiseLightControl(featureId, groupNumber, concreteCommand2.Parameter1,
                                        concreteCommand2.Parameter2);
                                    break;
                                case LightCommand<ushort, IEnumerable<BitwiseLightColor>> concreteCommand:
                                    peripheralLights.BitwiseLightControl(featureId, groupNumber, concreteCommand.Parameter1,
                                        concreteCommand.Parameter2);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Determines if a device is acquired given its feature name/ID.
        /// </summary>
        /// <param name="featureId">The feature ID to lookup.</param>
        /// <returns>True if the device is acquired, false if it is not.</returns>
        private bool IsDeviceAcquired(string featureId)
        {
            return lightDeviceInquiry.IsDeviceAcquired(featureId);
        }
    }
}
