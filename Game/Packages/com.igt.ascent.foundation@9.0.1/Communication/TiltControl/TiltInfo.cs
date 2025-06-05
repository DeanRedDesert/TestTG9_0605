//-----------------------------------------------------------------------
// <copyright file = "TiltInfo.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.TiltControl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using CompactSerialization;
    using Logging;
    using Tilts;

    /// <summary>
    /// A class for storing information about <see cref="IActiveTilt"/> objects.
    /// </summary>
    internal sealed class TiltInfo : ICompactSerializable, IEquatable<TiltInfo>
    {
        #region Fields

        /// <summary>
        /// The list of strings used for the message string format.
        /// </summary>
        private List<string> messageFormat;
        
        /// <summary>
        /// The list of strings used for the title string format.
        /// </summary>
        private List<string> titleFormat;

        /// <summary>
        /// Regex to detect format items in the form: "{index[,alignment][:formatString]}"
        /// </summary>
        private static readonly Regex CountRegex = new Regex(@"{\d+(?:,-?\d+)?(?::[\w\d]+)?}");

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="IActiveTilt"/> implementation that this TiltInfo is tracking.
        /// </summary>
        public IActiveTilt Tilt { get; private set; }

        /// <summary>
        /// The key by which this tilt is stored.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The String.Format() compatible representation of the tilt title format replacement strings.
        /// </summary>
        public object[] TitleFormat => titleFormat.Cast<object>().ToArray();

        /// <summary>
        /// The String.Format() compatible representation of the tilt message format replacement strings.
        /// </summary>
        public object[] MessageFormat => messageFormat.Cast<object>().ToArray();

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default instance of <see cref="TiltInfo"/>.
        /// </summary>
        /// <remarks>Primarily used for <see cref="ICompactSerializable"/>.</remarks>
        public TiltInfo()
        {
        }

        /// <summary>
        /// Create a tilt info from the component key, tilt, title format and message format.
        /// </summary>
        /// <param name="key">The string that will be used to track the <paramref name="tilt"/> provided.</param>
        /// <param name="tilt">the <see cref="IActiveTilt"/> that will be tracked.</param>
        /// <param name="titleFormat">Formatting information for the title of the <paramref name="tilt"/>.</param>
        /// <param name="messageFormat">Formatting information for the message of <paramref name="tilt"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="key"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tilt"/> is null.
        /// </exception>
        public TiltInfo(string key, IActiveTilt tilt, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat)
        {
            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("The key cannot be empty or null.", nameof(key));
            }

            Key = key;
            Tilt = tilt ?? throw new ArgumentNullException(nameof(tilt));
            this.titleFormat = titleFormat != null
                                   ? titleFormat.Select(obj => obj.ToString()).ToList()
                                   : new List<string>();
            this.messageFormat = messageFormat != null
                                     ? messageFormat.Select(obj => obj.ToString()).ToList()
                                     : new List<string>();
        }

        #endregion

        #region ICompactSerializable Member

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Key);
            CompactSerializer.WriteList(stream, titleFormat);
            CompactSerializer.WriteList(stream, messageFormat);
            var tiltType = Tilt.GetType();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, tiltType);
            try
            {
                CompactSerializer.Serialize(stream, Tilt);
            }
            catch(Exception ex)
            {
                throw new TiltSerializationException(
                    "Unable to serialize provided tilt.  See InnerException for details", ex);
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Key = CompactSerializer.ReadString(stream);
            titleFormat = CompactSerializer.ReadListString(stream);
            messageFormat = CompactSerializer.ReadListString(stream);
            IFormatter binaryFormatter = new BinaryFormatter();
            var tiltType = (Type)binaryFormatter.Deserialize(stream);
            try
            {
                Tilt = CompactSerializer.Deserialize(stream, tiltType) as ITilt;
            }
            catch(Exception ex)
            {
                throw new TiltSerializationException(
                    "Unable to deserialize provided tilt.  See InnerException for details", ex);
            }
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override int GetHashCode()
        {
            throw new NotSupportedException("This class should not be used in case where hashing is necessary");
        }

        /// <inheritdoc />
        public override bool Equals(object right)
        {
            if(ReferenceEquals(right, null))
                return false;

            if(ReferenceEquals(this, right))
                return true;

            if(GetType() != right.GetType())
                return false;

            return Equals(right as TiltInfo);
        }

        #endregion

        #region IEquatable<TiltInfo> Member

        /// <summary>
        /// Determine if two TiltInfo objects are equivalent.
        /// </summary>
        /// <param name="other">The TiltInfo to check against.</param>
        /// <returns>Returns true if they are equivalent.</returns>
        public bool Equals(TiltInfo other)
        {
            if(other == null)
            {
                return false;
            }

            return Key == other.Key &&
                   Tilt.Equals(other.Tilt) &&
                   titleFormat.SequenceEqual(other.titleFormat) &&
                   messageFormat.SequenceEqual(other.messageFormat);
        }

        #endregion

        #region Tilt Verification

        /// <summary>
        /// Log potential issues with the posted tilt.
        /// </summary>
        /// <param name="availableLanguages">The languages available to the game.</param>
        [Conditional("DEBUG")]
        public void Verify(IEnumerable<string> availableLanguages)
        {
            foreach(var language in availableLanguages)
            {
                var localizedTitle = Tilt.GetLocalizedTitle(language);
                var localizedMessage = Tilt.GetLocalizedMessage(language);

                if(string.IsNullOrEmpty(localizedTitle))
                {
                    Log.WriteWarning(string.Format(CultureInfo.InvariantCulture,
                                                   "Tilt '{0}' title is not localized for game supported language: {1}",
                                                   Key, language));
                }
                else
                {
                    var formatElements = CountFormatPlaceholders(localizedTitle);
                    var providedElements = titleFormat?.Count ?? 0;
                    if(providedElements != formatElements)
                    {
                        Log.WriteWarning(string.Format(CultureInfo.InvariantCulture,
                                                       "Tilt '{0}' expects {1} format elements for its title, but {2} were provided.",
                                                       Key, providedElements, formatElements));
                    }
                }

                if(string.IsNullOrEmpty(localizedMessage))
                {
                    Log.WriteWarning(string.Format(CultureInfo.InvariantCulture,
                                                   "Tilt {0} message is not localized for game supported language: {1}",
                                                   Key, language));
                }
                else
                {
                    var formatElements = CountFormatPlaceholders(localizedMessage);
                    var providedElements = messageFormat?.Count ?? 0;
                    if(providedElements != formatElements)
                    {
                        Log.WriteWarning(
                            $"Tilt '{Key}' expects {providedElements} format elements for its message, but {formatElements} were provided.");
                    }
                }
            }
        }

        /// <summary>
        /// Counts the number of format placeholders in a given string.
        /// </summary>
        /// <param name="format">The given string.</param>
        /// <returns>The number of placeholders in the string.</returns>
        /// <DevDoc>
        /// Made internal for testing.
        /// </DevDoc>
        // ReSharper disable once MemberCanBePrivate.Global
        internal static int CountFormatPlaceholders(string format)
        {
            return CountRegex.Matches(format).Count;
        }

        #endregion
    }
}