//-----------------------------------------------------------------------
// <copyright file = "GameTiltDefinition.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Linq;

    public partial class GameTiltDefinition : IGameTiltDefinition, IEquatable<GameTiltDefinition>
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(GameTiltDefinition));

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="GameTiltDefinition"/> using another instance.
        /// </summary>
        /// <param name="otherTiltDef"></param>
        public GameTiltDefinition(IGameTiltDefinition otherTiltDef)
        {
            var tiltDef = this as IGameTiltDefinition;
            tiltDef.DiscardBehavior = otherTiltDef.DiscardBehavior;
            tiltDef.GameControlledProgressiveLinkDown = otherTiltDef.GameControlledProgressiveLinkDown;
            tiltDef.GamePlayBehavior = otherTiltDef.GamePlayBehavior;
            tiltDef.MessageLocalizations = otherTiltDef.MessageLocalizations;
            tiltDef.Priority = otherTiltDef.Priority;
            tiltDef.TitleLocalizations = otherTiltDef.TitleLocalizations;
            tiltDef.UserInterventionRequired = otherTiltDef.UserInterventionRequired;
        }

        #endregion

        #region Load/Save functions.

        /// <summary>
        /// Loads a game tilt file from disk.
        /// </summary>
        /// <param name="filename">The path to the file to load.</param>
        /// <returns>The deserialized IGameTiltDefinition object.</returns>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the file path in <paramref name="filename"/> cannot be found on disk.
        /// </exception>
        public static IGameTiltDefinition Load(string filename)
        {
            if(!File.Exists(filename))
            {
                throw new FileNotFoundException($"Unable to find the tilt definition file '{filename}'.", filename);
            }

            GameTiltDefinition gameTiltObject;

            
            using(var reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                gameTiltObject = Serializer.Deserialize(reader) as GameTiltDefinition;
            }

            return gameTiltObject;
        }

        /// <summary>
        /// Save a game tilt file to disk.
        /// </summary>
        /// <param name="filename">The path to the file to save.</param>
        /// <param name="gameTilt">The IGAmeTiltDefinition file to save.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameTilt"/> is null
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown if <paramref name="gameTilt"/> does not have any message or title localizations.
        /// </exception>
        public static void Save(string filename, IGameTiltDefinition gameTilt)
        {
            var tiltToSave = new GameTiltDefinition(gameTilt);

            if (tiltToSave.titleLocalizationsField?.Any() != true)
            {
                throw new InvalidDataException("Cannot serialize tilt with no title localizations.");
            }

            if (tiltToSave.messageLocalizationsField?.Any() != true)
            {
                throw new InvalidDataException("Cannot serialize tilt with no message localizations.");
            }

            using(var writer = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                TextWriter textWriter = new StreamWriter(writer);
                Serializer.Serialize(textWriter, tiltToSave);
            }
        }

        #endregion

        #region IEquatable<GameTiltDefinition> Member

        /// <summary>
        /// Determine if two GameTiltDefinition objects are equivalent.
        /// </summary>
        /// <param name="other">The other <see cref="GameTiltDefinition"/> to check against.</param>
        /// <returns>Returns true if they are equivalent.</returns>
        public bool Equals(GameTiltDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return titleLocalizationsField.SequenceEqual(other.titleLocalizationsField) &&
                   messageLocalizationsField.SequenceEqual(other.messageLocalizationsField) &&
                   userInterventionRequiredField.Equals(other.userInterventionRequiredField) &&
                   gamePlayBehaviorField == other.gamePlayBehaviorField &&
                   priorityField == other.priorityField &&
                   gameControlledProgressiveLinkDownField.Equals(other.gameControlledProgressiveLinkDownField) &&
                   discardBehaviorField == other.discardBehaviorField;
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GameTiltDefinition)obj);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class is an xml generated object, and must be mutable.
        /// It should not be modified after GetHashCode is called.
        /// It should not be used as a key in a dictionary.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
// ReSharper disable NonReadonlyFieldInGetHashCode
                var hashCode = titleLocalizationsField != null ? titleLocalizationsField.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (messageLocalizationsField != null ? messageLocalizationsField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ userInterventionRequiredField.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)gamePlayBehaviorField;
                hashCode = (hashCode * 397) ^ (int)priorityField;
                hashCode = (hashCode * 397) ^ gameControlledProgressiveLinkDownField.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)discardBehaviorField;
// ReSharper restore NonReadonlyFieldInGetHashCode
                return hashCode;
            }
        }

        #endregion

        #region IGameTiltDefinition Implementation

        /// <inheritdoc/>
        TiltPriority IGameTiltDefinition.Priority
        {
            get => Priority.ToTiltPriority();
            set => Priority = value.ToTiltDefinitionPriority();
        }

        /// <inheritdoc/>
        TiltGamePlayBehavior IGameTiltDefinition.GamePlayBehavior
        {
            get => GamePlayBehavior.ToTiltGamePlayBehavior();
            set => GamePlayBehavior = value.ToTiltDefinitionGamePlayBehavior();
        }

        /// <inheritdoc/>
        TiltDiscardBehavior IGameTiltDefinition.DiscardBehavior
        {
            get => DiscardBehavior.ToTiltDiscardBehavior();
            set => DiscardBehavior = value.ToTiltDefinitionDiscardBehavior();
        }

        /// <inheritdoc/>
        IList<Pair<string, string>> IGameTiltDefinition.TitleLocalizations
        {
            get => TranslateLocalizationsFromSchema(TitleLocalizations);
            set => TitleLocalizations = TranslateLocalizationsToSchema(value);
        }

        /// <inheritdoc/>
        IList<Pair<string, string>> IGameTiltDefinition.MessageLocalizations
        {
            get => TranslateLocalizationsFromSchema(MessageLocalizations);
            set => MessageLocalizations = TranslateLocalizationsToSchema(value);
        }

        #endregion

        #region Helper Functions

        private static List<Localization> TranslateLocalizationsToSchema(IList<Pair<string, string>> localizations)
        {
            List<Localization> translatedLocalizations = null;
            if(localizations != null)
            {
                translatedLocalizations = localizations.Select(pair => new Localization
                {
                    Culture = pair.First,
                    Content = pair.Second
                })
                    .ToList();
            }
            return translatedLocalizations;
        }

        private static IList<Pair<string, string>> TranslateLocalizationsFromSchema(List<Localization> localizations)
        {
            List<Pair<string, string>> translatedLocalizations = null;
            if(localizations != null)
            {
                translatedLocalizations =
                    localizations.Select(titleLocalization => new Pair<string, string>
                    {
                        First = titleLocalization.Culture,
                        Second = titleLocalization.Content
                    })
                        .ToList();
            }
            return translatedLocalizations;
        }

        #endregion
    }
}
