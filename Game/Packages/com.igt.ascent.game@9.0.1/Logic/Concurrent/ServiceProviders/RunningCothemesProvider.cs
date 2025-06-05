// -----------------------------------------------------------------------
// <copyright file = "RunningCothemesProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Logic.Services;
    using Interfaces;

    /// <summary>
    /// This provider provides game services for querying information
    /// on the running Cothemes in a Shell.
    /// </summary>
    public class RunningCothemesProvider : NonObserverProviderBase
    {
        #region Constants

        /// <summary>
        /// The default name of the provider.
        /// </summary>
        private const string DefaultName = nameof(RunningCothemesProvider);

        #endregion

        #region Private Fields

        /// <summary>
        /// Back end field for the property RunningCothemes to help with argument validation in the setter.
        /// </summary>
        private IReadOnlyList<CothemePresentationKey> runningCothemes = new List<CothemePresentationKey>();

        /// <summary>
        /// Back end field for the property MaxNumCoplayers.
        /// </summary>
        private int maxNumCoplayers = 1;

        #endregion

        #region Game Services

        /// <summary>
        /// Gets or sets the presentation keys of current running cothemes.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the value to set is null.
        /// </exception>
        [GameService]
        public IReadOnlyList<CothemePresentationKey> RunningCothemes
        {
            get => runningCothemes;
            set => runningCothemes = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the total number of allowed coplayers for presentation.
        /// </summary>
        [GameService]
        public int MaxNumCoplayers
        {
            get => maxNumCoplayers;
            set
            {
                if(value > 0)
                {
                    maxNumCoplayers = value;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RunningCothemesProvider" />.
        /// </summary>
        /// <param name="name">
        /// The name of the service provider.
        /// This parameter is optional.  If not specified, the provider name
        /// will be set to <see cref="DefaultName" />.
        /// </param>
        public RunningCothemesProvider(string name = DefaultName) : base(name)
        {
        }

        #endregion
    }
}