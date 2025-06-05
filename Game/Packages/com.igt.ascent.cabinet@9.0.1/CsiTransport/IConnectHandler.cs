//-----------------------------------------------------------------------
// <copyright file = "IConnectHandler.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System.Xml.Serialization;
    using CSI.Schemas.Internal;

    /// <summary>
    /// Interface for the CSI connect handler. This class is responsible for negotiating CSI categories.
    /// </summary>
    public interface IConnectHandler
    {
        /// <summary>
        /// Serializer to use for connect messages.
        /// </summary>
        XmlSerializer MessageSerializer { get; }

        /// <summary>
        /// Method for handling connect messages.
        /// </summary>
        /// <param name="connectMessage">The connect message to handle.</param>
        void HandleMessage(CsiConnect connectMessage);

        /// <summary>
        /// Method which handles initialization.
        /// </summary>
        /// <param name="transport">Transport to use for communication.</param>
        void Initialize(CsiTransport transport);

        /// <summary>
        /// Method which handles de-initialization.
        /// </summary>
        /// <param name="transport">Transport to use for communication.</param>
        void Deinitialize(CsiTransport transport);
    }
}
