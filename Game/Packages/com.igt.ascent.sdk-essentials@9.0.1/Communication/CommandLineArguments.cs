//-----------------------------------------------------------------------
// <copyright file = "CommandLineArguments.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    using System;
    using System.Linq;

    /// <summary>
    /// Provides methods for parsing and storing command line arguments.
    /// </summary>
    public class CommandLineArguments
    {
        /// <summary>
        /// Array to hold all command line arguments.
        /// </summary>
        public string[] CommandLineArgs{get; private set;}

        /// <summary>
        /// Provides the invoking program.
        /// </summary>
        public string InvokingProgram { get; private set; }

        /// <summary>
        /// Boolean to determine whether we are in batch mode.
        /// </summary>
        public bool BatchMode => ContainsFlag("batchmode");

        /// <summary>
        /// Boolean to determine whether build is a release game build.
        /// </summary>
        public bool ReleaseGameBuild
        {
            get { return CommandLineArgs.Any(arg => arg.Contains("CommandLineBuild.ReleaseGame")); }
        }

        /// <summary>
        /// Determines whether flag is in dictionary.
        /// </summary>
        /// <param name="flag">The flag that is to be searched for.</param>
        /// <returns>Boolean value.</returns>
        public bool ContainsFlag(string flag)
        {
            foreach(var command in CommandLineArgs)
            {
                if(command.StartsWith("-" + flag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieves the argument for a flag from the dictionary.
        /// </summary>
        /// <param name="flag">The flag that you want the value for.</param>
        /// <returns>
        /// Returns argument to flag. Returns the empty string if flag didn't have an argument. 
        /// Returns null if flag was not found in the dictionary.
        /// </returns>
        public string GetValue(string flag)
        {
            for(int i = 0; i < CommandLineArgs.Length; i++)
            {
                if(CommandLineArgs[i].StartsWith("-"+flag))
                {
                    return CommandLineArgs[i].Substring(flag.Length + 1);
                }
            }
            return null;
        }

        /// <summary>
        /// Constructor. Parses command line arguments used for custom arguments.
        /// </summary>
        /// <param name="commandLineArgs">Command line arguments to process.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="commandLineArgs" /> is null.</exception>
        /// <exception cref="ArgumentException">If <paramref name ="commandLineArgs" /> is empty.</exception>
        public CommandLineArguments(string[] commandLineArgs)
        {
            if(commandLineArgs == null)
            {
                throw new ArgumentNullException(nameof(commandLineArgs));
            }
            if(commandLineArgs.Length < 1)
            {
                throw new ArgumentException("Must contain at least one element.", nameof(commandLineArgs));
            }

            // Save off the invoking program and arguments
            InvokingProgram = commandLineArgs[0];
            CommandLineArgs = new string[commandLineArgs.Length - 1];
            if(commandLineArgs.Length > 1)
            {
                Array.Copy(commandLineArgs, 1, CommandLineArgs, 0, CommandLineArgs.Length);
            }
        }
        
        /// <summary>
        /// Static CommandLineArguments instance constructed from the environment.
        /// </summary>
        public static readonly CommandLineArguments Environment = new CommandLineArguments(System.Environment.GetCommandLineArgs());
    }
}