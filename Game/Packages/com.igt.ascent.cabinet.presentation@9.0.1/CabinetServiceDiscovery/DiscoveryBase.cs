////-----------------------------------------------------------------------
//// <copyright file = "DiscoveryBase.cs" company = "IGT">
////     Copyright (c) IGT 2019-2021.  All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServiceDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// An abstract class that allows discovery of MEF compatible assemblies.
    /// </summary>
    public abstract class DiscoveryBase : IDisposable
    {
        #region fields

        /// <summary>
        /// A list of the last MEF config. file parse and discovery results.
        /// </summary>
        private readonly List<DiscoveryResult> lastDiscoveryResults;

        /// <summary>
        /// The <see cref="CompositionContainer"/> representing all discovered MEF assemblies.
        /// </summary>
        protected CompositionContainer Container;

        /// <summary>
        /// The <see cref="AggregateCatalog"/> representing all discovered MEF assemblies.
        /// </summary>
        protected readonly AggregateCatalog Catalog;

        #endregion

        #region Constructor

        /// <summary>
        /// Base constructor. Parses the MEF configuration file and attempts to load applicable MEF DLLs in the specified
        /// directories.
        /// </summary>
        /// <param name="gameMountPoint">The game root directory as passed by a game lib or other lib.</param>
        protected DiscoveryBase(string gameMountPoint)
        {
            Catalog = new AggregateCatalog();
            Container = new CompositionContainer();
            Container.ExportsChanged += OnContainerExportsChanged;
            lastDiscoveryResults = DiscoveryPathManager.GetMefDirectories(gameMountPoint, out var discoveryDirectoriesList).ToList();
            lastDiscoveryResults.AddRange(DiscoverDefaultDirectories(discoveryDirectoriesList.ToList()));
        }

        #endregion

        #region Public

        /// <summary>
        /// Returns a collection of <see cref="DiscoveryResult"/> from the last discovery process.
        /// </summary>
        public IList<DiscoveryResult> LastDiscoveryResults => new ReadOnlyCollection<DiscoveryResult>(lastDiscoveryResults);

        /// <summary>
        /// Gets the composition container.
        /// </summary>
        public CompositionContainer CompositionContainer => Container;

        /// <summary>
        /// Adds a path containing candidate MEF assemblies to discover.
        /// </summary>
        /// <param name="directoryToDiscover">A path to search for MEF assemblies.</param>
        /// <returns>A collection of <see cref="DiscoveryResult"/> messages describing the results from adding a path to discover.</returns>
        public IEnumerable<DiscoveryResult> AddDiscoveryDirectory(string directoryToDiscover)
        {
            return AddDiscoveryPathInternal(directoryToDiscover);
        }

        /// <summary>
        /// Removes a directory from the list of search locations.
        /// </summary>
        /// <param name="searchLocation">The path to remove.</param>
        public void RemoveDiscoveryDirectory(string searchLocation)
        {
            bool SearchFunction(ComposablePartCatalog individualCatalog)
            {
                return individualCatalog is DirectoryCatalog dirCatalog && dirCatalog.Path == searchLocation;
            }

            var catalogToRemove = Catalog.Catalogs.FirstOrDefault(SearchFunction);
            if(catalogToRemove != null)
            {
                Catalog.Catalogs.Remove(catalogToRemove);
            }
        }

        /// <summary>
        /// Clears all previously entered search directories.
        /// </summary>
        public virtual void ClearAllDiscoveryDirectories()
        {
           Catalog.Catalogs.Clear();
        }

        #endregion

        #region Protected and Private 

        /// <summary>
        /// Handles the container exports changed event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="exportsChangeEventArgs">Event arguments of type <see cref="ExportsChangeEventArgs"/>.</param>
        protected abstract void OnContainerExportsChanged(object sender, ExportsChangeEventArgs exportsChangeEventArgs);

        /// <summary>
        /// Attempt to discover MEF exports in specified default directories.
        /// </summary>
        /// <returns>The number of existing directories specified in the MEF configuration file. </returns>
        private List<DiscoveryResult> DiscoverDefaultDirectories(List<string> discoveryPathList)
        {
            var discoveryResults = new List<DiscoveryResult>();

            if(discoveryPathList != null && discoveryPathList.Any())
            {
                foreach(var path in discoveryPathList)
                {
                    if(Directory.Exists(path))
                    {
                        discoveryResults.AddRange(AddDiscoveryPathInternal(path));
                    }
                }
            }

            return discoveryResults;
        }

        /// <summary>
        /// Add a single path to the discovery list.
        /// </summary>
        /// <param name="pathToDiscover">The full path to add to the discovery list.</param>
        /// <returns>A collection of <see cref="DiscoveryResult"/> indicating the results of the discovery path addition.</returns>
        private IEnumerable<DiscoveryResult> AddDiscoveryPathInternal(string pathToDiscover)
        {
            var results = new List<DiscoveryResult>();

            // Sanity check on the path supplied.
            if(string.IsNullOrEmpty(pathToDiscover) || !Directory.Exists(pathToDiscover))
            {
                results.Add(new DiscoveryResult(DiscoveryResultType.Failure,
                    $@"The path supplied: {pathToDiscover} was null, empty or non-existent."));
                return results;
            }

            results.Add(new DiscoveryResult(DiscoveryResultType.Informational,
                $@"Started discovering in directory: {pathToDiscover}."));

            // Let MEF scan and discover assemblies that satisfy general MEF export requirements.
            try
            {
                Catalog.Catalogs.Add(new DirectoryCatalog(pathToDiscover));
                if(Catalog.Parts.Any())
                {
                    Container = new CompositionContainer(Catalog);
                }
                else
                {
                    results.Add(new DiscoveryResult(DiscoveryResultType.Failure, @"No applicable MEF assemblies discovered."));
                }
            }
            catch(Exception ex)
            {
                results.Add(new DiscoveryResult(DiscoveryResultType.Failure,
                    $"Exception during the MEF discover/filter process: {ex.Message}."));
            }
            finally
            {
                if(Catalog?.Catalogs != null)
                {
                    foreach(var thisCat in Catalog.Catalogs)
                    {
                        var resString = $"Discovered catalog: {thisCat}";
                        results.Add(new DiscoveryResult(DiscoveryResultType.Informational, resString));
                    }
                }
            }

            results.Add(new DiscoveryResult(DiscoveryResultType.Informational, @"Ended discovering in directory: {0}."));

            return results;
        }

        #endregion

        #region IDispose Implementation

        private bool disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Protected implementation of base Dispose pattern.
        /// Override this in any derived classes if additional clean-up is required.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposed)
            {
                return;
            }
      
            if(disposing)
            {
                Catalog.Dispose();
                Container.ExportsChanged -= OnContainerExportsChanged;
                Container.Dispose();
            }
      
            disposed = true;
        }

        #endregion
    }
}
