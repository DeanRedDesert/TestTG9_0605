//-----------------------------------------------------------------------
// <copyright file = "IConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface defines the methods to request information about
    /// custom configuration items declared by the game or extension in registry files.
    /// </summary>
    /// <remarks>
    /// When an API method requires a <see cref="ConfigurationItemKey"/>, 
    /// <see cref="ConfigurationItemKey.ScopeIdentifier"/> can specify
    /// an identifier for a specific theme, payvar, configuration extension, 
    /// or can be left blank.
    /// It is up to the interface implementation class to decide whether and how
    /// to handle it when <see cref="ConfigurationItemKey.ScopeIdentifierMissing"/> is true.
    /// </remarks>
    public interface IConfigurationRead : IGenericConfigurationRead<ConfigurationItemKey, ConfigurationScopeKey>
    {
    }
}
