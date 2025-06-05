//-----------------------------------------------------------------------
// <copyright file = "IGameTiltDefinition.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System.Collections.Generic;
    using Linq;

    /// <summary>
    /// An interface that encapsulates a tilt object.
    /// </summary>
    public interface IGameTiltDefinition
    {
        /// <summary>
        /// The priority of the tilts.  Tilts are sorted by priority.
        /// </summary>
        TiltPriority Priority { get; set; }

        /// <summary>
        /// Defines whether the tilt will block game play or not.
        /// </summary>
        TiltGamePlayBehavior GamePlayBehavior { get; set; }

        /// <summary>
        /// Defines whether the tilt will be discarded upon game unload.
        /// </summary>
        TiltDiscardBehavior DiscardBehavior { get; set; }

        /// <summary>
        /// Defines whether the tilt requires user notification through a protocol.
        /// </summary>
        bool UserInterventionRequired { get; set; }

        /// <summary>
        /// Defines whether the tilt is a game controlled progressive link down tilt.
        /// </summary>
        bool GameControlledProgressiveLinkDown { get; set; }

        /// <summary>
        /// The list of title localizations pairs of (culture, content)
        /// </summary>
        IList<Pair<string, string>> TitleLocalizations { get; set; }

        /// <summary>
        /// The list of message localizations pairs of (culture, content)
        /// </summary>
        IList<Pair<string, string>> MessageLocalizations { get; set; }
    }
}
