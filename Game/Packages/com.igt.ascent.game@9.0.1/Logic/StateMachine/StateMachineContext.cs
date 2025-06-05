//-----------------------------------------------------------------------
// <copyright file = "StateMachineContext.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class represents the context information that is used to create a state machine.
    /// </summary>
    /// <remarks>
    /// This class is not thread safe, so the thread safety should be guaranteed by the invokers from the 
    /// game logic thread and the state machine thread.
    /// </remarks>
    public class StateMachineContext : IStateMachineContext
    {
        #region Private Fields

        /// <summary>
        /// The game lib that is used to communicate with the Foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Flag indicating if the theme is newly selected for play.
        /// </summary>
        private bool? newlySelectedForPlay;

        /// <summary>
        /// The storage path of the newly selected for play flag.
        /// </summary>
        private const string NewlySelectedForPlayPath = "StateMachineContext/NewlySelectedForPlay";

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the instance.
        /// </summary>
        /// <param name="gameLib">The game lib used to communicate with the Foundation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameLib"/> is null.</exception>
        public StateMachineContext(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib", "Parameter may not be null.");
            }

            this.gameLib = gameLib;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the theme resource.
        /// </summary>
        /// <param name="themeResource">The new theme resource.</param>
        public void ApplyNewThemeResource(ThemeResource themeResource)
        {
            ThemeResource = themeResource;
        }

        /// <summary>
        /// Update the theme context information.
        /// </summary>
        /// <param name="themeContext">The new theme context.</param>
        /// <returns>True if the state machine restart is required; false, otherwise.</returns>
        public bool ApplyNewContextInformation(ThemeContext themeContext)
        {
            SetNewlySelectedForPlay(themeContext);

            var contextChanged = false;
            if(IsNewGameContext(themeContext))
            {
                contextChanged = true;
                GameContextMode = themeContext.GameContextMode;
                GameSubMode = themeContext.GameSubMode;
                PaytableName = themeContext.PaytableName;
                PaytableFileName = themeContext.PaytableFileName;
            }

            return contextChanged ||
                   (GameContextMode == GameMode.Play && themeContext.NewlySelectedForPlay);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Mark the current context(play mode) as is newly selected for play.
        /// </summary>
        /// <param name="themeContext">The theme context used to update the newly selected for play flag.</param>
        private void SetNewlySelectedForPlay(ThemeContext themeContext)
        {
            if(themeContext.GameContextMode == GameMode.Play)
            {
                // Restore the newly selected for play flag.
                if(!newlySelectedForPlay.HasValue)
                {
                    newlySelectedForPlay = gameLib.ReadCriticalData<bool>(CriticalDataScope.Theme,
                                                                          NewlySelectedForPlayPath);
                }

                // We are only supposed to update the flag when the new theme context is newly selected for play.
                // It should be the game state machine's responsibility to reset the NewlySelectedForPlay flag
                // to false. Because if power failure occurs after the ActivateThemeContextEvent message has been
                // replied and before this flag is handled by the state machine, when the game restarts up, although
                // the Foundation does not think the new theme is newly selected for play anymore, but it is still true
                // to the game state machine.
                if(themeContext.NewlySelectedForPlay &&
                   !newlySelectedForPlay.Value)
                {
                    newlySelectedForPlay = true;
                    gameLib.WriteCriticalData(CriticalDataScope.Theme,
                                              NewlySelectedForPlayPath,
                                              newlySelectedForPlay);
                }
            }
        }

        /// <summary>
        /// Verify if the theme context is new.
        /// </summary>
        /// <param name="themeContext">The theme context.</param>
        /// <returns>True if the theme context is new; false, otherwise.</returns>
        private bool IsNewGameContext(ThemeContext themeContext)
        {
            return !(GameContextMode == themeContext.GameContextMode &&
                     GameSubMode == themeContext.GameSubMode &&
                     PaytableName == themeContext.PaytableName &&
                     PaytableFileName == themeContext.PaytableFileName);
        }

        #endregion

        #region Override Members

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}/{3}/{4}/{5}", GameContextMode, GameSubMode, PaytableName, PaytableFileName,
                                 IsNewlySelectedForPlay, ThemeResource);
        }

        #endregion

        #region IStateMachineContext Members

        /// <inheritDoc />
        public GameMode GameContextMode { get; private set; }

        /// <inheritDoc />
        public GameSubMode GameSubMode { get; private set; }

        /// <inheritDoc />
        public string PaytableName { get; private set; }

        /// <inheritDoc />
        public string PaytableFileName { get; private set; }

        /// <inheritDoc />
        public ThemeResource ThemeResource { get; private set; }

        /// <inheritDoc />
        public bool IsNewlySelectedForPlay
        {
            get
            {
                if(gameLib.GameContextMode == GameMode.Play)
                {
                    return newlySelectedForPlay.GetValueOrDefault();
                }

                return false;
            }
        }

        /// <inheritDoc />
        /// <exception cref="FunctionCallNotAllowedInModeOrStateException">
        /// Thrown if this method is called in other game context mode rather than 
        /// <seealso cref="GameMode.Play"/> mode.
        /// </exception>
        public void ResetNewlySelectedForPlay()
        {
            if(gameLib.GameContextMode != GameMode.Play)
            {
                throw new FunctionCallNotAllowedInModeOrStateException(
                    "Function ResetNewlySelectedForPlay can only be called in play mode.",
                    gameLib.GameContextMode,
                    gameLib.QueryGameCycleState());
            }

            // Reset the newly selected for play flag to false.
            // We don't have to process the resetting if it is already false.
            if(!newlySelectedForPlay.HasValue || newlySelectedForPlay.Value)
            {
                newlySelectedForPlay = false;
                gameLib.WriteCriticalData(CriticalDataScope.Theme,
                                          NewlySelectedForPlayPath,
                                          newlySelectedForPlay);
            }
        }

        #endregion

    }
}
