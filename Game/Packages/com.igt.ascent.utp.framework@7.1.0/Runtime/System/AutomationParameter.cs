// -----------------------------------------------------------------------
// <copyright file = "AutomationParameter.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Parameter information for use in Automation Modules. 
    /// </summary>
    public class AutomationParameter
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type. Valid types include: Image, Text, and None. 
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationParameter"/> class.
        /// </summary>
        public AutomationParameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationParameter"/> class.
        /// Initializes the instance with a ToString output. 
        /// </summary>
        /// <param name="paramString">The parameter string.</param>
        public AutomationParameter(string paramString)
        {
            try
            {
                // Parameters are delimited by '|'
                var parameters = paramString.Split('|');
                if (parameters.Length == 3)
                {
                    Name = parameters[0];
                    Type = parameters[1];
                    Description = parameters[2];
                }
                else
                {
                    Name = "";
                    Description = paramString;
                    Type = "";
                }
            }
            catch
            {
                Name = "";
                Description = "";
                Type = "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationParameter"/> class. Value object will be serialized.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The object value. Use a string or other object type.</param>
        /// <param name="type">The type.</param>
        /// <param name="description">The description.</param>
        public AutomationParameter(string name, object value, string type = null, string description = null)
        {
            Name = name;

            if(value != null)
                Value = UtpTypeSerializer.IsSimpleObject(value.GetType()) ? value.ToString() : UtpTypeSerializer.SerializeObject(value);

            Type = type;
            Description = description;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if(string.IsNullOrEmpty(Value))
                return Name + "|" + Type + "|" + Description;

            return Name + "|" + Value + "|" + Type + "|" + Description;
        }

        /// <summary>
        /// Validates the incoming AutomationParameters list. If errors are found, the error message is returned. Otherwise the return string is null.
        /// </summary>
        /// <param name="incomingParameters">The incoming parameters.</param>
        /// <param name="expectedParameterNames">The expected parameter names.</param>
        /// <returns>If an error is found with the parameters, the returned string is a message describing the error. If no errors are found, it returns null.</returns>
        public static string GetIncomingParameterValidationErrors(List<AutomationParameter> incomingParameters, List<string> expectedParameterNames)
        {
            //  The incoming list should not be null
            if(incomingParameters == null)
            {
                return "Unable to process command because the incoming parameters list is null.";
            }

            //  The incoming list should have the correct number of parameters
            var expectedCount = expectedParameterNames.Count;

            if(incomingParameters.Count != expectedCount)
            {
                return "An invalid number of AutomationParameters were received. Exactly " + expectedCount +
                       " parameters were expected but " + incomingParameters.Count + " were received.";
            }

            //  Each parameter should have the expected Name
            for(var parameterCount = 0; parameterCount < expectedParameterNames.Count; parameterCount++)
            {
                var paramIndex = parameterCount + 1;

                //  Make sure the param isn't null
                if(incomingParameters[parameterCount] == null)
                {
                    return "Expected an AutomationParameter in position #" + paramIndex +
                           ", but found a null value instead.";
                }
                //  Make sure the names match
                if(incomingParameters[parameterCount].Name == expectedParameterNames[parameterCount])
                {
                    continue;
                }

                return "Expected incoming AutomationParameter #" + paramIndex + " to be named " +
                       expectedParameterNames[parameterCount] + " but it was actually " +
                       incomingParameters[parameterCount].Name;
            }
            return null;
        }

        /// <summary>
        /// Sets the value of all matching parameters.
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="parameter">Parameter name to set the value of.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="description">The parameter description.</param>
        public static void SetParameter(AutomationCommand command, string parameter, string value, string type = null, string description = null)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }

            var parameters = command.Parameters.Where(p => p.Name == parameter).ToList();
            if(parameters.Any())
            {
                parameters.ForEach(pp =>
                {
                    pp.Value = value;
                    pp.Type = type ?? pp.Type;
                    pp.Description = description ?? pp.Description;
                });
            }
            else
            {
                command.Parameters.Add(new AutomationParameter(parameter, value, type, description));
            }
        }

        /// <summary>
        /// Returns the values of all matching parameters.
        /// </summary>
        /// <param name="command">The command to find values in.</param>
        /// <param name="parameterName">Name of the parameter to get the values from.</param>
        /// <param name="ignoreCase">Ignore case.</param>
        /// <returns>Values of matching parameters.</returns>
        public static List<string> GetParameterValues(AutomationCommand command, string parameterName, bool ignoreCase = true)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }

            List<string> paramValues;
            if(ignoreCase)
            {
                paramValues =
                    command.Parameters.Where(p => p.Name.ToLower() == parameterName.ToLower())
                        .Select(p => p.Value)
                        .ToList();
            }
            else
            {
                paramValues = command.Parameters.Where(p => p.Name == parameterName).Select(p => p.Value).ToList();
            }
            return paramValues;
        }

        /// <summary>
        /// Gets a parameter dictionary from the supplied command
        /// </summary>
        /// <param name="command">Command to get the parameter dictionary from</param>
        /// <returns>Dictionary with the parameters grouped by name</returns>
        public static Dictionary<string, List<string>> GetParameterDictionary(AutomationCommand command)
        {
            if(command == null)
            {
                throw new ArgumentNullException("command");
            }

            var parameterDictionary = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach(var param in command.Parameters)
            {
                if(!parameterDictionary.ContainsKey(param.Name))
                {
                    parameterDictionary.Add(param.Name, new List<string> { param.Value });
                }
                else
                {
                    parameterDictionary[param.Name].Add(param.Value);
                }
            }
            return parameterDictionary;
        }
    }
}