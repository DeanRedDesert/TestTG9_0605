//-----------------------------------------------------------------------
// <copyright file = "PayvarGroupRegistryProxy.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using F2XPayvarGroupRegistryVerTip = Core.Registries.Internal.F2X.F2XPayvarGroupRegistryVer1;

    /// <summary>
    /// A proxy for the <see cref="F2XPayvarGroupRegistryVerTip"/> object that is used to
    /// retrieve information from the payvar group registry.
    /// </summary>
    internal class PayvarGroupRegistryProxy : IPayvarGroupRegistry
    {
        private string themeRegistryFileName;

        #region IPayvarGroupRegistry

        /// <inheritdoc />
        public string GroupTemplateName { get; }

        /// <inheritdoc />
        public IList<Payvar> Payvars { get; }

        /// <inheritdoc />
        public string ThemeRegistryFileName
        {
            get => themeRegistryFileName;
            set
            {
                if(!string.IsNullOrEmpty(themeRegistryFileName) &&
                   !themeRegistryFileName.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Cannot assign two different theme registries, {themeRegistryFileName} and {value}, to the same group registry, {GroupTemplateName}.");
                }

                themeRegistryFileName = value;
                foreach(var payvar in Payvars)
                {
                    payvar.ThemeRegistryFileName = value;
                }
            }
        }

        #endregion IPayvarGroupRegistry

        #region Constructor

        /// <summary>
        /// Construct a <see cref="F2XPayvarGroupRegistryVerTip"/> proxy with the payvar group registry object.
        /// </summary>
        /// <param name="payvarGroupRegistry">
        /// The <see cref="F2XPayvarGroupRegistryVerTip"/> that defines a list of payvars that will be initialized using the
        /// payvar registry group template.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="payvarGroupRegistry"/> is null.
        /// </exception>
        public PayvarGroupRegistryProxy(F2XPayvarGroupRegistryVerTip.PayvarGroupRegistry payvarGroupRegistry)
        {
            if(payvarGroupRegistry == null)
            {
                throw new ArgumentNullException(nameof(payvarGroupRegistry));
            }

            GroupTemplateName = Path.GetFileNameWithoutExtension(payvarGroupRegistry.PayvarRegistry);

            Payvars = new List<Payvar>();

            foreach(var payvar in payvarGroupRegistry.PayvarGroup)
            {
                Payvars.Add(new Payvar
                    {
                        MaxBetRedefinition = GetMaxBetRedefinition(payvar.MaxBetMapping),
                        MinimumPaybackPercentage = decimal.ToSingle(payvar.MinimumPaybackPercentage),
                        PaybackPercentage = decimal.ToSingle(payvar.PaybackPercentage),
                        PaytableName = payvar.PaytableName,
                        // The mono compiler does not recognize the method group correctly, so lambda is used instead.
                        // ReSharper disable once ConvertClosureToMethodGroup
                        WagerCategories = payvar.WagerCategories.Select(
                                            wagerCategory => decimal.ToSingle(wagerCategory)).ToList(),
                        GroupPayvarTag = payvar.GroupTagDataFile.Tag,
                        GroupPayvarTagData = payvar.GroupTagDataFile.Value
                    });
            }

            themeRegistryFileName = string.Empty;
        }

        #endregion Constructor

        #region Private Methods

        /// <summary>
        /// Gets the max bet redefinition from the max bet mapping F2X schema type.
        /// </summary>
        /// <param name="maxBetMapping">
        /// The max bet mapping F2X schema type.
        /// </param>
        /// <returns>
        /// The max bet redefinition.
        /// </returns>
        private static IDictionary<long, long> GetMaxBetRedefinition(F2XPayvarGroupRegistryVerTip.MaxBetMapping maxBetMapping)
        {
            var maxBetRedefinition = new Dictionary<long, long>();

            foreach(var mapping in maxBetMapping.EnumerationMapping)
            {
                maxBetRedefinition[Convert.ToInt64(mapping.Enumeration)] = Convert.ToInt64(mapping.Value);
            }

            return maxBetRedefinition;
        }

        #endregion Private Methods
    }
}
