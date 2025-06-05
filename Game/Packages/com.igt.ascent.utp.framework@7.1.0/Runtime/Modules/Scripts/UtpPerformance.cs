// -----------------------------------------------------------------------
//  <copyright file = "UtpPerformance.cs" company = "IGT">
//      Copyright (c) 2016-2019 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework;
    using Framework.Communications;
    using ModuleComponents;

    /// <summary>
    /// This class utilizes the UtpDiagnostics class
    /// </summary>
    [ModuleEvent("PerformanceUpdate", "float CurrentFps, float AverageFps, float TotalAllocatedMemory, float TotalReservedMemory, float TotalAllocatedManagedMemory",
        "Event occurs when the performance metrics have updated.")]
    [ModuleEvent("LowFrameRate", "float CurrentFrameRate, float AverageFrameRate", "Fires when the frame rate goes below a certain threshold.")]
    [ModuleEvent("LowSystemMemory", "float TotalAllocatedMemory", "Fires when allocated memory has exceeded 1GB threshold.")]
    public class UtpPerformance : AutomationModule
    {
        #region Fields

        private UtpDiagnostics utpDiagnosticsBackingField;

        private UtpDiagnostics utpDiagnostics
        {
            get
            {
                return utpDiagnosticsBackingField ??
                       (utpDiagnosticsBackingField = UtpController.GetComponent<UtpDiagnostics>() ?? UtpController.gameObject.AddComponent<UtpDiagnostics>());
            }
        }

        #endregion Fields

        #region AutomationModule Overrides

        public override string Name
        {
            get { return "Performance"; }
        }

        public override Version ModuleVersion
        {
            get { return new Version(1, 2, 1, 0); }
        }

        public override bool Initialize()
        {
            try
            {
                if(utpDiagnostics == null)
                {
                    return false;
                }

                RegisterEventHandlers();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Dispose()
        {
            UnregisterEventHandlers();
        }

        #endregion AutomationModule Overrides

        #region Module Commands

        [ModuleCommand("ConfigureDiagnostics", "bool Success", "Reconfigure diagnostics polling values",
            new[]
            {
                "UpdateFrequency|float|Frequency (in sec) to poll.; default is '0.2f'",
                "LowFrameRateThreshold|float|Low framerate threshold. If FPS drops below this value an event will be fired.; default is '30.0f'",
                "SystemMemoryThreshold|float|Low system memory threshold. If system memory drops below this value an event will be fired.; default is '1000.0f'"
            })]
        public bool ConfigureDiagnostics(AutomationCommand command, IUtpCommunication sender)
        {
            var errors = AutomationParameter.GetIncomingParameterValidationErrors(command.Parameters,
                new List<string> { "UpdateFrequency", "LowFrameRateThreshold", "SystemMemoryThreshold" });

            if(!string.IsNullOrEmpty(errors))
            {
                SendErrorCommand(command.Command, errors, sender);
                return false;
            }

            if(utpDiagnostics == null)
            {
                SendErrorCommand(command.Command, "UTP's Diagnostics object was not found");
                return false;
            }

            var parameters = AutomationParameter.GetParameterDictionary(command);
            var updateFrequencyValue = parameters["UpdateFrequency"].FirstOrDefault();
            var lowFrameRateThresholdValue = parameters["LowFrameRateThreshold"].FirstOrDefault();
            var systemMemoryThresholdValue = parameters["SystemMemoryThreshold"].FirstOrDefault();

            float updateFrequency, lowFrameRateThreshold, systemMemoryThreshold;
            updateFrequency = float.TryParse(updateFrequencyValue, out updateFrequency) ? updateFrequency : 0.2f;
            lowFrameRateThreshold = float.TryParse(lowFrameRateThresholdValue, out lowFrameRateThreshold) ? lowFrameRateThreshold : 30.0f;
            systemMemoryThreshold = float.TryParse(systemMemoryThresholdValue, out systemMemoryThreshold) ? systemMemoryThreshold : 1000.0f;

            utpDiagnostics.UpdateFrequency = updateFrequency;
            utpDiagnostics.LowFrameRateThreshold = lowFrameRateThreshold;
            utpDiagnostics.SystemMemoryThreshold = systemMemoryThreshold;

            return SendCommand(command.Command, new List<AutomationParameter> { new AutomationParameter("Success", true.ToString(), "bool") }, sender);
        }

        /// <summary>
        /// Enables or disables diagnostic polling in UtpDiagnostics. When disabled 
        /// </summary>
        /// <param name="command">The incoming command. Requires: Enable</param>
        /// <param name="sender">The sender of the command. Returns: Success:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("EnableDiagnosticPolling", "bool Success", "Enable/Disable diagnostic polling for FPS counter and memory usage alerts",
            new[] { "Enable|bool|Sets the enabled state; default is 'true'" })]
        public bool EnableDiagnosticPolling(AutomationCommand command, IUtpCommunication sender)
        {
            var enableValue = AutomationParameter.GetParameterValues(command, "Enable").FirstOrDefault();
            bool enabled;

            if(string.IsNullOrEmpty(enableValue))
            {
                enabled = true;
            }
            else if(!bool.TryParse(enableValue, out enabled))
            {
                SendErrorCommand(command.Command,
                    string.Format("Parameter {0} must be boolean, couldn't parse {1}", "Enable", enableValue),
                    sender);
                return false;
            }

            utpDiagnostics.enabled = enabled;

            return SendCommand(command.Command,
                new List<AutomationParameter>
                {
                    new AutomationParameter("Success",
                        (enabled == utpDiagnostics.enabled ? "true" : "false"),
                        "bool",
                        "Polling set correctly")
                },
                sender);
        }

        /// <summary>
        /// Gets all metrics data
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetAllMetrics", "float CurrentFps, float AverageFps, float TotalAllocatedMemory, float TotalReservedMemory, float TotalAllocatedManagedMemory", "Gets all metrics data")]
        public bool GetAllMetrics(AutomationCommand command, IUtpCommunication sender)
        {
            return SendCommand(command.Command, GetAllMetricsParameters(), sender);
        }

        /// <summary>
        /// Gets the current frame rate (in frames per second).
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetCurrentFrameRate", "float CurrentFps", "Gets the current frame rate (in FPS) of the game.")]
        public bool GetCurrentFrameRate(AutomationCommand command, IUtpCommunication sender)
        {
            var param1 = new AutomationParameter("CurrentFps", utpDiagnostics.FramesPerSecond.ToString(), "float", "Gets the current frame rate (FPS).");
            var paramsList = new List<AutomationParameter> { param1 };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Gets the average frame rate (in frames per second).
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetAverageFrameRate", "float AverageFps", "Gets the average frame rate (in FPS) of the game.")]
        public bool GetAverageFrameRate(AutomationCommand command, IUtpCommunication sender)
        {
            var param1 = new AutomationParameter("AverageFps", utpDiagnostics.AverageFramesPerSecond.ToString(), "float", "The average frame rate (FPS).");
            var paramsList = new List<AutomationParameter> { param1 };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Gets the total allocated RAM by the application (in MB).
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetTotalAllocatedMemory", "float TotalAllocatedMemory", "Gets the amount of RAM allocated by the application (in MB).")]
        public bool GetTotalAllocatedMemory(AutomationCommand command, IUtpCommunication sender)
        {
            var param1 = new AutomationParameter("TotalAllocatedMemory", utpDiagnostics.TotalAllocatedMemory.ToString(), "float", "The amount of RAM allocated by the application (MB).");
            var paramsList = new List<AutomationParameter> { param1 };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Gets the total allocated managed RAM by the application (in MB).
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetTotalAllocatedManagedMemory", "float TotalAllocatedManagedMemory", "Gets the total allocated managed RAM by the application (in MB).")]
        public bool GetTotalAllocatedManagedMemory(AutomationCommand command, IUtpCommunication sender)
        {
            var param1 = new AutomationParameter("TotalAllocatedManagedMemory", utpDiagnostics.TotalAllocatedManagedMemory, "float", "The amount of managed RAM allocated by the application (MB).");
            var paramsList = new List<AutomationParameter> { param1 };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Gets the total reserved RAM by the application (in MB).
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("GetTotalReservedMemory", "float TotalReservedMemory", "Gets the amount of RAM reserved by the application (in MB).")]
        public bool GetTotalReservedMemory(AutomationCommand command, IUtpCommunication sender)
        {
            var param1 = new AutomationParameter("TotalReservedMemory", utpDiagnostics.TotalReservedMemory.ToString(), "float", "The amount of RAM reserved by the application (MB).");
            var paramsList = new List<AutomationParameter> { param1 };
            return SendCommand(command.Command, paramsList, sender);
        }

        #endregion Module Commands

        #region Private Methods

        private List<AutomationParameter> GetAllMetricsParameters()
        {
            var paramList = new List<AutomationParameter>
            {
                new AutomationParameter("CurrentFps", utpDiagnostics.FramesPerSecond.ToString(), "float", "Gets the current frame rate (FPS)."),
                new AutomationParameter("AverageFps", utpDiagnostics.AverageFramesPerSecond.ToString(), "float", "The average frame rate (FPS)."),
                new AutomationParameter("TotalAllocatedMemory", utpDiagnostics.TotalAllocatedMemory.ToString(), "float", "The amount of RAM allocated by the application (MB)."),
                new AutomationParameter("TotalAllocatedManagedMemory", utpDiagnostics.TotalAllocatedManagedMemory, "float", "The amount of managed RAM allocated by the application (MB)."),
                new AutomationParameter("TotalReservedMemory", utpDiagnostics.TotalReservedMemory.ToString(), "float", "The amount of RAM reserved by the application (MB).")
            };
            return paramList;
        }

        private void RegisterEventHandlers()
        {
            UnregisterEventHandlers();

            utpDiagnostics.UpdateAvailable += UtpDiagnosticsOnUpdateAvailable;
            utpDiagnostics.LowFrameRate += UtpDiagnosticsLowFrameRate;
            utpDiagnostics.LowSystemMemory += UtpDiagnosticsLowSystemMemory;
        }

        private void UnregisterEventHandlers()
        {
            utpDiagnostics.UpdateAvailable -= UtpDiagnosticsOnUpdateAvailable;
            utpDiagnostics.LowFrameRate -= UtpDiagnosticsLowFrameRate;
            utpDiagnostics.LowSystemMemory -= UtpDiagnosticsLowSystemMemory;
        }

        #endregion Private Methods

        #region Event Handling

        /// <summary>
        /// Sends event when performance metrics are updated.
        /// </summary>
        private void UtpDiagnosticsOnUpdateAvailable(object sender, EventArgs eventArgs)
        {
            SendEvent("PerformanceUpdate", GetAllMetricsParameters());
        }

        /// <summary>
        /// Handles the the LowFrameRate event & sends it in a ModuleEvent command.
        /// </summary>
        private void UtpDiagnosticsLowFrameRate(object sender, EventArgs e)
        {
            var param1 = new AutomationParameter("CurrentFrameRate", utpDiagnostics.FramesPerSecond.ToString(), "float", "The frame rate (FPS), captured after the event was triggered in the game.");
            var param2 = new AutomationParameter("AverageFrameRate", utpDiagnostics.AverageFramesPerSecond.ToString(), "float", "The average frame rate (FPS), captured after the event was triggered in the game.");
            var paramsList = new List<AutomationParameter> { param1, param2 };
            SendEvent("LowFrameRate", paramsList);
        }

        /// <summary>
        /// Handles the TotalAlloctedMemory event & sends it in a ModuleEvent command.
        /// </summary>
        private void UtpDiagnosticsLowSystemMemory(object sender, EventArgs e)
        {
            var param1 = new AutomationParameter("TotalAllocatedMemory", utpDiagnostics.TotalAllocatedMemory.ToString(), "float", "The amount of RAM allocated (MB), captured after the event was triggered in the game.");
            var paramsList = new List<AutomationParameter> { param1 };
            SendEvent("LowSystemMemory", paramsList);
        }

        #endregion Event Handling
    }
}