//-----------------------------------------------------------------------
// <copyright file = "ProgressiveSimulatorParser.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.Xml.Linq;
    using Communication.Standalone;

    /// <summary>
    /// This class parses progressive simulator information from an XmlElement.
    /// </summary>
    internal class ProgressiveSimulatorParser
    {
        /// <summary>
        /// Flag indicating if the simulator should be created and enabled in idle mode.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// The credits to add to the underlying progressive setups.
        /// </summary>
        public int Credits { get; }

        /// <summary>
        /// The frequency in which to contribute to the progressive, specified in seconds.
        /// </summary>
        public int ContributionFrequency { get; }

        /// <summary>
        /// Initialize a new instance of a ProgressiveSimulatorParser using an xml element derived from a persistent
        /// settings file.
        /// </summary>
        /// <param name="settings">
        /// An <see cref="XContainer"/> element that contains the settings to enable or disable the progressive simulator.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="settings"/> has invalid data.
        /// </exception>
        public ProgressiveSimulatorParser(XContainer settings)
        {
            if(settings != null)
            {
                // Enabled.
                Enabled = settings.Element(@"Enabled")?.Value.Contains(@"true".ToLowerInvariant()) == true;

                // Credits.
                var setting = settings.Element(@"Credits")?.Value;
                Credits = int.TryParse(setting, out var convertValue) ? convertValue : 10;

                // The update timer frequency in seconds.
                setting = settings.Element(@"ContributionFrequency")?.Value;
                ContributionFrequency = int.TryParse(setting, out convertValue) ? convertValue : 1;
            }
        }
    }
}
