// -----------------------------------------------------------------------
// <copyright file = "CultureChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Event arguments that indicate that the current culture has changed.
    /// </summary>
    [Serializable]
    public class CultureChangedEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the context for the culture that has changed.
        /// </summary>
        public CultureContext AffectedContext { get; private set; }

        /// <summary>
        /// Gets the current culture in the format of: LanguageCode-RegionCode_Dialect.
        /// </summary>
        public string Culture { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="CultureChangedEventArgs"/>.
        /// </summary>
        /// <param name="affectedContext">
        /// The context for the culture that has changed.
        /// </param>
        /// <param name="newCulture">
        /// The new culture in the format of: LanguageCode-RegionCode_Dialect.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the parameter <paramref name="newCulture"/> is null.
        /// </exception>
        public CultureChangedEventArgs(CultureContext affectedContext, string newCulture)
        {
            AffectedContext = affectedContext;
            Culture = newCulture ?? throw new ArgumentNullException(nameof(newCulture));
        }


        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("CultureChangedEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t AffectedContext: " + AffectedContext);
            builder.AppendLine("\t Culture: " + Culture);

            return builder.ToString();
        }

        #endregion
    }
}
