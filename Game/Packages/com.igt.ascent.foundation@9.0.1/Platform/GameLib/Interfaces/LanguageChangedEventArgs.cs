//-----------------------------------------------------------------------
// <copyright file = "LanguageChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Event arguments that indicate that the current language has changed.
    /// </summary>
    [Serializable]
    public class LanguageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The current language.
        /// </summary>
        public string Language { private set; get; }

        /// <summary>
        /// Construct an instance of the event arguments.
        /// </summary>
        /// <param name="newLanguage">The new language.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the parameter <paramref name="newLanguage"/> is null.
        /// </exception>
        public LanguageChangedEventArgs(string newLanguage)
        {
            Language = newLanguage ?? throw new ArgumentNullException(nameof(newLanguage), "Parameter may not be null.");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"LanguageChangedEvent -\t Language = {Language}";
        }
    }
}
