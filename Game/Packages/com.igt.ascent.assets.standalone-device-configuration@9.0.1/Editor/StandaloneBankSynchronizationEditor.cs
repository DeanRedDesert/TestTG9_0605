//-----------------------------------------------------------------------
// <copyright file = "StandaloneBankSynchronizationEditor.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Core.Communication.Cabinet.CSI.Schemas;
    using Core.Communication.Cabinet.Standalone;
    using Core.Presentation.BankSynchronization;
    using Core.Presentation.BankSynchronization.GameEvents;
    using Core.Presentation.BankSynchronization.GameEvents.GameMessages;
    using StandaloneDevices;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom editor for the standalone bank synchronization class.
    /// </summary>
    [CustomEditor(typeof(StandaloneBankSynchronization))]
    public class StandaloneBankSynchronizationEditor : Editor
    {
        private const int MinimumNumberOfMachines = 1;
        private const int MaximumNumberOfMachines = 12;

        private static IGameMessage currentMessage;
        private int choiceIndex;
        private int lastChoice;
        private readonly GameMessageSerializer serializer = new GameMessageSerializer();
        private readonly GameMessageHeaderSerializer headerSerializer = new GameMessageHeaderSerializer();
        private string senderThemeId = "Game020001AAABBB";
        private const int MaxGameMessageSizeInBytes = 2048;

        private readonly List<Type> gameMessageTypes = Assembly.GetAssembly(typeof(IGameMessage))
            .GetTypes()
            .Where(
                myType =>
                    myType.IsClass && !myType.IsAbstract &&
                    myType.GetInterfaces().Contains(typeof(IGameMessage))).ToList();

        private static readonly Dictionary<Type, Action<PropertyInfo>> SupportedEditTypes =
            new Dictionary<Type, Action<PropertyInfo>>
            {
                {typeof(float), FloatPropertyEdit},
                {typeof(int), IntPropertyEdit},
                {typeof(bool), BoolPropertyEdit},
                {typeof(string), StringPropertyEdit}
            };

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            var controller = target as StandaloneBankSynchronization;

            if(controller != null)
            {
                IBankSynchronizationSettings synchronizer = controller;
                if(Application.isPlaying)
                {
                    synchronizer = controller.Synchronizer;
                }

                if(synchronizer != null)
                {
                    Undo.RecordObject(controller, "Standalone Bank Synchronization");
                    // Bank Synchronization
                    synchronizer.Enabled = EditorGUILayout.Toggle("Enabled:", synchronizer.Enabled);
                    synchronizer.Precision =
                        (TimeFramePrecisionLevel)
                            EditorGUILayout.EnumPopup("Synchronization Precision:", synchronizer.Precision);
                    var previousTotal = synchronizer.TotalMachinesInBank;
                    var previousPosition = synchronizer.BankPosition;
                    synchronizer.TotalMachinesInBank = (UInt32)EditorGUILayout.IntSlider("Total Machines In Bank: ",
                        (Int32)synchronizer.TotalMachinesInBank,
                        MinimumNumberOfMachines,
                        MaximumNumberOfMachines);
                    if(synchronizer.TotalMachinesInBank > 1)
                    {
                        synchronizer.BankPosition =
                            (UInt32)
                                EditorGUILayout.IntSlider("Bank Position:",
                                    (Int32)synchronizer.BankPosition,
                                    1,
                                    (Int32)synchronizer.TotalMachinesInBank);
                    }

                    // If the position changes, re-register with the stored theme ID. This would normally happen as a
                    // result of setting the cabinet library on un-park.
                    if(previousPosition != synchronizer.BankPosition ||
                       previousTotal != synchronizer.TotalMachinesInBank)
                    {
                        BankSynchronizationController.RegisterForGameEvents(BankSynchronizationController.ThemeId);
                    }

                    // Game Events
                    EditorGUILayout.BeginVertical("Box");

                    var style = new GUIStyle("TextField");
                    senderThemeId =
                        EditorGUILayout.TextField(
                            new GUIContent("Sender Theme ID:", "Theme ID for the simulated message to come from"),
                            senderThemeId,
                            style);
                    EditorGUILayout.BeginVertical("Box");
                    choiceIndex = EditorGUILayout.Popup(choiceIndex,
                        gameMessageTypes.Select(type => type.Name.ToString()).ToArray());

                    // Recreate the game message if it's a new type.
                    if(choiceIndex != lastChoice || currentMessage == null)
                    {
                        currentMessage = Activator.CreateInstance(gameMessageTypes[choiceIndex]) as IGameMessage;
                    }
                    lastChoice = choiceIndex;

                    var properties = gameMessageTypes[choiceIndex].GetProperties();
                    foreach(var prop in properties)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                        if(!prop.CanWrite)
                        {
                            string value = prop.GetValue(currentMessage, null).ToString();
                            EditorGUILayout.LabelField(string.Format("{0} (Read Only):   {1}", prop.Name, value));
                        }
                        else
                        {
                            if(SupportedEditTypes.ContainsKey(prop.PropertyType))
                            {
                                SupportedEditTypes[prop.PropertyType](prop);
                            }
                            else
                            {
                                EditorGUILayout.LabelField(
                                    new GUIContent(string.Format("{0} (Editing this type not supported)", prop.Name)),
                                    new GUIStyle {normal = {textColor = Color.red}});
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    if(GUILayout.Button("Force Receive Message") && currentMessage != null)
                    {
                        var header = new GameMessageHeader(senderThemeId,
                            serializer.SerializeGameMessage(currentMessage),
                            currentMessage.GetType().ToString(),
                            currentMessage.MessageVersion, 0, 0);
                        var serializedHeader = headerSerializer.SerializeGameMessageHeader(header);
                        var count = System.Text.Encoding.UTF8.GetByteCount(serializedHeader);
                        if(count >= MaxGameMessageSizeInBytes)
                        {
                            throw new Exception(
                                string.Format("Serialized Game Message exceeds maximum size of {0} bytes.",
                                    MaxGameMessageSizeInBytes));
                        }
                        controller.Synchronizer.SendGameEvent(serializedHeader);
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {

                    EditorGUILayout.LabelField(
                        "BankSynchronizationController not available. \nIs the StandaloneDeviceConfigurator setup correctly for bank synchronization?",
                        new GUIStyle {wordWrap = true, normal = {textColor = Color.red}}
                        );
                }
            }
        }

        /// <summary>
        /// Display the edit field for a float property, and update the current message with the value.
        /// </summary>
        /// <param name="property">The property info to display.</param>
        private static void FloatPropertyEdit(PropertyInfo property)
        {
            var current = property.GetValue(currentMessage, null);
            float newValue = EditorGUILayout.FloatField(property.Name, (float)current);
            property.SetValue(currentMessage, newValue, null);
        }

        /// <summary>
        /// Display the edit field for an int property, and update the current message with the value.
        /// </summary>
        /// <param name="property">The property info to display.</param>
        private static void IntPropertyEdit(PropertyInfo property)
        {
            var current = property.GetValue(currentMessage, null);
            int newValue = EditorGUILayout.IntField(property.Name, (int)current);
            property.SetValue(currentMessage, newValue, null);
        }

        /// <summary>
        /// Display the edit field for a bool property, and update the current message with the value.
        /// </summary>
        /// <param name="property">The property info to display.</param>
        private static void BoolPropertyEdit(PropertyInfo property)
        {
            var current = property.GetValue(currentMessage, null);
            bool newValue = EditorGUILayout.Toggle(property.Name, (bool)current);
            property.SetValue(currentMessage, newValue, null);
        }

        /// <summary>
        /// Display the edit field for a string property, and update the current message with the value.
        /// </summary>
        /// <param name="property">The property info to display.</param>
        private static void StringPropertyEdit(PropertyInfo property)
        {
            var current = property.GetValue(currentMessage, null);
            string newValue = EditorGUILayout.TextField(property.Name, (string)current);
            property.SetValue(currentMessage, newValue, null);
        }
    }
}