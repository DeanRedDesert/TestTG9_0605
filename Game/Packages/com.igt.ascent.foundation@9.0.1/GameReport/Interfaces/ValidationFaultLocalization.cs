//-----------------------------------------------------------------------
// <copyright file = "ValidationFaultLocalization.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    using System;

    /// <summary>
    /// Represents a localized fault description.
    /// </summary>
    public class ValidationFaultLocalization
    {
        /// <summary>
        /// The max number of characters in the title of fault description.
        /// </summary>
        private const int MaxTitleLength = 39;

        /// <summary>
        /// The max number of characters in the message of fault description.
        /// </summary>
        private const int MaxMessageLength = 119;

        /// <summary>
        /// Gets the culture of fault description.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets the title of fault description.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the content of fault description.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Instantiates a new <see cref="ValidationFaultLocalization"/>.
        /// </summary>
        /// <param name="culture">The culture of fault description.</param>
        /// <param name="title">
        /// The title of fault description.
        /// Title characters exceeding <see cref="MaxTitleLength"/> will be truncated.
        /// </param>
        /// <param name="message">
        /// The message of fault description.
        /// Message characters exceeding <see cref="MaxMessageLength"/> will be truncated.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="title"/> or <paramref name="message"/> is null or empty,
        /// </exception>
        public ValidationFaultLocalization(string culture, string title, string message)
        {
            Culture = culture;

            if(string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }
            if(string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            Title = title.Length > MaxTitleLength ? title.Substring(0, MaxTitleLength) : title;
            Message = message.Length > MaxMessageLength ? message.Substring(0, MaxMessageLength) : message;
        }
    }
}
