//-----------------------------------------------------------------------
// <copyright file = "PaytableListParser.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Communication.Standalone;

    /// <summary>
    /// This class builds a list of paytable variants and their associated
    /// denominations by parsing an an xml element that contains
    /// the needed information.
    /// </summary>
    internal class PaytableListParser
    {
        /// <summary>
        /// Get the list of paytable configurations, each associated with a denomination,
        /// grouped by the theme names, as the parsing result.
        /// </summary>
        public Dictionary<string, Dictionary<long, PaytableConfiguration>> PaytableList { get; }

        /// <summary>
        /// Get the paytable variant that is specified as default,
        /// as the parsing result.
        /// </summary>
        public PaytableVariant DefaultPaytableVariant { get; }

        /// <summary>
        /// Get the denomination associated with the default paytable,
        /// as the parsing result.
        /// </summary>
        public long DefaultDenomination { get; }

        /// <summary>
        /// Initialize a new instance of a Paytable List Parser using
        /// an xml element that contains the needed information for parsing.
        /// 
        /// If no paytable variant is marked as default in the xml element,
        /// the first paytable variant in the list will be set as the default.
        /// </summary>
        /// <param name="paytableListElement">
        /// An xml element contains list of paytable configurations and their
        /// associated denominations.
        /// Paytable list element must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when paytable list has invalid data, such as duplicate denominations,
        /// or duplicate default paytable variants.
        /// </exception>
        public PaytableListParser(XElement paytableListElement)
        {
            if(paytableListElement != null)
            {
                PaytableList = new Dictionary<string, Dictionary<long, PaytableConfiguration>>();

                var defaultFound = false;

                // Convert xml configuration elements to game lib types and add to the paytable list.
                // Check for data validity along the way.
                foreach(var configurationElement in paytableListElement.Elements("PaytableConfiguration"))
                {
                    var themeIdentifier = (string)configurationElement.Element("ThemeIdentifier");

                    // Add a new theme to the paytable list.
                    if(!PaytableList.ContainsKey(themeIdentifier))
                    {
                        PaytableList[themeIdentifier] = new Dictionary<long, PaytableConfiguration>();
                    }

                    var denomination = (long)configurationElement.Element("Denomination");

                    // For a specific theme, denomination can only be associated with one paytable.
                    if(PaytableList[themeIdentifier].ContainsKey(denomination))
                    {
                        throw new InvalidStreamDataException(
                            $"Denomination {denomination} is associated with more than one paytable for theme {themeIdentifier}.");
                    }

                    var paytableName = (string)configurationElement.Element("PaytableName");
                    var paytableFileName = (string)configurationElement.Element("PaytableFileName");
                    var maxBet = (long?)configurationElement.Element("MaxBet");
                    var buttonPanelMinBet = (long?)configurationElement.Element("ButtonPanelMinBet");

                    // Convert and add to the paytable list.
                    var paytableVariant = new PaytableVariant(themeIdentifier,
                                                              paytableName,
                                                              Utility.UniformSlashes(paytableFileName));

                    PaytableList[themeIdentifier][denomination] =
                        new PaytableConfiguration(paytableVariant, maxBet, buttonPanelMinBet);

                    // Default configuration must be unique.
                    var attribute = configurationElement.Attribute("IsDefault");
                    var isDefault = attribute != null && (bool)attribute;
                    if(isDefault)
                    {
                        if(!defaultFound)
                        {
                            DefaultPaytableVariant = paytableVariant;
                            DefaultDenomination = denomination;
                            defaultFound = true;
                        }
                        else
                        {
                            throw new InvalidStreamDataException(
                                "Only one paytable variant can be set as default.");
                        }
                    }
                }

                // If no configuration is marked as default, set the first one to be default.
                if(!defaultFound)
                {
                    DefaultPaytableVariant = PaytableList.First().Value.First().Value.PaytableVariant;
                    DefaultDenomination = PaytableList.First().Value.First().Key;
                }
            }
        }
    }
}
