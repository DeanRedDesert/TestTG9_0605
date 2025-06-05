//-----------------------------------------------------------------------
// <copyright file = "IActiveTilt.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using CompactSerialization;

    /// <summary>
    /// An interface that encapsulates a tilt object which does not define a tilt discard behavior.
    /// The tilt object will be discarded when the current context gets inactivated or on a power hit.
    /// </summary>
    public interface IActiveTilt : ICompactSerializable
    {
        /// <summary>
        /// The priority of the tilts.  Tilts are sorted by priority.
        /// </summary>
        TiltPriority Priority { get; }

        /// <summary>
        /// Defines whether the tilt will block the game play.
        /// </summary>
        /// <remarks>
        /// The tilt posted by an extension cannot block extension or other applications play. The tilt posted by an extension
        /// can only block game play. This is because the Foundation cannot block the extension from play.
        /// </remarks>
        TiltGamePlayBehavior GamePlayBehavior { get; }

        /// <summary>
        /// Defines whether the tilt requires user notification through a protocol.
        /// </summary>
        bool UserInterventionRequired { get; }

        /// <summary>
        /// Defines whether the tilt signals a progressive link down.
        /// </summary>
        /// <remarks>
        /// An extension can post an active tilt for any application progressive link down. This includes a game controlled
        /// progressive link down although the game can post a tilt on a game progressive link down.
        /// </remarks>
        bool ProgressiveLinkDown { get; }

        /// <summary>
        /// Defines whether the tilt can be cleared by an attendant.
        /// </summary>
        /// <remarks>
        /// A tilt posted with this set will get a notification when the tilt is cleared by an attendant.
        /// </remarks>
        bool AttendantClear { get; }

        /// <summary>
        /// Defines whether delay the preventing of game play until all games and game like features (such as sports betting purchases) have been completed.
        /// </summary>
        /// <remarks>
        /// It is an error to set this flag when requesting a tilt unless the <see cref="GamePlayBehavior"/> is <see cref="TiltGamePlayBehavior.Blocking"/>.
        /// </remarks>
        bool DelayPreventGamePlayUntilNoGameInProgress { get; }

        /// <summary>
        /// Returns the title for the tilt in the given culture.
        /// </summary>
        /// <param name="culture">The requested culture.</param>
        /// <returns>The title for the given culture, or null if the tilt is not localized to that culture.</returns>
        string GetLocalizedTitle(string culture);
        
        /// <summary>
        /// Returns the message for the tilt in the given culture.
        /// </summary>
        /// <param name="culture">The requested culture.</param>
        /// <returns>The message for the given culture, or null if the tilt is not localized to that culture.</returns>
        string GetLocalizedMessage(string culture);
    }
}
