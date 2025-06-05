// -----------------------------------------------------------------------
// <copyright file = "TextLocalization.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// This class defines the text and its culture to display.
    /// </summary>
    [Serializable]
    public class TextLocalization : ICompactSerializable
    {
        #region Read-Only Properties

        /// <summary>
        ///  Gets the culture associated to the <see cref="Text"/>.
        /// </summary>
        public string Culture { get; private set; }

        /// <summary>
        /// Gets the text to display.
        /// </summary>
        public string Text { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="TextLocalization"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing <see cref="ICompactSerializable"/> must have
        /// a public parameter-less constructor.
        /// </remarks>
        public TextLocalization()
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="TextLocalization"/> with the given culture and text.
        /// </summary>
        /// <param name="culture">The culture in which <paramref name="text"/> is displayed.</param>
        /// <param name="text">The text to display.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="culture"/> or <paramref name="text"/> is null.
        /// </exception>
        public TextLocalization(string culture, string text)
        {
            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Culture);
            CompactSerializer.Write(stream, Text);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Culture = CompactSerializer.ReadString(stream);
            Text = CompactSerializer.ReadString(stream);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Culture({Culture})/Text({Text})";
        }
    }
}