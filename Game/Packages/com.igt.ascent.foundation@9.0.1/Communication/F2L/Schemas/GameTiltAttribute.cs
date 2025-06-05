//-----------------------------------------------------------------------
// <copyright file = "GameTiltAttribute.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas
{
    /// <summary>
    /// All possible tilt attributes defined in F2LGameTiltVer1.xsd
    /// </summary>
    /// <remarks>
    /// With the way the schema is written, the generated code does not have the enum type definition,
    /// hence we have to define it ourselves.
    /// </remarks>
    public enum GameTiltAttribute
    {
        /// <summary>
        /// If this attribute is present, this tilt will block tilt game play.
        /// </summary>
        PreventGamePlay,

        /// <summary>
        /// If this attribute is present protocols will be notified of this tilt.
        /// </summary>
        NotifyProtocols,

        /// <summary>
        /// If this attribute is present the tilt is signifying a progressive link down.
        /// </summary>
        ProgressiveLinkDown,
    }
}