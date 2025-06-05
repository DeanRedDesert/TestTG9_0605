// -----------------------------------------------------------------------
// <copyright file = "ResourcesAssembly.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Build.EmbeddedResources
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This class looks up the assembly with name [CurrentAssemblyName].resources from the loaded assemblies
    /// or from the files under the same folder.
    /// For example, if the current assembly name is "A.dll", it'll look up assembly "A.resources" from the
    /// loaded assemblies. If "A.resources" is not loaded, it'll look up the file "A.resources.dll" and load it
    /// if the file exists.
    /// If the target resources assembly cannot be found, the current assembly is used as its resources assembly.
    /// This code won't work if it's invoked from outside the assembly where "ResourcesAssembly" is located.
    /// </summary>
    internal static class ResourcesAssembly
    {
        /// <summary>
        /// The resources assembly.
        /// </summary>
        private static Assembly resourcesAssembly;

        /// <summary>
        /// The lock object used to synchronize the creation of <see cref="resourcesAssembly"/>.
        /// </summary>
        private static readonly object ResourcesAssemblyLockObject = new object();

        /// <summary>
        /// Gets the current resources assembly.
        /// </summary>
        /// <summary>
        /// Returns assembly "[CurrentAssemblyName].resources" if it's loaded; otherwise, looks up file
        /// "[CurrentAssemblyName].resources.dll" under the same folder and loads it if found;
        /// otherwise, returns current assembly.
        /// </summary>
        public static Assembly GetCurrent()
        {
            // avoid locks after resourcesAssembly is initialized.
            if(resourcesAssembly == null)
            {
                lock(ResourcesAssemblyLockObject)
                {
                    if(resourcesAssembly == null)
                    {
                        resourcesAssembly = LoadResourcesAssembly();
                    }
                }
            }
            
            return resourcesAssembly;
        }

        /// <summary>
        /// Loads the designated resources assembly.
        /// </summary>
        /// <returns>
        /// Returns assembly "[CurrentAssemblyName].resources" if it's loaded; otherwise, looks up file
        /// "[CurrentAssemblyName].resources.dll" under the same folder and loads it if found;
        /// otherwise, returns current assembly.
        /// </returns>
        private static Assembly LoadResourcesAssembly()
        {
            var assembly = Assembly.GetAssembly(typeof(ResourcesAssembly));
            
            // Search from the loaded assemblies for the resources assembly.
            var assemblyName = assembly.GetName().Name;
            var resourcesAssemblyName = assemblyName + ".Resources";
            var resAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                asm => string.Equals(asm.GetName().Name, resourcesAssemblyName,
                    StringComparison.CurrentCultureIgnoreCase));
            if(resAssembly != null)
            {
                return resAssembly;
            }

            // Search files for the resources assembly.
            var assemblyPath = assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyPath);
            var assemblyFileName = Path.GetFileNameWithoutExtension(assemblyPath);
            if(assemblyDir != null)
            {
                var resourcesAssemblyFileName = Path.Combine(assemblyDir, assemblyFileName + ".resources.dll");
                if(File.Exists(resourcesAssemblyFileName))
                {
                    resAssembly = Assembly.LoadFrom(resourcesAssemblyFileName);
                }
            }

            return resAssembly ?? assembly;
        }
    }
}