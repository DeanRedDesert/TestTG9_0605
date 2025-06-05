//-----------------------------------------------------------------------
// <copyright file = "MachineActivityParser.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Xml.Linq;
    using Communication.Standalone;

    /// <summary>
    /// This class builds a list of machine activity variants by parsing an xml element that
    /// contains the needed information for machine activity.
    /// </summary>
    public class MachineActivityParser
    {
        /// <summary>
        /// MachineActivity Settings.
        /// </summary>
        internal MachineActivityVariant MachineActivitySettings { get; }

        /// <summary>
        /// Initialize a new instance of a machine activity parser using
        /// an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="machineActivityElement">
        /// An xml element contains machine activity related information
        /// Machine activity element must conform to the schema defined in "CsiConfigFileSchemas.xsd".
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when machine activity settings has invalid data.
        /// </exception>
        public MachineActivityParser(XElement machineActivityElement)
        {
            if (machineActivityElement != null)
            {
                try
                {
                    var newGame = bool.Parse(machineActivityElement.Element("NewGame")?.Value ?? string.Empty);
                    var attractInterval = uint.Parse(machineActivityElement.Element("AttractInterval")?.Value ?? string.Empty);
                    var inActivityDelay = uint.Parse(machineActivityElement.Element("InActivityDelay")?.Value ?? string.Empty);
                    var attractsEnabled = bool.Parse(machineActivityElement.Element("AttractsEnabled")?.Value ?? string.Empty);

                    MachineActivitySettings = new MachineActivityVariant(attractInterval, inActivityDelay,
                                                                         attractsEnabled,
                                                                         newGame);
                }
                catch
                {
                    throw new InvalidStreamDataException("Error: parsing machine activity settings failed.");
                }
            }
        }
    }
}
