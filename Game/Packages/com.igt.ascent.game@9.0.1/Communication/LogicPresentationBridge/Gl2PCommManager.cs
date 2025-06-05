// -----------------------------------------------------------------------
// <copyright file = "Gl2PCommManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Logic.Concurrent.Interfaces;
    using Logic.CommServices;
    using Presentation.CommServices;

    /// <summary>
    /// This class manages the GL2P comm service channels for
    /// multiple pair of logic/presentation clients.
    /// </summary>
    /// <remarks>
    /// This implementation assumes that all its public methods are available for
    /// a logic client, while only methods in <see cref="IGl2PCommManager"/>
    /// are for the presentation client.
    /// </remarks>
    public sealed class Gl2PCommManager : IGl2PCommManager, IDisposable
    {
        #region Private Fields

        private readonly Dictionary<CothemePresentationKey, CombinedCommServices> commServicesMap;

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Gl2PCommManager"/>.
        /// </summary>
        public Gl2PCommManager()
        {
            commServicesMap = new Dictionary<CothemePresentationKey, CombinedCommServices>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates and returns a new GL2P comm channel for a given presentation key,
        /// if it does not exist yet.  Otherwise, returns the existing one.
        /// </summary>
        /// <remarks>
        /// If there is an existing GL2P comm channel for the same coplayer (with a different cotheme),
        /// the existing GL2P comm channel will be disposed to prevent exploding number of channels.
        /// </remarks>
        /// <param name="key">
        /// The key to identify the new comm channel created.
        /// </param>
        /// <returns>
        /// The logic side interface of the newly created comm channel.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="key"/> is null.
        /// </exception>
        public ILogicCommServices CreateCommServices(CothemePresentationKey key)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock(commServicesMap)
            {
                // First, check if we already have one for the key.
                if(commServicesMap.TryGetValue(key, out var result))
                {
                    return result;
                }

                // Next, check if we have one for the coplayer, but with a different theme id.
                // If yes, remove and dispose the old one before creating a new one.
                // This is to avoid exploding number of channels if the user keeps loading
                // different themes on the same coplayer.
                var existingCoplayer = commServicesMap.Keys.FirstOrDefault(existingKey => existingKey.CoplayerId == key.CoplayerId);

                if(existingCoplayer != null)
                {
                    // Dispose the existing channel.
                    commServicesMap[existingCoplayer].Dispose();

                    // Remove it from the collection.
                    commServicesMap.Remove(existingCoplayer);
                }

                // Finally, create a new one and add to the collection.
                result = new CombinedCommServices();

                commServicesMap.Add(key, result);

                return result;
            }
        }

        #endregion

        #region IGl2PCommManager Implementation

        /// <inheritdoc/>
        public IPresentationCommServices GetPresentationCommServices(CothemePresentationKey key)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CombinedCommServices result;

            lock(commServicesMap)
            {
                _ = commServicesMap.TryGetValue(key, out result);
            }

            return result;
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if(disposing)
                {
                    lock(commServicesMap)
                    {
                        foreach(var combinedCommServices in commServicesMap.Values)
                        {
                            combinedCommServices.Dispose();
                        }
                    }
                }
                isDisposed = true;
            }
        }

        #endregion
    }
}