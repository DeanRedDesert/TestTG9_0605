//-----------------------------------------------------------------------
// <copyright file = "Payvar.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// The definition of one payvar in the payvar group registry, and its overloaded elements from the
    /// payvar group template (payvar registry).
    /// </summary>
    public class Payvar : IPayvarInfo
    {
        /// <summary>
        /// Stores the name of the theme registry, used to produce a unique paytable
        /// identifier for two Payvars in different themes using the same paytable name.
        /// </summary>
        private string themeRegistry;

        /// <summary>
        /// Gets the name of the payvar.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string PaytableName { get; set; }

        /// <summary>
        /// Gets the group tag for the payvar.
        /// </summary>
        public string GroupPayvarTag { get; set; }

        /// <summary>
        /// Gets the data for the <see cref="GroupPayvarTag"/>.
        /// </summary>
        public string GroupPayvarTagData { get; set; }

        /// <summary>
        /// Gets the theoretical maximum payback percentage (including progressive contributions where applicable).
        /// </summary>
        /// <remarks>
        /// If not specified, value is taken from the payvar group template.
        /// </remarks>
        public float PaybackPercentage { get; set; }

        /// <summary>
        /// Gets the theoretical minimum payback percentage.
        /// </summary>
        /// <remarks>
        /// If not specified, value is taken from the payvar group template.
        /// </remarks>
        public float MinimumPaybackPercentage { get; set; }

        /// <summary>
        /// Gets the theoretical minimum payback percentage excluding progressive contributions.
        /// </summary>
        /// <remarks>
        /// Value is always taken from the payvar group template.
        /// </remarks>
        public float MinPaybackPercentageWithoutProgressives { get; set; }

        /// <summary>
        /// Gets the Wager Category information.
        /// </summary>
        /// <remarks>
        /// The index is derived from the payvar template by matching the percentage value.
        /// Only percentages are allowed which are defined in the payvar template.
        /// Multiple declarations are allowed over more payvar game within a game group.
        /// </remarks>
        public IList<float> WagerCategories { get; set; }

        /// <summary>
        /// Gets the redefinition of a max bet value, for the payvar metering.
        /// </summary>
        /// <remarks>
        /// The key represents the max bet configured at the group level,
        /// and the value represents the corresponding max bet at the payvar level.
        /// </remarks>
        public IDictionary<long, long> MaxBetRedefinition { get; set; }

        /// <summary>
        /// A paytable identifier that is unique to each theme-paytable combination.
        /// </summary>
        public string PaytableIdentifier => $"{ThemeRegistryFileName}{(string.IsNullOrEmpty(ThemeRegistryFileName) ? string.Empty : "/")}{PaytableName}";

        /// <summary>
        /// The theme registry file name, used to make PaytableIdentifier
        /// unique for the same paytable used in two different themes.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string ThemeRegistryFileName
        {
            get => themeRegistry;
            set => themeRegistry = (value ?? string.Empty).Replace(@"\", "/");
        }

        /// <summary>
        /// Gets the information on payback percentages.
        /// </summary>
        public PaytablePaybackInfo PaybackInfo =>
            new PaytablePaybackInfo(PaytableIdentifier,
                (decimal)PaybackPercentage,
                (decimal)MinimumPaybackPercentage,
                (decimal)MinPaybackPercentageWithoutProgressives);
    }
}
