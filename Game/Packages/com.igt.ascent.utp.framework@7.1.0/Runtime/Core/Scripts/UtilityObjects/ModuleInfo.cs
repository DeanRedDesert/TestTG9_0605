// -----------------------------------------------------------------------
//  <copyright file = "ModuleInfo.cs" company = "IGT">
//      Copyright (c) 2020 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Utp.Framework.UtilityObjects
{
    /// <summary>
    /// Class for wrapping up information about modules to be serialized and sent to UTP clients.
    /// One of the main purposes for this is to help clients distinct between modules that share the same base class.
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// The <see cref="AutomationModule.Name" /> property of the module.
        /// </summary>
        public string ModuleName;

        /// <summary>
        /// The <see cref="AutomationModule.Description" /> property of the module.
        /// </summary>
        public string ModuleDescription;

        /// <summary>
        /// The class name of the module.
        /// </summary>
        public string ClassName;

        /// <summary>
        /// The namespace of the module.
        /// </summary>
        public string ClassNamespace;

        /// <summary>
        /// The base class name of the module.
        /// </summary>
        public string BaseClassName;

        /// <summary>
        /// The namespace of the base class.
        /// </summary>
        public string BaseClassNamespace;

        /// <summary>
        /// The module version.
        /// </summary>
        public string Version;

        /// <summary>
        /// Whether or not the module is enabled.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Default constructor required for serialization.
        /// </summary>
        public ModuleInfo()
        {

        }

        /// <summary>
        /// Constructor for easily creating and populating a new ModuleInfo.
        /// </summary>
        /// <param name="am">The <see cref="AutomationModule"/> this ModuleInfo is based on.</param>
        public ModuleInfo(AutomationModule am)
        {
            ModuleName = am.Name;
            ModuleDescription = am.Description;

            var moduleType = am.GetType();
            ClassName = moduleType.Name;
            ClassNamespace = moduleType.Namespace;

            if(moduleType.BaseType != null)
            {
                BaseClassName = moduleType.BaseType.Name;
                BaseClassNamespace = moduleType.BaseType.Namespace;
            }

            Version = am.ModuleVersion.ToString();

            Enabled = am.ModuleStatus == ModuleStatuses.InitializedEnabled;
        }
    }
}
