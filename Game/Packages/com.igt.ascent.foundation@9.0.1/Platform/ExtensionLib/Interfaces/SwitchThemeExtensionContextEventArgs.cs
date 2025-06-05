// -----------------------------------------------------------------------
// <copyright file = "SwitchThemeExtensionContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for the switch theme extension context event which occurs
    /// when the extension should change to a new theme extension context without
    /// inactivating the current one first.
    /// </summary>
    public class SwitchThemeExtensionContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the theme identifier of the theme context.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets the payvar identifier for the theme context.
        /// </summary>
        public string PayvarIdentifier { get; }

        /// <summary>
        /// Gets the denomination of the theme context.
        /// </summary>
        public uint Denomination { get; }

        /// <summary>
        /// Constructs an instance of <see cref="SwitchThemeExtensionContextEventArgs"/>.
        /// </summary>
        /// <param name="themeIdentifier">Theme identifier of the theme context being activated.</param>
        /// <param name="payvarIdentifier">Payvar identifier of the theme context being activated.</param>
        /// <param name="denomination">Denomination of the theme context being activated.</param>
        public SwitchThemeExtensionContextEventArgs(string themeIdentifier, string payvarIdentifier, uint denomination)
        {
            ThemeIdentifier = themeIdentifier;
            PayvarIdentifier = payvarIdentifier;
            Denomination = denomination;
        }
    }
}