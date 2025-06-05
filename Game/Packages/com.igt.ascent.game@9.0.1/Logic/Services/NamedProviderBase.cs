// -----------------------------------------------------------------------
// <copyright file = "NamedProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    /// <summary>
    /// Base implementation of <see cref="INamedProvider"/> to handle the name assignment.
    /// </summary>
    public abstract class NamedProviderBase : INamedProvider
    {
        #region Protected and Private Fields

        /// <summary>
        /// Gets the default provider name which must be overridden in derived providers. If overridden to
        /// return a valid name, it will be returned by the <see cref="Name"/> getter when no name is
        /// provided through the constructor.
        /// </summary>
        protected abstract string DefaultName { get; }

        /// <summary>
        /// The provider name which is set through the constructor. If this is set to a valid name, it 
        /// will be returned by the <see cref="Name"/> getter and takes priority over <see cref="DefaultName"/>.
        /// </summary>
        private readonly string constructorName;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="NamedProviderBase"/>.
        /// </summary>
        protected NamedProviderBase(string name = null)
        {
            constructorName = name;
        }

        #endregion

        #region INamedProvider Implementation

        /// <inheritdoc />
        /// <exception cref="ApplicationException">Thrown if the provider name is undefined.</exception>
        public string Name
        {
            get
            {
                string result;
                if(constructorName != null)
                {
                    result = constructorName;
                }
                else if(DefaultName != null)
                {
                    result = DefaultName;
                }
                else
                {
                    throw new ApplicationException("The provider name is undefined. Make sure either " +
                                                   "this class's DefaultName getter is overriden or the " +
                                                   "class object is instantiated with a valid provider name.");
                }
                return result;
            }
        }

        #endregion
    }
}