//-----------------------------------------------------------------------
// <copyright file = "ExtensionTiltClearedByAttendantEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces.TiltControl
{
    using System;
    using Platform.Interfaces.TiltControl;

    /// <summary>
    /// This class represents an extension tilt clear by an attendant event.
    /// </summary>
    [Serializable]
    public class ExtensionTiltClearedByAttendantEventArgs : TiltClearedByAttendantEventArgs
    {
        /// <summary>
        /// Gets the guid for the extension that posted the tilt that is being cleared.
        /// </summary>
        public Guid ExtensionGuid { get; private set; }

        /// <summary>
        /// Construct an instance of this class with the given name and guid.
        /// </summary>
        /// <param name="extensionGuid">The guid of the extension that posted this tilt.</param>
        /// <param name="tiltName">The name of the cleared tilt.</param>
        public ExtensionTiltClearedByAttendantEventArgs(Guid extensionGuid, string tiltName) : base(tiltName)
        {
            ExtensionGuid = extensionGuid;
        }
    }
}
