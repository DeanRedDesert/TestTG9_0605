////-----------------------------------------------------------------------
//// <copyright file = "DeviceDiscovery.cs" company = "IGT">
////     Copyright (c) IGT 2019.  All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServiceDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using CabinetServices;

    /// <summary>
    /// A class that allows discovery of MEF compatible cabinet service device assemblies.
    /// </summary>
    public class DeviceDiscovery : DiscoveryBase
    {
        // Disable the unused variable warning since it is used at runtime with MEF.
        #pragma warning disable 0649

        [ImportMany(AllowRecomposition = true)]
        private readonly IList<IDeviceService> allDeviceInterfaces;
                
        #pragma warning restore 0649

        /// <summary>
        /// Gets all of the loaded device interface plugins that support the <see cref="IDeviceService"/> interface
        /// which have been discovered during construction and via the addition of additional MEF plugin
        /// directories.
        /// </summary>
        public IEnumerable<IDeviceService> AllDeviceInterfaces => new ReadOnlyCollection<IDeviceService>(allDeviceInterfaces);

        /// <summary>
        /// Constructor. Composes the MEF container and passes the game mount point to the base constructor,
        /// which will scan for and load MEF assemblies.
        /// </summary>
        /// <param name="gameMountPoint">The game root directory as passed by a game lib or other lib.</param>
        public DeviceDiscovery(string gameMountPoint) : base(gameMountPoint)
        {
            allDeviceInterfaces = new List<IDeviceService>();
            Container.ComposeParts(this);
        }

        /// <inheritdoc/>
        public override void ClearAllDiscoveryDirectories()
        {
            base.ClearAllDiscoveryDirectories();
            allDeviceInterfaces.Clear();
        }

        /// <summary>
        /// Filters on all previously discovered MEF compatible DLLs that export <see cref="IDeviceService"/>.
        /// </summary>
        /// <returns>A collection of <see cref="DiscoveryResult"/>.</returns>
        public IList<DiscoveryResult> FindCabinetDeviceInterfaces()
        {
            var results = new List<DiscoveryResult>();
            allDeviceInterfaces.Clear();

            // Filter on any previously discovered MEF DLLs according to the appropriate interfaces.
            try
            {
                if(Catalog?.Parts.Any() == true)
                {
                    var iDeviceServices = Container.GetExportedValues<IDeviceService>().ToList();

                    foreach(var iDeviceService in iDeviceServices)
                    {
                        allDeviceInterfaces.Add(iDeviceService);
                        results.Add(new DiscoveryResult(DiscoveryResultType.Success,
                            $"Added a compliant IDeviceService export: {iDeviceService}."));
                    }
                }
                else
                {
                    results.Add(new DiscoveryResult(DiscoveryResultType.Failure, @"No MEF assemblies discovered. Ensure that one or more directories containing valid DLLs has been previously added."));
                }
            }
            catch(Exception ex)
            {
                results.Add(new DiscoveryResult(DiscoveryResultType.Failure,
                    $"Exception during the MEF discover/filter process: {ex.Message}."));
            }
            finally
            {
                results.Add(new DiscoveryResult(DiscoveryResultType.Informational, @"MEF discovery/filtering process done."));
            }

            return results;
        }

        #region Discovery Base overrides 
        
        /// <inheritdoc/> 
        protected override void OnContainerExportsChanged(object sender, ExportsChangeEventArgs exportsChangeEventArgs)
        {
            allDeviceInterfaces.Clear(); 
        }

        #endregion
    }
}