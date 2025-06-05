// -----------------------------------------------------------------------
// <copyright file = "FrameworkDebug.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Game.Core.Communication.CommunicationLib;
    using Interfaces;

    /// <summary>
    /// This class holds some internal functions to help with debugging.
    /// </summary>
    internal static class FrameworkDebug
    {
        [Conditional("DEBUG")]
        public static void LogState(string frameworkIdentifier, string stateName, StateStep step)
        {
            Game.Core.Logging.Log.Write($"Logic {frameworkIdentifier} {stateName} : {step}");
        }

        /// <summary>
        /// Helper function to dump a DataItems object to Log.
        /// </summary>
        /// <param name="dataItems">
        /// The DataItems object to dump.
        /// </param>
        /// <param name="dumpTitle">
        /// The title of the dump.  Could be used to identify the <paramref name="dataItems"/> being dumped.
        /// </param>
        [Conditional("DEBUG")]
        public static void DumpDataItems(DataItems dataItems, string dumpTitle = null)
        {
            var builder = new StringBuilder();

            builder.Append($"Dumping Data Items for {dumpTitle}...\n");

            if(dataItems == null)
            {
                _ = builder.Append("\tDataItems is null\n");
            }
            else
            {
                foreach(var kvp in dataItems)
                {
                    _ = builder.Append($"\tProvider Name = {kvp.Key}\n");
                    foreach(var serviceData in kvp.Value)
                    {
                        _ = builder.Append($"\tservice id [{serviceData.Key}] = {serviceData.Value}\n");
                    }

                    builder.Append("\n");
                }
            }

            Game.Core.Logging.Log.Write(builder.ToString());
        }

        /// <summary>
        /// Helper function to dump a service request data map, which is a dictionary keyed by state names,
        /// each state has a <see cref="ServiceRequestData"/>.
        /// </summary>
        /// <remarks>
        /// This is helpful for debugging whether a game's logic data is configured correctly.
        /// </remarks>
        /// <param name="map">
        /// The map object to dump.
        /// </param>
        /// <param name="mapTitle">
        /// The title of the map.
        /// </param>
        [Conditional("DEBUG")]
        public static void DumpServiceRequestDataMap(IDictionary<string, ServiceRequestData> map, string mapTitle = null)
        {
            var builder = new StringBuilder();

            builder.Append($"******************** ServiceRequestData Map {mapTitle} ************************\n");
            builder.Append("\n");

            foreach(var stateEntry in map)
            {
                builder.Append($"StateName = {stateEntry.Key}\n");

                foreach(var providerEntry in stateEntry.Value)
                {
                    builder.Append($"\t{providerEntry.Key}\n");

                    foreach(var accessor in providerEntry.Value)
                    {
                        builder.Append($"\t\t[{accessor.Identifier}]: {accessor.Service} | {accessor.NotificationType}\n");
                    }
                }
            }

            Game.Core.Logging.Log.Write(builder.ToString());
        }
    }
}