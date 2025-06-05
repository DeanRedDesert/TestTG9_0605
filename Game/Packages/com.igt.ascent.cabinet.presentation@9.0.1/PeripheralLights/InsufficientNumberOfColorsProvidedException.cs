//-----------------------------------------------------------------------
// <copyright file = "InsufficientNumberOfColorsProvidedException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that an insufficient number of colors was provided.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"), Serializable]
    public class InsufficientNumberOfColorsProvidedException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "An insufficient number of colors were specified. Expecting: {0} Got: {1}.";

        /// <summary>
        /// The number of colors that was expected.
        /// </summary>
        public int ExpectedNumberOfColors
        {
            get;
            protected set;
        }

        /// <summary>
        /// The number of colors received.
        /// </summary>
        public int ActualNumberOfColors
        {
            get;
            protected set;
        }

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="expected">The expected number of colors.</param>
        /// <param name="actual">The actual number of colors provided.</param>
        public InsufficientNumberOfColorsProvidedException(int expected, int actual):
            base(string.Format(CultureInfo.InvariantCulture, MessageFormat, expected, actual))
        {
            ExpectedNumberOfColors = expected;
            ActualNumberOfColors = actual;
        }
    }
}
