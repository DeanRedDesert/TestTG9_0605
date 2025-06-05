// -----------------------------------------------------------------------
// <copyright file = "IParcelCommMockAdapter.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface indicates an adapter that hooks up a custom parcel comm mock implementation
    /// with the SDK IxxxParcelComm implementations in standalone running environment.
    /// </summary>
    /// <remarks>
    /// Even though the mock object is originally provided to SDK by the user, this interface
    /// allows the user to retrieve it from SDK IxxxLib, which might be more convenient than
    /// the user having to cache it in user's code.
    /// 
    /// Usage Example:
    /// <code>
    /// var adapter = shellLib.GameParcelComm as IParcelCommMockAdapter;
    /// if(adapter != null)
    /// {
    ///     var myMock = adapter.MockObject as MyMock
    /// }
    /// </code>
    /// </remarks>
    public interface IParcelCommMockAdapter
    {
        /// <summary>
        /// Gets the mock object that implements the custom parcel comm.
        /// </summary>
        object MockObject { get; }
    }
}