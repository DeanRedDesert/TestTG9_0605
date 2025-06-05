// -----------------------------------------------------------------------
// <copyright file = "LocalizationTypesConverters.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System.Collections.Generic;
    using System.Linq;
    using F2XLocalizationTypes = F2X.Schemas.Internal.LocalizationTypes;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2X schema types for common types
    /// defined in the LocalizationTypes schema.
    /// </summary>
    public static class LocalizationTypesConverters
    {
        /// <summary>
        /// Converts an instance of the public <see cref="LocalizationTable"/> to
        /// a new instance of the F2X internal <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.LocalizationTable"/>.
        /// </summary>
        /// <param name="publicTable">
        /// The instance of public type LocalizationTable.
        /// </param>
        /// <returns>
        /// The converted F2X internal type of <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.LocalizationTable"/>.
        /// </returns>
        public static F2XLocalizationTypes.LocalizationTable ToInternal(this LocalizationTable publicTable)
        {

            F2XLocalizationTypes.LocalizationTable f2XLocalizationTable = null;
            if(publicTable != null)
            {
                f2XLocalizationTable = new F2XLocalizationTypes.LocalizationTable
                {
                    Culture = publicTable.Locale,
                    Resource = publicTable.LocalizedResources.Select(resource =>
                        new F2XLocalizationTypes.LocalizedResource
                        {
                            Id = resource.Key,
                            Item = resource.Value.ToInternal()
                        }).ToList(),
                };
            }

            return f2XLocalizationTable;
        }

        /// <summary>
        /// Converts a public localized resource to an internal F2X type of <see cref="string"/>, or
        /// <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.ImageAsset"/>, <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.MovieAsset"/>, or
        /// <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.SoundAsset"/>, <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.FileAsset"/>.
        /// </summary>
        /// <param name="resource">
        /// Resource object to convert to internal type.
        /// </param>
        /// <returns>
        /// The converted internal localized object.
        /// </returns>
        public static object ToInternal(this LocalizedResource resource)
        {
            switch(resource.Type)
            {
                case LocalizedResourceType.File:
                    return new F2XLocalizationTypes.FileAsset { Path = resource.Value };

                case LocalizedResourceType.Image:
                    return new F2XLocalizationTypes.ImageAsset { Path = resource.Value };

                case LocalizedResourceType.Sound:
                    return new F2XLocalizationTypes.SoundAsset { Path = resource.Value };

                case LocalizedResourceType.Movie:
                    return new F2XLocalizationTypes.MovieAsset { Path = resource.Value };

                default:
                    return resource.Value;
            }
        }

        /// <summary>
        /// Converts an internal type of localized resource to a public type.
        /// </summary>
        /// <param name="f2XResource">
        /// F2X resource object to convert to public type.
        /// </param>
        /// <returns>
        /// The converted public localized resource.
        /// </returns>
        public static LocalizedResource ToPublic(this F2XLocalizationTypes.LocalizedResource f2XResource)
        {
            if(f2XResource == null || f2XResource.Item == null)
            {
                return null;
            }

            LocalizedResourceType resourceType;
            string resourcePath;
            var itemType = f2XResource.Item.GetType();
            if(itemType == typeof(F2XLocalizationTypes.FileAsset))
            {
                resourceType = LocalizedResourceType.File;
                resourcePath = ((F2XLocalizationTypes.FileAsset)f2XResource.Item).Path;
            }
            else if(itemType == typeof(F2XLocalizationTypes.ImageAsset))
            {
                resourceType = LocalizedResourceType.Image;
                resourcePath = ((F2XLocalizationTypes.ImageAsset)f2XResource.Item).Path;
            }
            else if(itemType == typeof(F2XLocalizationTypes.SoundAsset))
            {
                resourceType = LocalizedResourceType.Sound;
                resourcePath = ((F2XLocalizationTypes.SoundAsset)f2XResource.Item).Path;
            }
            else if(itemType == typeof(F2XLocalizationTypes.MovieAsset))
            {
                resourceType = LocalizedResourceType.Movie;
                resourcePath = ((F2XLocalizationTypes.MovieAsset)f2XResource.Item).Path;
            }
            else
            {
                resourceType = LocalizedResourceType.String;
                resourcePath = f2XResource.Item as string;
            }
            return new LocalizedResource(resourceType, resourcePath);
        }

        /// <summary>
        /// Returns a list of SDK compatible <see cref="LocalizationTable"/>.
        /// </summary>
        /// <param name="f2XLocalizationTables">A list of F2X LocalizationTables.</param>
        /// <returns>
        /// The converted list of <see cref="IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.LocalizationTypes.LocalizationTable"/> 
        /// to <see cref="LocalizationTable"/>.
        /// </returns>
        public static List<LocalizationTable> ToPublic(
            this IList<F2XLocalizationTypes.LocalizationTable> f2XLocalizationTables)
        {
            List<LocalizationTable> localizationTables = null;
            if(f2XLocalizationTables != null)
            {
                localizationTables = new List<LocalizationTable>();
                foreach(var f2XLocalizationTable in f2XLocalizationTables)
                {
                    var resources = new Dictionary<string, LocalizedResource>();
                    foreach(var localizedResource in f2XLocalizationTable.Resource)
                    {
                        resources[localizedResource.Id] = localizedResource.ToPublic();
                    }
                    localizationTables.Add(new LocalizationTable(
                        f2XLocalizationTable.Culture,
                        resources));
                }
            }
            return localizationTables;
        }
    }
}