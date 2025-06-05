//-----------------------------------------------------------------------
// <copyright file = "HistoryMenuItem.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Assets.SimulateOperatorMenu
{
    using System;
    using Communication.Platform.Interfaces;
    using UnityEngine;

    /// <summary>
    /// This class represents a menu item for history browsing.
    /// </summary>
    public class HistoryMenuItem : OperatorMenuItem
    {
        private const int TitleHeight = 30;
        private const int ButtonCount = 3;
        private readonly Vector2 buttonSize = new Vector2(100, 40);
        private readonly Vector2 buttonSpace = new Vector2(10, 10);

        private ISimulateGameModeControl gameModeControl;
        private Action exitAction;

        private readonly object historyLocker = new object();

        /// <summary>
        /// Initialize a new instance of HistoryMenuItem with a label text.
        /// </summary>
        /// <param name="label">The text to be displayed on the menu item.</param>
        public HistoryMenuItem(string label)
        {
            Label = label;
            InControl = false;

            OnClick = DoHistory;
        }

        /// <inheritdoc />
        public override void DrawWindow()
        {
            GUI.Window((int)OperatorMenuWindowId.HistoryDisplay,
                       new Rect(Screen.width - buttonSize.x - buttonSpace.x * 3,
                                buttonSpace.y,
                                buttonSize.x + 2 * buttonSpace.x,
                                TitleHeight + ButtonCount * (buttonSize.y + buttonSpace.y)),
                       DrawControls,
                       "HISTORY");
        }

        /// <summary>
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for</param>
        private void DrawControls(int windowId)
        {
            var rectButton = new Rect(buttonSpace.x,
                                      TitleHeight,
                                      buttonSize.x,
                                      buttonSize.y);

            lock(historyLocker)
            {
                if(gameModeControl != null &&
                   gameModeControl.IsPreviousAvailable() &&
                   GUI.Button(rectButton, "Previous Game"))
                {
                    gameModeControl.PreviousHistoryRecord();
                }

                rectButton.y += buttonSize.y + buttonSpace.y;

                if(gameModeControl != null &&
                   gameModeControl.IsNextAvailable() &&
                   GUI.Button(rectButton, "Next Game"))
                {
                        gameModeControl.NextHistoryRecord();
                }

                rectButton.y += buttonSize.y + buttonSpace.y;

                // Always display the Exit button.
                if(GUI.Button(rectButton, "Exit"))
                {
                    if(gameModeControl != null)
                    {
                        gameModeControl.ExitMode();
                    }

                    InControl = false;

                    if(exitAction != null)
                    {
                        exitAction();
                    }
                }
            }
        }

        /// <summary>
        /// Take over the control of the operator menu,
        /// and start the history mode of the game.
        /// </summary>
        /// <param name="dependency">
        /// The dependency object needed by the operator menu to function.
        /// </param>
        /// <param name="onExit">
        /// The callback of the operator menu to be called
        /// when the menu item is done with its job.
        /// </param>
        private void DoHistory(ISimulateOperatorMenuDependency dependency, Action onExit)
        {
            gameModeControl = dependency?.GameModeControl;
            exitAction = onExit;

            if(gameModeControl != null)
            {
                InControl = true;

                gameModeControl.EnterMode(GameMode.History);
            }
        }
    }
}
