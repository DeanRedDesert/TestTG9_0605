//-----------------------------------------------------------------------
// <copyright file = "CsiConfigTypePartial.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone.Schemas
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Partial class for implementing the ICloneable interface for <see cref="VolumeSettingsConfig"/>.
    /// </summary>
    public partial class VolumeSettingsConfig : ICloneable
    {
        /// <inheritdoc />
        public object Clone()
        {
            return new VolumeSettingsConfig
            {
                VolumePlayerSelectable = VolumePlayerSelectable,
                VolumePlayerMuteSelectable = VolumePlayerMuteSelectable,
                MuteAll = MuteAll,
            };
        }
    }

    /// <summary>
    /// Partial class for implementing the ICloneable interface for <see cref="MachineActivityConfig"/>.
    /// </summary>
    public partial class MachineActivityConfig : ICloneable
    {
        /// <inheritdoc />
        public object Clone()
        {
            return new MachineActivityConfig
            {
                NewGame = NewGame,
                AttractInterval = AttractInterval,
                InActivityDelay = InActivityDelay,
                AttractsEnabled = AttractsEnabled,
            };
        }
    }

    /// <summary>
    /// Partial class for implementing the ICloneable interface for <see cref="MonitorSettingsConfig"/>.
    /// </summary>
    public partial class MonitorSettingsConfig : ICloneable
    {
        /// <inheritdoc />
        public object Clone()
        {
            if(Monitors != null)
            {
                var monitorCopy = new List<MonitorType>();
                foreach(var monitor in Monitors)
                {
                    if(monitor == null)
                    {
                        monitorCopy.Add(null);
                        continue;
                    }
                    var monitorType = new MonitorType
                    {
                        DeviceId = monitor.DeviceId,
                        Role = monitor.Role,
                        Style = monitor.Style,
                        Aspect = monitor.Aspect,
                        Model = monitor.Model != null
                            ? new MonitorModelType
                            {
                                Manufacturer = monitor.Model.Manufacturer,
                                Model = monitor.Model.Model,
                                Version = monitor.Model.Version
                            }
                            : null,
                        DesktopCoordinates = monitor.DesktopCoordinates != null
                            ? new DesktopRectangleType
                            {
                                x = monitor.DesktopCoordinates.x,
                                y = monitor.DesktopCoordinates.y,
                                w = monitor.DesktopCoordinates.w,
                                h = monitor.DesktopCoordinates.h
                            }
                            : null,
                        VirtualX = monitor.VirtualX,
                        VirtualY = monitor.VirtualY,
                        ColorProfileId = monitor.ColorProfileId
                    };
                    monitorCopy.Add(monitorType);
                }
                return new MonitorSettingsConfig
                {
                    Monitors = monitorCopy
                };
            }
            return new MonitorSettingsConfig();
        }
    }

    /// <summary>
    /// Partial class for implementing the ICloneable interface for <see cref="ButtonPanelSettingsConfig"/>.
    /// </summary>
    public partial class ButtonPanelSettingsConfig : ICloneable
    {
        /// <inheritdoc />
        public object Clone()
        {
            if(ButtonPanels != null)
            {
                var buttonPanelsCopy = new List<ButtonPanel>();
                foreach(var buttonPanel in ButtonPanels)
                {
                    if(buttonPanel == null)
                    {
                        buttonPanelsCopy.Add(null);
                        continue;
                    }
                    var buttonsCopy = new List<Button>();
                    if(buttonPanel.Buttons != null)
                    {
                        foreach(var button in buttonPanel.Buttons)
                        {
                            if(button == null)
                            {
                                buttonsCopy.Add(null);
                                continue;
                            }
                            var buttonFunctionsCopy = new List<ButtonFunction>();
                            if(button.ButtonFunctions != null)
                            {
                                buttonFunctionsCopy.AddRange(button.ButtonFunctions);
                            }
                            var buttonId = new ButtonId
                            {
                                Value = button.ButtonId.Value
                            };
                            var buttonCopy = new Button
                            {
                                ButtonId = buttonId,
                                ButtonType = button.ButtonType,
                                ButtonFunctions = buttonFunctionsCopy,
                            };
                            buttonsCopy.Add(buttonCopy);
                        }
                    }
                    var buttonPanelCopy = new ButtonPanel
                    {
                        PanelID = buttonPanel.PanelID,
                        PanelIDSpecified = buttonPanel.PanelIDSpecified,
                        PanelLocation = buttonPanel.PanelLocation,
                        PanelType = buttonPanel.PanelType,
                        Buttons = buttonsCopy,
                    };
                    buttonPanelsCopy.Add(buttonPanelCopy);
                }
                return new ButtonPanelSettingsConfig
                {
                    ButtonPanels = buttonPanelsCopy
                };
            }
            return new ButtonPanelSettingsConfig();
        }
    }

    /// <summary>
    /// Partial class for implementing the ICloneable interface for <see cref="ServiceSettingsConfig"/>.
    /// </summary>
    public partial class ServiceSettingsConfig : ICloneable
    {
        /// <inheritdoc />
        public object Clone()
        {
            return new ServiceSettingsConfig
            {
                promptPlayerOnCashoutField = PromptPlayerOnCashout,
                emulatableButtonsField = EmulatableButtons == null ? null : new List<ServiceSettingsConfigEmulatableButton> (EmulatableButtons) 
            };
        }
    }
}
