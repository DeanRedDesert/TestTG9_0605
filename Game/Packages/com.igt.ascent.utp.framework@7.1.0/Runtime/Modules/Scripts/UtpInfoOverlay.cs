// -----------------------------------------------------------------------
//  <copyright file = "UtpInfoOverlay.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
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
    using IgtUnityEngine;
    using UnityEngine;

    public class UtpInfoOverlay : AutomationModule
    {
        #region Fields

        private UtpInfoOverlayController overlayController;

        #endregion Fields

        #region AutomationModule Overrides

        public override Version ModuleVersion
        {
            get { return new Version(1, 2, 0, 0); }
        }

        public override string Name
        {
            get { return "Info Overlay"; }
        }

        public override bool Initialize()
        {
            try
            {
                overlayController = UtpInfoOverlayController.GetInstance();
                if(overlayController != null)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion AutomationModule Overrides

        #region Module Commands

        /// <summary>
        /// Adds an information overlay item to the InfoOverlayController
        /// </summary>
        /// <param name="command">The incoming command. Requires: ID:Text, Group:Text, Type:Text, XPos:Float, YPos:Float, Width:Float, Height:Float, Text:Text, Visible:Bool</param>
        /// <param name="sender">The sender of the command. Returns: ID:Text, Group:Text</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("AddOverlayItem", "string ID",
            "Adds an information overlay item to the game", new[]
            {
                "ID|Text|Unique ID, used to show/hide/remove",
                "Text|Text|Text to display in the item",
                "Type|Text|Choose from: Box, Label, Button, RepeatButton, TextField, TextArea, Toggle, Toolbar, SelectionGrid, HorizontalSlider, VerticalSlider, HorizontalScrollbar, VerticalScrollbar, ScrollView, Window",
                "Group|Text|Group to add the item to; default is 'Default'",
                "XPos|Float|X coordinate; default is '0'",
                "YPos|Float|Y coordinate; default is '0'",
                "Width|Float|Width of container; default is '200'",
                "Height|Float|Height of container; default is '100'",
                "Visible|Bool|Make item visible; default is 'true'",
                "Time|Float|Time (in seconds) to display the overlay item; default is '-1'"
            })]
        public bool AddOverlayItem(AutomationCommand command, IUtpCommunication sender)
        {
            #region Read parameters

            var parameters = AutomationParameter.GetParameterDictionary(command);

            string id = parameters["ID"].FirstOrDefault();
            string text = parameters["Text"].FirstOrDefault();

            InfoOverlayItem.InfoType type;
            try
            {
                type = (InfoOverlayItem.InfoType)Enum.Parse(typeof(InfoOverlayItem.InfoType), parameters["Type"].First(), true);
            }
            catch(Exception)
            {
                return SendErrorCommand(command.Command,
                    parameters["Type"].FirstOrDefault() + " is not a valid InfoOverLayItem type",
                    sender);
            }

            string group = parameters["Group"].FirstOrDefault() ?? "Default";

            float xPos, yPos;
            if(!float.TryParse(parameters["XPos"].FirstOrDefault(), out xPos))
                xPos = 0;
            if(!float.TryParse(parameters["YPos"].FirstOrDefault(), out yPos))
                yPos = 0;

            float width, height;
            if(!float.TryParse(parameters["Width"].FirstOrDefault(), out width))
                width = 200;
            if(!float.TryParse(parameters["Height"].FirstOrDefault(), out height))
                height = 100;

            Rect position = new Rect(xPos, yPos, width, height);
            bool visible;
            if(!parseBool(parameters["Visible"].FirstOrDefault(), out visible, true))
                return SendErrorCommand(command.Command, "Couldn't parse " + parameters["Visible"].FirstOrDefault() + " as bool", sender);

            float displayTime;
            if(!float.TryParse(parameters["Time"].FirstOrDefault(), out displayTime))
                displayTime = -1;

            #endregion

            string newId = overlayController.AddItem(new InfoOverlayItem(id, group, type, position, text, visible, displayTime));

            var paramsList = new List<AutomationParameter>
            {
                new AutomationParameter("ID", newId, "Text", "ID of overlay item"),
                new AutomationParameter("Group", group, "Text", "Group the overlay item was added to")
            };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">The incoming command. Requires: Text:Text, Time:Float = 10</param>
        /// <param name="sender">The sender of the command. Returns: ID: Text</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("ShowPrompt", "string ID",
            "Shows a prompt in the center of the screen", new[]
            {
                "Text|Text|Text to display in the prompt",
                "Time|Float|Time (in seconds) to display the prompt; default is '10'"
            })]
        public bool ShowPrompt(AutomationCommand command, IUtpCommunication sender)
        {
            // Read the parameters
            var parameters = AutomationParameter.GetParameterDictionary(command);
            string text = parameters["Text"].FirstOrDefault();
            float displayTime;
            if(!float.TryParse(parameters["Time"].FirstOrDefault(), out displayTime))
                displayTime = 10;

            // Create and add the prompt item
            var cameras = FindObjectsOfType<Camera>();
            //var mainCamera = cameras.FirstOrDefault(c => c.monitorAssociation == CameraMonitorAssociation.MLDFront);
            var mainCamera = cameras.FirstOrDefault(c => c.targetDisplay == (int)MonitorRole.Main);
            if(mainCamera == null)
                mainCamera = Camera.main;

            var promptItem = overlayController.CreatePromptItem(text, mainCamera, displayTime);
            promptItem.Visible = true;
            string id = overlayController.AddItem(promptItem);

            var paramsList = new List<AutomationParameter> { new AutomationParameter("ID", id, "Text", "ID of the prompt that was added") };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Sets a specific overlay item's visibility
        /// </summary>
        /// <param name="command">The incoming command. Requires: Name:Text, Visible:Bool</param>
        /// <param name="sender">The sender of the command. Returns: Name:Text, Visible:Bool, Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("Show", "bool Result", "Sets a specific overlay item's visibility", new[]
        {
            "ID|Text|Item name to show or hide",
            "Visible|Bool|Set the item's visibility; default is 'true'",
            "Time|Float|Time (in seconds) to display the overlay item; default is '-1'"
        })]
        public bool Show(AutomationCommand command, IUtpCommunication sender)
        {
            var parameters = AutomationParameter.GetParameterDictionary(command);

            string itemName = parameters["ID"].FirstOrDefault();
            bool visible;
            if(!parseBool(parameters["Visible"].FirstOrDefault(), out visible, true))
                return SendErrorCommand(command.Command, "Couldn't parse " + parameters["Visible"].FirstOrDefault() + " as bool", sender);

            float displayTime;
            if(!float.TryParse(parameters["Time"].FirstOrDefault(), out displayTime))
                displayTime = -1;

            bool result = overlayController.Show(itemName, visible, displayTime);
            var paramsList = new List<AutomationParameter>
            {
                new AutomationParameter("ID", itemName, "Text", "Item name to show or hide"),
                new AutomationParameter("Visible", visible.ToString(), "Bool", "Item's visibility"),
                new AutomationParameter("Result", result.ToString(), "Bool", "Was item able to show or hide")
            };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Sets a group of overlay item's visibility
        /// </summary>
        /// <param name="command">The incoming command. Requires: Group:Text, Visible:Bool</param>
        /// <param name="sender">The sender of the command. Returns: Group:Text, Visible:Bool, Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("ShowGroup", "bool Result", "Sets a group of overlay item's visibility", new[]
        {
            "Group|Text|Group to show or hide",
            "Visible|Bool|Set the group's visibility; default is 'true'",
            "Time|Float|Time (in seconds) to display the overlay item; default is '-1'"
        })]
        public bool ShowGroup(AutomationCommand command, IUtpCommunication sender)
        {
            var parameters = AutomationParameter.GetParameterDictionary(command);

            string group = parameters["Group"].FirstOrDefault();
            bool visible;
            if(!parseBool(parameters["Visible"].FirstOrDefault(), out visible, true))
                return SendErrorCommand(command.Command, "Couldn't parse " + parameters["Visible"].FirstOrDefault() + " as bool", sender);

            float displayTime;
            if(!float.TryParse(parameters["Time"].FirstOrDefault(), out displayTime))
                displayTime = -1;

            bool result = overlayController.ShowGroup(group, visible, displayTime);
            var paramsList = new List<AutomationParameter> { new AutomationParameter("Group", @group, "Text", "Group to show or hide"),
                new AutomationParameter("Visible", visible.ToString(), "Bool", "Item's visibility"),
                new AutomationParameter("Result", result.ToString(), "Bool", "Was group able to show or hide") };
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// Shows or hides all overlay items
        /// </summary>
        /// <param name="command">The incoming command. Requires: Visible:Bool</param>
        /// <param name="sender">The sender of the command. Returns: Visible:Bool, Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("ShowAll", "bool Result", "Shows or hides all overlay items", new[]
        {
            "Visible|Bool|Set all info overlay items visibility; default is 'true'",
            "Time|Float|Time (in seconds) to display the overlay item; default is '-1'"
        })]
        public bool ShowAll(AutomationCommand command, IUtpCommunication sender)
        {
            var parameters = AutomationParameter.GetParameterDictionary(command);

            bool visible;
            if(!parseBool(parameters["Visible"].FirstOrDefault(), out visible, true))
                return SendErrorCommand(command.Command, "Couldn't parse " + parameters["Visible"].FirstOrDefault() + " as bool", sender);

            float displayTime;
            if(!float.TryParse(parameters["Time"].FirstOrDefault(), out displayTime))
                displayTime = -1;

            bool result = overlayController.ShowAll(visible, displayTime);
            var paramList = new List<AutomationParameter>
            {
                new AutomationParameter("Visible", visible.ToString(), "Bool", "New item visibility"),
                new AutomationParameter("Result", result.ToString(), "Bool", "Was able to show or hide")
            };
            return SendCommand(command.Command, paramList, sender);
        }

        /// <summary>
        /// Removes an overlay item from available items
        /// </summary>
        /// <param name="command">The incoming command. Requires: ID:Text</param>
        /// <param name="sender">The sender of the command. Returns: ID:Text, Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("RemoveItem", "bool Result", "Removes an overlay item from available items", new[]
        {
            "ID|Text|Information overlay item ID to remove"
        })]
        public bool RemoveItem(AutomationCommand command, IUtpCommunication sender)
        {
            var parameters = AutomationParameter.GetParameterDictionary(command);
            string item = parameters["ID"].FirstOrDefault();

            bool result = overlayController.RemoveItem(item);
            var paramList = new List<AutomationParameter>
            {
                new AutomationParameter("ID", item, "Text", "Information overlay item ID to remove"),
                new AutomationParameter("Result", result.ToString(), "Bool", "Was able to remove item")
            };
            return SendCommand(command.Command, paramList, sender);
        }

        /// <summary>
        /// Removes an overlay group from available items
        /// </summary>
        /// <param name="command">The incoming command. Requires: Group:Text</param>
        /// <param name="sender">The sender of the command. Returns: Group:Text, Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("RemoveGroup", "string Result", "Removes an overlay group from available items", new[]
        {
            "Group|Text|Information overlay group to remove"
        })]
        public bool RemoveGroup(AutomationCommand command, IUtpCommunication sender)
        {
            var parameters = AutomationParameter.GetParameterDictionary(command);
            string group = parameters["Group"].FirstOrDefault();

            bool result = overlayController.RemoveGroup(group);
            var paramList = new List<AutomationParameter>
            {
                new AutomationParameter("Group", @group, "Text", "Information overlay group to remove"),
                new AutomationParameter("Result", result.ToString(), "Bool", "Was able to remove group")
            };
            return SendCommand(command.Command, paramList, sender);
        }

        /// <summary>
        /// Removes all overlay items
        /// </summary>
        /// <param name="command">The incoming command. Requires: None</param>
        /// <param name="sender">The sender of the command. Returns: Result:Bool</param>
        /// <returns>Command processed status</returns>
        [ModuleCommand("RemoveAll", "bool Result", "Removes all overlay items")]
        public bool RemoveAll(AutomationCommand command, IUtpCommunication sender)
        {
            bool result = overlayController.RemoveAll();
            var paramList = new List<AutomationParameter> { new AutomationParameter("Result", result.ToString(), "Bool", "Was able to remove all") };
            return SendCommand(command.Command, paramList, sender);
        }

        #endregion Module Commands

        #region Private Methods

        /// <summary>
        /// Parses a boolean string
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="result">Result</param>
        /// <param name="defaultVal">Default value if string is null or empty</param>
        /// <returns>Able to parse or set default value</returns>
        private bool parseBool(string value, out bool result, bool defaultVal = false)
        {
            bool parsedVal;
            if(!bool.TryParse(value, out parsedVal))
            {
                if(String.IsNullOrEmpty(value))
                    result = defaultVal;
                else
                {
                    result = defaultVal;
                    return false;
                }
            }
            else
            {
                result = parsedVal;
            }
            return true;
        }

        #endregion Private Methods
    }
}