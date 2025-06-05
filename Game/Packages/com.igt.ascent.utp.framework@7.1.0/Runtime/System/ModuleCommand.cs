// -----------------------------------------------------------------------
// <copyright file = "ModuleCommand.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using System;

    /// <summary>
    /// The main command interpreter and executioner.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class ModuleCommand : Attribute
    {
        /// <summary>
        /// The command that the module calls. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Command { get; set; }

        /// <summary>
        /// A description of what the command does. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Description { get; set; }

        /// <summary>
        /// The type of response the command returns with. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Returns { get; set; }

        /// <summary>
        /// The description of the parameters of the command.
        /// </summary>
        /// <remarks>This can be separated with a "|" to offer more description of the command. 
        ///     ie: name|type|description
        ///     All items must be used or only the parameter description will be updated. 
        /// </remarks>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string[] Parameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleCommand"/> class.
        /// </summary>
        public ModuleCommand()
        {
            Command = "";
            Description = "";
            Returns = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleCommand"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="returns">The returns.</param>
        /// <param name="description">The description.</param>
        /// <param name="parameters">The parameters of the module. 
        /// Parameters may be separated with a "|" to offer more description of the command. 
        ///     ie: name|type|description
        ///     All items must be used or only the parameter description will be updated. </param>
        public ModuleCommand(string command, string returns, string description, string[] parameters = null)
        {
            if(string.IsNullOrEmpty(returns))
            {
                throw new ArgumentNullException("returns");
            }

            Command = command;
            Returns = UtpTypeSerializer.GetTypeDefinition(returns);
            Description = description;

            if(parameters == null)
                Parameters = new string[] {};
            else
            {
                for(int i = 0; i < parameters.Length; i++)
                {
                    var parts = parameters[i].Split('|');
                    if(parts.Length == 3)
                    {
                        parts[1] = UtpTypeSerializer.GetTypeDefinition(parts[1], true);
                        parameters[i] = string.Join("|", parts);
                    }
                }
                Parameters = parameters;
            }
        }
    }
}