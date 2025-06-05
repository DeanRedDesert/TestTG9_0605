//-----------------------------------------------------------------------
// <copyright file = "F2XRegistryVer1.Extensions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2X.F2XRegistryVer1
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using F2X;

    /// <summary>
    /// Extensions to the <see cref="Registry"/> class. These extensions primarily allow loading of a specified registry type
    /// embedded in the "Body" element of the containing registry.
    /// </summary>
    public partial class Registry
    {
        /// <summary>
        /// Object for parsing the generic registry container.
        /// </summary>
        private static readonly XmlSerializer ContainerRegistryXmlSerializer = new XmlSerializer(typeof(Registry));

        /// <summary>
        /// Gets the registry embedded in the given container registry file.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the .xml container registry file, including the absolute or relative path,
        /// and the full file name with extension.
        /// </param>
        /// <returns>
        /// The registry embedded in the given container registry file.
        /// </returns>
        public static Registry GetRegistry(string registryFile)
        {
            Registry containerReg;

            using(var fileStream = new FileStream(registryFile, FileMode.Open, FileAccess.Read))
            {
                containerReg = GetRegistryFromStream(fileStream);
            }

            return containerReg;
        }

        /// <summary>
        /// Loads a container registry containing registry information of the specified type <typeparamref name="TRegType"/>.
        /// </summary>
        /// <typeparam name="TRegType">
        /// The type of the registry file embedded in this container registry.
        /// </typeparam>
        /// <param name="containerReg">
        /// The container registry containing registry information of the specified type <typeparamref name="TRegType"/>.
        /// </param>
        /// <returns>
        /// A registry object of type <typeparamref name="TRegType"/>
        /// </returns> 
        /// <exception cref="InvalidOperationException">
        /// Thrown if the container registry is malformed, not of a supporte version, or the contained registry is malformed,
        /// not of a supported version, or not of the correct type specified in the generic parameter.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the specified registry type and the registry type enumeration defined in the container are not equal.
        /// </exception>
        public static TRegType Load<TRegType>(Registry containerReg)
        {
            TRegType registry;

            // Sanity check to ensure the specified type matches the type enum specified in the container.
            if(!F2XRegistryLookup.TypesMatch(typeof(TRegType), containerReg.RegistryType))
            { 
                throw new ArgumentException(
                    $"The type specified ({typeof(TRegType)}) does not match the embedded type in the" +
                    $" top level of the container registry ({containerReg.RegistryType}).");
            }

            // Since the body is an XmlElement, write it to memory and then deserialize it.
            using(var memoryStream = new MemoryStream())
            {
                using(var xmlWriter = XmlWriter.Create(memoryStream))
                {
                    containerReg.Body.WriteTo(xmlWriter);
                }

                // Reset and deserialize the XML.
                memoryStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    var embeddedRegistrySerializer = new XmlSerializer(typeof(TRegType));
                    registry = (TRegType)embeddedRegistrySerializer.Deserialize(memoryStream);
                }
                catch(Exception ex)
                {
                    if(ex is InvalidOperationException)
                    {
                        throw new InvalidOperationException("Contained registry file cannot be loaded; it is" +
                                                            "either malformed, or not of the type specfied. " +
                                                            $"Type defined in container: {containerReg.RegistryType}. Type specified " +
                                                            $"for load: {typeof(TRegType)}. Message: {ex.Message}");
                    }

                    throw;
                }
            }

            return registry;
        }

        /// <summary>
        /// Loads an xml registry file of the specified type <typeparamref name="TRegType"/>.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the xml registry file, including the absolute or relative path, and the full file name with extension.
        /// </param>
        /// <typeparam name="TRegType">
        /// The type of the registry file embedded in this container registry.
        /// </typeparam>
        /// <returns>
        /// Registry object of type <typeparamref name="TRegType"/>created from an registry Xml.
        /// </returns>
        public static TRegType Load<TRegType>(string registryFile)
        {
            var containerReg = GetRegistry(registryFile);
            return Load<TRegType>(containerReg);
        }

        /// <summary>
        /// Gets the registry embedded in the given container registry file's stream.
        /// </summary>
        /// <param name="stream">Stream of the registry file</param>
        /// <returns>The deserialized registry file from the stream.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the container registry file cannot be loaded. It is either malformed, or not of a supported version.
        /// </exception>
        public static Registry GetRegistryFromStream(Stream stream)
        {
            Registry containerReg;

            try
            {
                containerReg = (Registry)ContainerRegistryXmlSerializer.Deserialize(stream);
            }
            catch(Exception ex)
            {
                if(ex is InvalidOperationException)
                {
                    throw new InvalidOperationException(
                        $"Container registry file stream cannot be loaded. It is either malformed, or not of a supported version. Message: {ex.Message}.");
                }

                throw;
            }

            return containerReg;
        }
    }
}