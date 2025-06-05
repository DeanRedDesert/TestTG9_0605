//-----------------------------------------------------------------------
// <copyright file = "ProgressiveParser.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Communication.Standalone;
    using Logic.ProgressiveController;
    using Registries;

    /// <summary>
    /// This class builds a list of progressive controllers and a list
    /// of progressive setups by parsing an xml element that contains
    /// the needed information.
    /// </summary>
    internal class ProgressiveParser
    {
        #region Progressive Parser Implementation

        /// <summary>
        /// List of progressive controller settings, keyed by the
        /// controller names, as the parsing result.
        /// 
        /// For each controller name, there is a list of controller
        /// level configurations.
        /// </summary>
        public Dictionary<string, List<ProgressiveConfiguration>> ControllerSettings { get; private set; }

        /// <summary>
        /// List of progressive setups, keyed by the combination of
        /// a denomination and a paytable variant, as the parsing result.
        /// Null if no ProgressiveSetups element is present.
        /// 
        /// For each denomination of each paytable, there is a list
        /// of game level mappings, a.k.a progressive links.
        /// </summary>
        public Dictionary<KeyValuePair<long, PaytableVariant>, List<ProgressiveLink>> ProgressiveSetups { get; private set; }

        /// <summary>
        /// Initialize a new instance of Progressive Parser using an xml element
        /// that contains the needed information for parsing.
        /// </summary>
        /// <param name="progressiveElement">
        /// An xml element that contains the controller settings and link setups
        /// for the system controlled progressives.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="progressiveElement"/> has invalid data,
        /// such as missing controllers, nonconsecutive controller levels, duplicate
        /// controller names, invalid controller name and controller level in
        /// a progressive link, game level linking to multiple controller levels
        /// and so on. 
        /// </exception>
        public ProgressiveParser(XElement progressiveElement)
        {
            if(progressiveElement != null)
            {
                ParseProgressiveControllers(progressiveElement.Element("ProgressiveControllers"));
                ParseProgressiveSetups(progressiveElement.Element("ProgressiveSetups"));
            }
            // If no system controlled progressive is defined, initialize members
            // to empty dictionaries to avoid checking for null in the future.
            else
            {
                ControllerSettings = new Dictionary<string, List<ProgressiveConfiguration>>();
                ProgressiveSetups = new Dictionary<KeyValuePair<long, PaytableVariant>, List<ProgressiveLink>>();
            }
        }

        /// <summary>
        /// Initialize the list of progressive controller settings from an xml element.
        /// </summary>
        /// <param name="controllersElement">
        /// An xml element that contains the controller settings.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="controllersElement"/> has invalid data. 
        /// </exception>
        private void ParseProgressiveControllers(XElement controllersElement)
        {
            // Should have been covered by the schema.  But double check.
            if(controllersElement == null)
            {
                throw new InvalidStreamDataException(
                    "No controller is defined for System Controlled Progressives node.");
            }

            // Process the ProgressiveControllers element.
            var controllerElements = controllersElement.Elements("ProgressiveController").ToList();
            ControllerSettings = new Dictionary<string, List<ProgressiveConfiguration>>(controllerElements.Count);

            // Controller count being 0 is not allowed by the schema,
            // but is ok here since it does not crash the parser.
            // If the controller count is 0, but the setup count not,
            // the parser will throw an exception when cross checking
            // the data validity.

            // For each controller element...
            foreach(var controllerElement in controllerElements)
            {
                var controllerParser = new ControllerParser(controllerElement);

                if(ControllerSettings.ContainsKey(controllerParser.ControllerName))
                {
                    throw new InvalidStreamDataException(
                        $"Duplicate controller name of {controllerParser.ControllerName} in Progressive Controllers node.");
                }

                ControllerSettings[controllerParser.ControllerName] = controllerParser.ConfigurationList;
            }
        }

        /// <summary>
        /// Initialize the list of progressive links from an xml element.
        /// </summary>
        /// <param name="setupsElement">
        /// An xml element that contains the progressive setups.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="setupsElement"/> has invalid data. 
        /// </exception>
        private void ParseProgressiveSetups(XElement setupsElement)
        {
            // If the element presents.
            if(setupsElement != null)
            {
                // Process the ProgressiveSetups element.
                var setupElements = setupsElement.Elements("ProgressiveSetup").ToList();
                ProgressiveSetups =
                    new Dictionary<KeyValuePair<long, PaytableVariant>, List<ProgressiveLink>>(setupElements.Count);

                // Setup count being 0 is not allowed by the schema,
                // but is ok here since it does not crash the parser.
                // The game will just play as no game level is linked.

                // For each setup element...
                foreach(var setupElement in setupElements)
                {
                    var setupParser = new SetupParser(setupElement);

                    // Add the setup to the manager's setup list.
                    ProgressiveSetups[setupParser.PaytableConfiguration] = setupParser.ProgressiveLinks;
                }
            }
        }

        #endregion

        #region Inner Class: Controller Parser

        /// <summary>
        /// This class builds a progressive controller by parsing
        /// an xml element that contains the needed information.
        /// </summary>
        private class ControllerParser
        {
            /// <summary>
            /// Name of the controller, as the parsing result.
            /// </summary>
            public string ControllerName { get; }

            /// <summary>
            /// List of level configurations for the controller,
            /// as the parsing result.
            /// </summary>
            public List<ProgressiveConfiguration> ConfigurationList { get; }

            /// <summary>
            /// Initialize a new instance of Controller Parser using an xml
            /// element that contains the needed information for parsing.
            /// </summary>
            /// <param name="controllerElement">
            /// An xml element that contains the controller setting.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="controllerElement"/> is null. 
            /// </exception>
            /// <exception cref="InvalidStreamDataException">
            /// Thrown when <paramref name="controllerElement"/> has invalid data. 
            /// </exception>
            public ControllerParser(XElement controllerElement)
            {
                if(controllerElement == null)
                {
                    throw new ArgumentNullException(nameof(controllerElement));
                }

                // Retrieve the controller's name.
                ControllerName = (string)controllerElement.Attribute("Name");

                // Retrieve the configuration list for the controller's levels.
                var levelElements = (from level in controllerElement.Elements("ControllerLevel")
                                    orderby (int)level.Attribute("Id")
                                    select level).ToList();

                ConfigurationList = new List<ProgressiveConfiguration>(levelElements.Count);
                var levelId = 0;

                foreach(var level in levelElements)
                {
                    // Controller level must be defined consecutively.
                    if(levelId != (int)level.Attribute("Id"))
                    {
                        throw new InvalidStreamDataException(
                            $"Missing Controller Level {levelId} in {ControllerName}'s controller node.");
                    }

                    // Add the controller level's configuration to the configuration list.
                    ConfigurationList.Add(new ProgressiveConfiguration(levelId,
                                                                       (long)level.Element("StartingAmount"),
                                                                       (long)level.Element("MaximumAmount"),
                                                                       (float)level.Element("ContributionPercentage"),
                                                                       (string)level.Element("PrizeString"),
                                                                       level.Element("IsEventBased") != null && (bool)level.Element("IsEventBased")));
                    levelId++;
                }
            }
        }

        #endregion

        #region Inner Class: Setup Parser

        /// <summary>
        /// This class builds a list of progressive links that corresponds
        /// to a specific combination of a denomination and a paytable variant
        /// by parsing an an xml element that contains the needed information.
        /// </summary>
        private class SetupParser
        {
            // List of controller types that are disallowed for dynamic setup.
            private readonly List<string> controllerTypesDisallowed = new List<string>
                                                                      {
                                                                          ProgressiveControllerTypes.WAP,
                                                                          ProgressiveControllerTypes.GCP
                                                                      };
            /// <summary>
            /// Pair of denomination and paytable variant,
            /// used as the key of the progressive setup.
            /// Part of the parsing result.
            /// </summary>
            public KeyValuePair<long, PaytableVariant> PaytableConfiguration { get; }

            /// <summary>
            /// List of progressive links.  Each list corresponds
            /// to a paytable variant at a denomination.
            /// Part of the parsing result.
            /// </summary>
            public List<ProgressiveLink> ProgressiveLinks { get; }

            /// <summary>
            /// Initialize a new instance of Setup Parser using an xml
            /// element that contains the needed information for parsing.
            /// </summary>
            /// <param name="setupElement">
            /// An xml element that contains the progressive setup information
            /// for a paytable variant at a denomination.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="setupElement"/> is null. 
            /// </exception>
            /// <exception cref="InvalidStreamDataException">
            /// Thrown when <paramref name="setupElement"/> has invalid data. 
            /// </exception>
            public SetupParser(XElement setupElement)
            {
                if(setupElement == null)
                {
                    throw new ArgumentNullException(nameof(setupElement));
                }

                // Retrieve the progressive setup key, which is the pair of denomination and paytable variant.
                var paytableConfigurationElement = setupElement.Element("PaytableConfiguration");

                if(paytableConfigurationElement == null)
                {
                    throw new InvalidStreamDataException(
                        "Missing Paytable Configuration element from a Progressive Setup node.");
                }

                var paytableVariant =
                    new PaytableVariant((string)paytableConfigurationElement.Element("ThemeIdentifier"),
                                        (string)paytableConfigurationElement.Element("PaytableName"),
                                        Utility.UniformSlashes((string)paytableConfigurationElement.Element("PaytableFileName")));

                PaytableConfiguration =
                    new KeyValuePair<long, PaytableVariant>(
                        (long)paytableConfigurationElement.Element("Denomination"),
                        paytableVariant);

                // Retrieve the progressive setup value, which is the list of progressive links.
                var linkElements = setupElement.Elements("ProgressiveLink").ToList();

                if(linkElements.Any() != true)
                {
                    throw new InvalidStreamDataException(
                        $"No progressive link is defined for the denomination {PaytableConfiguration.Key} and paytable variant {PaytableConfiguration.Value}");
                }

                ProgressiveLinks = new List<ProgressiveLink>(linkElements.Count);

                // Validate each progressive link and add to the link list.
                foreach(var linkElement in linkElements)
                {
                    // Create a progressive link from the xml node.
                    var progressiveLink = new ProgressiveLink
                                              {
                                                  GameLevel = (int)linkElement.Element("GameLevel"),
                                                  ControllerName = (string)linkElement.Element("ControllerName"),
                                                  ControllerLevel = (int)linkElement.Element("ControllerLevel")
                                              };

                    // Verify the controller type is allowed for dynamic setup via the operator menu,
                    // which is represented by the system configuration file in Standalone Game Lib.
                    if(controllerTypesDisallowed.Contains(progressiveLink.ControllerName))
                    {
                        throw new InvalidStreamDataException(
                            $"{progressiveLink.ControllerName} links cannot be set up dynamically.  They can only be set up in game registries.");
                    }

                    // Verify the game level is not linked yet.
                    var existingLink = ProgressiveLinks.FirstOrDefault(link => link.GameLevel == progressiveLink.GameLevel);

                    if(existingLink != null)
                    {
                        throw new InvalidStreamDataException(
                            $"Game level {progressiveLink.GameLevel} is linked more than once for the denomination {PaytableConfiguration.Key} and paytable variant {PaytableConfiguration.Value}.");
                    }

                    ProgressiveLinks.Add(progressiveLink);
                }
            }
        }

        #endregion
    }
}
