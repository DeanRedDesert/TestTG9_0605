// -----------------------------------------------------------------------
// <copyright file = "IInnerLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    /// <summary>
    /// This interface defines functionality of an Inner Lib, which represents an "inner" context
    /// which is only available on a specific negotiation level.
    /// 
    /// An Inner Lib may or may not be connected/active during the lifetime of the Extension Bin application.
    /// It depends on what types of executable extensions are present in the Extension Bin (e.g. SystemExtensionLib),
    /// whether an executable extension is enabled at runtime (e.g. AppExtensionLib), and/or
    /// whether the linked game is selected for play (e.g. AscribedGameExtensionLib).
    ///
    /// It is strongly recommended that the functionality of the Inner Lib is only used when it is in active context,
    /// that is from receiving the ActivateContextEvent till receiving the InactivateContextEvent.
    ///
    /// Using APIs outside the active context could result in null references of services.
    /// Make sure to check for nullness before using them.
    /// </summary>
    public interface IInnerLib
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating whether there is an active context (i.e. there has been an ActivateContextEvent)
        /// on the specific negotiation level.
        /// </summary>
        bool IsContextActive { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an extended interface if it was requested and installed on the specific negotiation level.
        /// </summary>
        /// <typeparam name="TExtendedInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. <see langword="null"/> if none was found.
        /// </returns>
        TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class;

        #endregion
    }
}