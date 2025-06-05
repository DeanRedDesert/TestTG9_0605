// -----------------------------------------------------------------------
// <copyright file = "UtpGameSpeed.cs" company = "IGT">
//     Copyright © 2015-2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Communications;
    using Framework;
    using UnityEngine;

    public class UtpGameSpeed : AutomationModule
    {
        #region AutomationModule Overrides

        public override Version ModuleVersion
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public override string Name
        {
            get { return "Game Speed"; }
        }

        public override bool Initialize()
        {
            return true;
        }

        #endregion AutomationModule Overrides

        #region Module Commands

        [ModuleCommand("SetTimeScale", "float TimeScale", "Sets the game's timescale",
            new[] { "TimeScale|float|The timescale to set the game to, 1.0f is default speed, max of 10.0f; default is '1.0f'" })]
        public bool SetTimeScale(AutomationCommand command, IUtpCommunication sender)
        {
            string timeScaleValue = AutomationParameter.GetParameterValues(command, "TimeScale").FirstOrDefault();

            float timeScale;
            if(!float.TryParse(timeScaleValue, out timeScale))
                timeScale = 1.0f;

            // Setting a max because I've noticed some issues with the template game if it goes too fast (couldn't play more than 1 game)
            timeScale = Math.Min(timeScale, 10f);
            Time.timeScale = timeScale;

            return SendCommand(command.Command,
                new List<AutomationParameter> { new AutomationParameter("TimeScale", Time.timeScale.ToString(), "float", "The current timescale") },
                sender);
        }

        [ModuleCommand("GetTimeScale", "float TimeScale", "Gets the current timescale")]
        public bool GetTimeScale(AutomationCommand command, IUtpCommunication sender)
        {
            return SendCommand(command.Command,
                new List<AutomationParameter> { new AutomationParameter("TimeScale", Time.timeScale.ToString(), "float", "The current timescale") },
                sender);
        }

        #endregion Module Commands
    }
}