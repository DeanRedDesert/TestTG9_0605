//-----------------------------------------------------------------------
// <copyright file = "TournamentSessionConfigParser.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Xml.Linq;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// This class builds a list of tournament configuration options by parsing an xml
    /// element that contains the needed information for Standalone mode.
    /// </summary>
    internal class TournamentSessionConfigParser
    {
        /// <summary>
        /// The tournament session type.
        /// </summary>
        public TournamentSessionType SessionType { get; }

        /// <summary>
        /// The count down duration.
        /// </summary>
        public int CountdownDuration { get; }

        /// <summary>
        /// The duration of the tournament session.
        /// </summary>
        public int PlayDuration { get; }

        /// <summary>
        /// The starting credits of the tournament session.
        /// </summary>
        public long InitialCredits { get; }

        /// <summary>
        /// The class constructor that initializes a new instance of a tournament session configuration
        /// parser using an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="tournamentSessionConfigElement">
        /// An xml element that contains the tournament session configuration.
        /// </param>
        public TournamentSessionConfigParser(XElement tournamentSessionConfigElement)
        {
            if(tournamentSessionConfigElement != null)
            {
                var element = tournamentSessionConfigElement.Element("SessionType");
                if(element != null)
                {
                    SessionType = (TournamentSessionType)Enum.Parse(typeof(TournamentSessionType), (string)element);
                }

                element = tournamentSessionConfigElement.Element("CountdownDuration");
                if(element != null)
                {
                    CountdownDuration = (int)element;
                }

                element = tournamentSessionConfigElement.Element("PlayDuration");
                if(element != null)
                {
                    PlayDuration = (int)element;
                }

                element = tournamentSessionConfigElement.Element("InitialCredits");
                if(element != null)
                {
                    InitialCredits = (long)element;
                }
            }
        }
    }
}
