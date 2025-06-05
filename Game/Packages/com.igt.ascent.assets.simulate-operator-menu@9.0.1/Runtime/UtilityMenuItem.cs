//-----------------------------------------------------------------------
// <copyright file = "UtilityMenuItem.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Assets.SimulateOperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform.Interfaces;
    using UnityEngine;

    /// <summary>
    /// This class represents an item on the simulated operator menu used to enter the utility mode.
    /// </summary>
    public class UtilityMenuItem : OperatorMenuItem
    {
        #region Private Fields

        /// <summary>
        /// Interface used to simulate the utility functionalities.
        /// </summary>
        private ISimulateGameModeControl gameModeControl;

        /// <summary>
        /// The callback used to exit the utility display window.
        /// </summary>
        private Action exitAction;

        /// <summary>
        /// The themes defined in the registry that support Utility mode.
        /// </summary>
        private IReadOnlyList<string> registrySupportedThemes;

        /// <summary>
        /// The supported denominations defined in the registry.
        /// </summary>
        private IReadOnlyDictionary<KeyValuePair<string, string>, IEnumerable<long>> registrySupportedDenominations;

        /// <summary>
        /// The window id of the sub menus.
        /// </summary>
        private OperatorMenuWindowId windowInDisplay;

        /// <summary>
        /// The selected theme name.
        /// </summary>
        private string selectedTheme;

        /// <summary>
        /// The selected paytable variant, pair of paytable and paytable file name.
        /// </summary>
        private KeyValuePair<string, string> selectedPaytable;

        /// <summary>
        /// The index of the selected denomination in the denomination list.
        /// </summary>
        private int selectedDenominationIndex;

        #endregion

        #region Menu Texts

        /// <summary>
        /// The title height of the utility windows.
        /// </summary>
        private const int TitleHeight = 30;

        /// <summary>
        /// The button space in the utility windows.
        /// </summary>
        private readonly Vector2 buttonSpace = new Vector2(10, 10);

        /// <summary>
        /// Theme selection window text.
        /// </summary>
        private const string ThemeSelectionWindowText = "SELECT A THEME";

        /// <summary>
        /// Paytable selection window text.
        /// </summary>
        private const string PaytableSelectionWindowText = "SELECT A PAYTABLE";

        /// <summary>
        /// Denomination selection window text.
        /// </summary>
        private const string DenominationSelectionWindowText = "SELECT A DENOMINATION";

        /// <summary>
        /// Utility display windows text.
        /// </summary>
        private const string UtilityDisplayWindowText = "UTILITY";

        /// <summary>
        /// Utility exit button text.
        /// </summary>
        private const string UtilityExitButtonText = "EXIT";

        #endregion

        #region Menu Display Configurations

        /// <summary>
        /// Theme button size.
        /// </summary>
        private readonly Vector2 themeButtonSize = new Vector2(200, 40);

        /// <summary>
        /// Theme window size.
        /// </summary>
        private Vector2 themeWindowSize;

        /// <summary>
        /// Theme scroll position.
        /// </summary>
        private Vector2 themeScrollPosition;

        /// <summary>
        /// Paytable button size.
        /// </summary>
        private readonly Vector2 paytableButtonSize = new Vector2(200, 40);

        /// <summary>
        /// Paytable window size.
        /// </summary>
        private Vector2 paytableWindowSize;

        /// <summary>
        /// Paytable scroll position.
        /// </summary>
        private Vector2 paytableScrollPosition;

        /// <summary>
        /// Maximum columns shown in the denomination selection menu.
        /// </summary>
        private const int MaxColumns = 4;

        /// <summary>
        /// Denomination button size.
        /// </summary>
        private readonly Vector2 denominationButtonSize = new Vector2(100, 40);

        /// <summary>
        /// Denomination window size.
        /// </summary>
        private Vector2 denominationWindowSize;

        /// <summary>
        /// Denomination layout.
        /// </summary>
        private Vector2 denominationLayout;

        /// <summary>
        /// The count of buttons to display in the utility display window.
        /// </summary>
        private const int DisplayButtonCount = 1;

        /// <summary>
        /// The display button size.
        /// </summary>
        private readonly Vector2 displayButtonSize = new Vector2(100, 40);

        #endregion

        #region Cosntructor

        /// <summary>
        /// Initialize a new instance of UtilityMenuItem with a label text.
        /// </summary>
        /// <param name="label">The text to be displayed on the menu item.</param>
        public UtilityMenuItem(string label)
        {
            Label = label;
            InControl = false;

            OnClick = DoUtility;
    }

        /// <summary>
        /// Take over the control of the operator menu,
        /// and start the utility mode of the game.
        /// </summary>
        /// <param name="dependency">
        /// The dependency object needed by the operator menu to function.
        /// </param>
        /// <param name="onExit">
        /// The callback of the operator menu to be called
        /// when the menu item is done with its job.
        /// </param>
        private void DoUtility(ISimulateOperatorMenuDependency dependency, Action onExit)
        {
            gameModeControl = dependency.GameModeControl;

            if(gameModeControl == null || !gameModeControl.IsUtilityModeEnabled)
            {
                return;
            }

            exitAction = onExit;

            // Get the information on the themes.
            if(registrySupportedThemes == null)
            {
                registrySupportedThemes = gameModeControl.GetRegistrySupportedThemes();
            }

            // Reset the selected values.
            selectedTheme = null;
            selectedPaytable = new KeyValuePair<string, string>();
            selectedDenominationIndex = -1;

            // Reset the scroll position.
            themeScrollPosition = new Vector2();

            // Got to the theme selection window.
            windowInDisplay = OperatorMenuWindowId.UtilitySelectTheme;

            InControl = true;
        }

        #endregion

        #region OpratorMenuItem Overrides

        /// <inheritdoc />
        public override void DrawWindow()
        {
            switch(windowInDisplay)
            {
                case OperatorMenuWindowId.UtilitySelectTheme:
                    DrawThemeSelectionWindow();
                    break;

                case OperatorMenuWindowId.UtilitySelectPaytable:
                    DrawPaytableSelectionWindow();
                    break;

                case OperatorMenuWindowId.UtilitySelectDenomination:
                    DrawDenominationSelectionWindow();
                    break;

                case OperatorMenuWindowId.UtilityDisplay:
                    DrawUtilityDisplayWindow();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(windowInDisplay), $"Window Id {windowInDisplay} not supported by Utility menu.");
            }
        }

        #endregion

        #region Theme Selection Window

        /// <summary>
        /// Draw the window for selecting a theme.
        /// </summary>
        private void DrawThemeSelectionWindow()
        {
            var buttonSize = themeButtonSize;
            var buttonCount = registrySupportedThemes.Count;

            // If there are too many buttons, make it scroll.
            themeWindowSize.x = buttonSize.x + buttonSpace.x * 2;
            themeWindowSize.y = Math.Min(Screen.height * 0.75f,
                                         TitleHeight + buttonCount * (buttonSize.y + buttonSpace.y));

            // Place the window in the center of the screen.
            GUI.Window((int)OperatorMenuWindowId.UtilitySelectTheme,
                        new Rect((Screen.width - themeWindowSize.x) / 2,
                                 (Screen.height - themeWindowSize.y) / 2,
                                 themeWindowSize.x,
                                 themeWindowSize.y),
                        DrawThemeSelectionControls,
                        ThemeSelectionWindowText);
        }

        /// <summary>
        /// Draw buttons for theme selection window.
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for.</param>
        private void DrawThemeSelectionControls(int windowId)
        {
            var buttonSize = themeButtonSize;
            var buttonCount = registrySupportedThemes.Count;

            var scrollFrame = new Rect(buttonSpace.x,
                                       TitleHeight,
                                       themeWindowSize.x - buttonSpace.x,
                                       themeWindowSize.y - TitleHeight);


            var fullView = new Rect(0,
                                    0,
                                    buttonSize.x,
                                    buttonCount * (buttonSize.y + buttonSpace.y));

            themeScrollPosition = GUI.BeginScrollView(scrollFrame, themeScrollPosition, fullView);

            var rectButton = new Rect(0,
                                      0,
                                      buttonSize.x,
                                      buttonSize.y);

            // Draw the buttons.
            foreach(var themeName in registrySupportedThemes)
            {
                // Do not draw buttons which are not visible.
                if(rectButton.xMax >= themeScrollPosition.x &&
                   rectButton.xMin <= themeScrollPosition.x + scrollFrame.width &&
                   rectButton.yMax >= themeScrollPosition.y &&
                   rectButton.yMin <= themeScrollPosition.y + scrollFrame.height)
                {
                    // Use the paytable name as the button label.
                    if(GUI.Button(rectButton, themeName))
                    {
                        // Record the theme selection.
                        selectedTheme = themeName;

                        // Get the list of paytables and their associated denominations
                        // for the selected theme.
                        registrySupportedDenominations = gameModeControl.GetRegistrySupportedDenominations(selectedTheme);

                        // Reset the paytable selection.
                        selectedPaytable = new KeyValuePair<string, string>();

                        // Go to the denomination selection window.
                        windowInDisplay = OperatorMenuWindowId.UtilitySelectPaytable;
                    }
                }

                rectButton.y += buttonSize.y + buttonSpace.y;
            }

            GUI.EndScrollView();
        }

        #endregion

        #region Paytable Selection Window

        /// <summary>
        /// Draw the window for selecting a paytable.
        /// </summary>
        private void DrawPaytableSelectionWindow()
        {
            var buttonSize = paytableButtonSize;
            var buttonCount = registrySupportedDenominations.Count;

            // If there are too many buttons, make it scroll.
            paytableWindowSize.x = buttonSize.x + buttonSpace.x * 2;
            paytableWindowSize.y = Math.Min(Screen.height * 0.75f,
                                            TitleHeight + buttonCount * (buttonSize.y + buttonSpace.y));

            // Place the window in the center of the screen.
            GUI.Window((int)OperatorMenuWindowId.UtilitySelectPaytable,
                        new Rect((Screen.width - paytableWindowSize.x) / 2,
                                 (Screen.height - paytableWindowSize.y) / 2,
                                 paytableWindowSize.x,
                                 paytableWindowSize.y),
                        DrawPaytableSelectionControls,
                        PaytableSelectionWindowText);
        }

        /// <summary>
        /// Draw buttons for paytable selection window.
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for.</param>
        private void DrawPaytableSelectionControls(int windowId)
        {
            var buttonSize = paytableButtonSize;
            var buttonCount = registrySupportedDenominations.Count;

            var scrollFrame = new Rect(buttonSpace.x,
                                       TitleHeight,
                                       paytableWindowSize.x - buttonSpace.x,
                                       paytableWindowSize.y - TitleHeight);


            var fullView = new Rect(0,
                                    0,
                                    buttonSize.x,
                                    buttonCount * (buttonSize.y + buttonSpace.y));

            paytableScrollPosition = GUI.BeginScrollView(scrollFrame, paytableScrollPosition, fullView);

            var rectButton = new Rect(0,
                                      0,
                                      buttonSize.x,
                                      buttonSize.y);

            // Draw the buttons.
            foreach(var paytable in registrySupportedDenominations.Keys)
            {
                // Do not draw buttons which are not visible.
                if(rectButton.xMax >= paytableScrollPosition.x &&
                   rectButton.xMin <= paytableScrollPosition.x + scrollFrame.width &&
                   rectButton.yMax >= paytableScrollPosition.y &&
                   rectButton.yMin <= paytableScrollPosition.y + scrollFrame.height)
                {
                    // Use the paytable name as the button label.
                    if(GUI.Button(rectButton, paytable.Key))
                    {
                        // Record the paytable selection.
                        selectedPaytable = paytable;

                        // Reset the denomination selection.
                        selectedDenominationIndex = -1;

                        // Go to the denomination selection window.
                        windowInDisplay = OperatorMenuWindowId.UtilitySelectDenomination;
                    }
                }

                rectButton.y += buttonSize.y + buttonSpace.y;
            }

            GUI.EndScrollView();
        }

        #endregion

        #region Denomination Selection Window

        /// <summary>
        /// Draw the window for selecting a denomination.
        /// </summary>
        private void DrawDenominationSelectionWindow()
        {
            var buttonSize = denominationButtonSize;
            var denominationList = registrySupportedDenominations[selectedPaytable].ToList();
            var buttonCount = denominationList.Count;

            denominationLayout.x = buttonCount < MaxColumns ? buttonCount : MaxColumns;
            denominationLayout.y = (float)Math.Ceiling(buttonCount / denominationLayout.x);

            // Ensure menu scales appropriately for differing screen resolutions.
            // Make sure it fits to screen. If its too large for the screen width, shrink button size to make it fit.
            if((buttonSize.x + buttonSpace.x) * denominationLayout.x + buttonSpace.x >= Screen.width)
            {
                buttonSize.x = (Screen.width - buttonSpace.x * (denominationLayout.x + 1)) / denominationLayout.x;
            }

            // Shrink height if necessary.
            if((buttonSize.y + buttonSpace.y) * denominationLayout.y + buttonSpace.y + TitleHeight >= Screen.height)
            {
                buttonSize.y = (Screen.height - TitleHeight - buttonSpace.y * (denominationLayout.y + 1)) / denominationLayout.y;
            }

            // The number of denominations is no more than 26.
            // So we don't have to worry about scrolling.
            // Use buttonSpace.x instead of buttonSize.x to calculate appropriate window size.
            denominationWindowSize.x = buttonSpace.x + denominationLayout.x * (buttonSize.x + buttonSpace.x);
            denominationWindowSize.y = TitleHeight + denominationLayout.y * (buttonSize.y + buttonSpace.y);

            // Place the window in the center of the screen.
            GUI.Window((int)OperatorMenuWindowId.UtilitySelectDenomination,
                        new Rect((Screen.width - denominationWindowSize.x) / 2,
                                 (Screen.height - denominationWindowSize.y) / 2,
                                 denominationWindowSize.x,
                                 denominationWindowSize.y),
                        DrawDenominationSelectionControls,
                        DenominationSelectionWindowText);
        }

        /// <summary>
        /// Draw buttons for denomination selection window.
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for.</param>
        private void DrawDenominationSelectionControls(int windowId)
        {
            var denominationList = registrySupportedDenominations[selectedPaytable].ToList();
            var buttonLabels =  denominationList.ConvertAll(denomination => denomination.ToString());

            var fullView = new Rect(buttonSpace.x,
                                    TitleHeight,
                                    denominationWindowSize.x - buttonSpace.x * 2,
                                    denominationWindowSize.y - TitleHeight - buttonSpace.y);

            selectedDenominationIndex = GUI.SelectionGrid(fullView,
                                                          selectedDenominationIndex,
                                                          buttonLabels.ToArray(),
                                                          (int)denominationLayout.x);

            if(selectedDenominationIndex != -1)
            {
                // Go to the utility display window.
                windowInDisplay = OperatorMenuWindowId.UtilityDisplay;

                gameModeControl.UtilityTheme = selectedTheme;
                gameModeControl.UtilityPaytable = selectedPaytable;
                gameModeControl.UtilityDenomination = denominationList[selectedDenominationIndex];
                gameModeControl.UtilitySelectionComplete = true;

                gameModeControl.EnterMode(GameMode.Utility);
            }
        }

        #endregion

        #region Utility Display Window

        /// <summary>
        /// Draw the window for performing utility functions.
        /// </summary>
        private void DrawUtilityDisplayWindow()
        {
            var buttonSize = displayButtonSize;

            GUI.Window((int)OperatorMenuWindowId.UtilityDisplay,
                       new Rect(Screen.width - buttonSize.x - buttonSpace.x * 3,
                                buttonSpace.y,
                                buttonSize.x + 2 * buttonSpace.x,
                                TitleHeight + DisplayButtonCount * (buttonSize.y + buttonSpace.y)),
                       DrawUtilityDisplayControls,
                       UtilityDisplayWindowText);
        }

        /// <summary>
        /// Draw buttons for utility display window.
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for.</param>
        private void DrawUtilityDisplayControls(int windowId)
        {
            var buttonSize = displayButtonSize;

            var rectButton = new Rect(buttonSpace.x,
                                      TitleHeight,
                                      buttonSize.x,
                                      buttonSize.y);

            // Always display the Exit button.
            if(GUI.Button(rectButton, UtilityExitButtonText))
            {
                gameModeControl?.ExitMode();

                InControl = false;

                if(exitAction != null)
                {
                    exitAction();
                }
            }
        }

        #endregion
    }
}
