//-----------------------------------------------------------------------
// <copyright file = "CategoryInstalledEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using CsiTransport;

    /// <summary>
    /// Event indicating that a category has been installed.
    /// </summary>
    internal class CategoryInstalledEventArgs : EventArgs
    {
        /// <summary>
        /// Category which was installed.
        /// </summary>
        public ICabinetCategory InstalledCategory { get; }

        /// <summary>
        /// Construct an instance of the event.
        /// </summary>
        /// <param name="installedCategory">The category which was installed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="installedCategory"/> is null.
        /// </exception>
        public CategoryInstalledEventArgs(ICabinetCategory installedCategory)
        {
            InstalledCategory = installedCategory ?? throw new ArgumentNullException(nameof(installedCategory));
        }
    }
}