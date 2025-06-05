// -----------------------------------------------------------------------
// <copyright file = "AutomationCommand.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    /// <summary>
    /// AutomationCommand. Class that encapsulates commands and data to be used for testing.
    /// </summary>
    [Serializable]
    public class AutomationCommand
    {
        /// <summary>
        /// The target module for the automation command. 
        /// </summary>
        /// <value>
        /// The module name.
        /// </value>
        public string Module { get; set; }

        /// <summary>
        /// The command of the module to be called. 
        /// </summary>
        /// <value>
        /// The command name.
        /// </value>
        public string Command { get; set; }

        /// <summary>
        /// A descriptive summary of the intent of the command. 
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Type { get; set; }

        /// <summary>
        /// Parameter for the command to execute
        /// </summary>
        /// <value>
        /// A list of parameters for the command
        /// </value>
        public List<AutomationParameter> Parameters { get; set; }

        /// <summary>
        /// If a parameter is an event, flag.
        /// </summary>
        [DefaultValue(false)]
        public bool IsEvent { get; set; }

        /// <summary>
        /// AutomationCommand()
        /// Constructor. Doesn't do anything. However is used by other parts of the framework.
        /// </summary>
        public AutomationCommand()
        {
            Parameters = new List<AutomationParameter>();
        }

        /// <summary>
        /// AutomationCommand()
        /// Constructor. This takes the arguments and stores them locally.
        /// </summary>
        /// <param name="module">String name of the module this data is addressing.</param>
        /// <param name="command">String command for the module.</param>
        /// <param name="parameters">A list of AutomationParameters needed by the command.</param>
        public AutomationCommand(string module, string command, List<AutomationParameter> parameters)
        {
            Module = module;
            Command = command;
            Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationCommand"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="command">The command.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="isEvent">if set to <c>true</c> [is event].</param>
        public AutomationCommand(string module,
                                 string command,
                                 string description,
                                 string type,
                                 List<AutomationParameter> parameters,
                                 bool isEvent = false)
        {
            Module = module;
            Command = command;
            Description = description;
            Type = type;
            Parameters = parameters;
            IsEvent = isEvent;
        }

        /// <summary>
        /// Serialize() is a utility method that converts an AutomationCommand into an XML string.
        /// </summary>
        /// <param name="command">The AutomationCommand to convert.</param>
        /// <param name="subNode">Remove xml node - if not removed it will break javascript parsers</param>
        /// <returns>A string that represents the AutomationCommand in XML.</returns>
        public static string Serialize(AutomationCommand command, bool subNode = false)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }

            var serializedCommand = new XmlSerializer(typeof(AutomationCommand));
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, Encoding.Unicode);

            serializedCommand.Serialize(streamWriter, command);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(memoryStream, Encoding.Unicode);
            var streamData = streamReader.ReadToEnd();
            streamReader.Close();
            memoryStream.Close();

            if(subNode)
            {
                streamData = Regex.Replace(streamData, @"\<\?xml.+\?\>", "", RegexOptions.IgnoreCase);
            }

            return streamData;
        }

        /// <summary>
        /// Deserialize()
        /// Utility method that converts a properly formatted XML string into an AutomationCommand.
        /// </summary>
        /// <param name="command">The string to convert in XML.</param>
        /// <returns>An AutomationCommand.</returns>
        public static AutomationCommand Deserialize(string command)
        {
            if(string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException("command");
            }

            command = command.Trim();
            var serializer = new XmlSerializer(typeof(AutomationCommand));
            var automationCommand = serializer.Deserialize(new StringReader(command)) as AutomationCommand;

            return automationCommand;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            //  The result should appear something like this:
            //  Module:UtpBanners, Command:GetBannerGroupNames, Parameters:[BannerName|Banner1, BannerGroupName|BG1]
            var paramsString = string.Join(", ", Parameters.Select(p => p.Name + "|" + p.Value).ToArray());
            return "Module:" + Module + ", Command:" + Command + "Parameters:[" + paramsString + "]";
        }
    }
}