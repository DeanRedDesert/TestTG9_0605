// -----------------------------------------------------------------------
// <copyright file = "VolumeSettingsParser.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Xml.Linq;

    /// <summary>
    /// This class retrieves volume settings
    /// by parsing an xml element that contains the needed information.
    /// </summary>
    internal class VolumeSettingsParser
    {
        /// <summary>
        /// Indicate whether the volume is selectable.
        /// </summary>
        public bool VolumePlayerSelectable = true;

        /// <summary>
        /// Indicate whether the mute is selectable.
        /// </summary>
        public bool VolumePlayerMuteSelectable = true;

        /// <summary>
        /// Indicate whether the MuteAll is selected.
        /// </summary>
        public bool MuteAll;

        /// <summary>
        /// Initialize a new instance of Cabinet Settings Parser using
        /// an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="settingsElement">
        /// An xml element that contains the Cabinet settings.
        /// </param>
        public VolumeSettingsParser(XElement settingsElement)
        {
            if(settingsElement != null)
            {
                var element = settingsElement.Element("VolumePlayerSelectable");
                if(element != null)
                {
                    VolumePlayerSelectable = (bool)element;
                }

                element = settingsElement.Element("VolumePlayerMuteSelectable");
                if(element != null)
                {
                    VolumePlayerMuteSelectable = (bool)element;
                }

                element = settingsElement.Element("MuteAll");
                if(element != null)
                {
                    MuteAll = (bool)element;
                }
            }
        }
    }
}