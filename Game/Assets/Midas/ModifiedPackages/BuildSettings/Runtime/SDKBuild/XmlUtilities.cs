// -----------------------------------------------------------------------
//  <copyright file = "XmlUtilities.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild
{
    using System.IO;
    using System.Xml.Serialization;
    using UnityEngine;

    /// <summary>
    /// Provides utility methods for loading and saving XML files.
    /// </summary>
    public static class XmlUtilities
    {
        /// <summary>
        /// Loads an XML file from the specified location.
        /// </summary>
        /// <param name="fileName">File name to load from.</param>
        /// <returns>Stored XML object, or null if no file exists or the wrong type was specified.</returns>
        public static TData LoadFrom<TData>(string fileName)
        {
            var result = default(TData);

            try
            {
                if(!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                {
                    var serializer = new XmlSerializer(typeof(TData));
                    using(var reader = new StreamReader(fileName))
                    {
                        result = (TData)serializer.Deserialize(reader);
                    }
                }
            }
            catch
            {
                // Return default value.
            }

            return result;
        }

        /// <summary>
        /// Saves an object to an XML file to the specified location.
        /// </summary>
        /// <param name="fileName">File name to save to.</param>
        /// <param name="xmlObject">Object to save.</param>
        public static void SaveTo<TData>(string fileName, TData xmlObject)
        {
            ClearReadOnlyFlag(fileName);

            var serializer = new XmlSerializer(typeof(TData));
            using(var writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, xmlObject);
            }
        }

        /// <summary>
        /// Clears the read only flag from the specified file if it exists and is read only.
        /// </summary>
        /// <param name="filePath">Full path to the file.</param>
        private static void ClearReadOnlyFlag(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.IsReadOnly)
                {
                    Debug.Log("Clearing read only flag on " + filePath);
                    fileInfo.IsReadOnly = false;
                }
            }
        }
    }
}