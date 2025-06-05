//-----------------------------------------------------------------------
// <copyright file = "SimulateOperatorMenu.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Assets.SimulateOperatorMenu
{
    using System.Collections.Generic;
    using Communication.Platform.Interfaces;
    using UnityEngine;

    /// <summary>
    /// This class simulates the operator menu for the Standalone Game Lib.
    /// </summary>
    public class SimulateOperatorMenu : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// The key which will cause the menu to be shown and closed.
        /// </summary>
        public KeyCode SwitchKey = KeyCode.Tab;

        /// <summary>
        /// The size of the menu button.
        /// </summary>
        public Vector2 ButtonSize = new Vector2(150, 40);

        /// <summary>
        /// The amount of space between buttons.
        /// </summary>
        public Vector2 ButtonSpace = new Vector2(10, 10);

        /// <summary>
        /// The size of the scroll bar.  This has to be a constant since it is
        /// used in a static function for updating the window style.
        /// </summary>
        public const float ScrollBarSize = 8;

        #endregion

        #region Private Fields

        /// <summary>
        /// The implementation of the dependency needed by the operator menu.
        /// </summary>
        private ISimulateOperatorMenuDependency dependencyImplementation;

        /// <summary>
        /// The top level menu items.
        /// </summary>
        private List<OperatorMenuItem> menuItems;

        /// <summary>
        /// The linked list of picked menu items, from the top level to the most expanded level.
        /// </summary>
        private readonly List<OperatorMenuItem> visitPath = new List<OperatorMenuItem>();

        /// <summary>
        /// The flag indicating whether the operator menu is being shown.
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// The the viewing frame of the scrollable menu window.
        /// </summary>
        private Rect scrollFrame;

        /// <summary>
        /// Current scroll position in the menu.
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// The size of the menu window being shown.
        /// </summary>
        private Vector2 shownSize;

        #endregion

        #region MonoBehavior Overrides

        /// <summary>
        /// Initialize the operator menu.
        /// </summary>
        protected virtual void Start()
        {
            #if UNITY_EDITOR
            dependencyImplementation = GetComponent<ISimulateOperatorMenuDependency>();

            if(dependencyImplementation == null)
            {
                Debug.LogWarning("Failed to find an implementation of ISimulateOperatorMenuDependency. " +
                                 $"Please make sure such an implementation is attached to the game object {gameObject.name}, " +
                                 "same as SimulateOperatorMenu.");

                enabled = false;
                return;
            }

            isOpen = false;
            shownSize = new Vector2(Screen.width, Screen.height);

            // Initialize menu items.
            menuItems = new List<OperatorMenuItem>();

            if(dependencyImplementation.GameModeControl != null)
            {
                menuItems.Add(new HistoryMenuItem("History"));
            }

            if(dependencyImplementation.GameModeControl?.IsUtilityModeEnabled == true)
            {
                menuItems.Add(new UtilityMenuItem("Utility"));
            }

            menuItems.Add(new OperatorMenuItem
                          {
                              Label = "Shut Down",
                              OnClick = (dependency, onExit) => dependency?.GameModeControl?.ShutDown(),
                          });
            #else
            enabled = false;
            #endif
        }

        /// <summary>
        /// OnGUI function for the menu. Handles showing/hiding of the menu and drawing of the controls.
        /// </summary>
        protected virtual void OnGUI()
        {
            var gameModeControl = dependencyImplementation.GameModeControl;

            var lastVisit = visitPath.Count > 0 ? visitPath[visitPath.Count - 1] : null;
            var menuItemInControl = lastVisit?.InControl == true;

            if(gameModeControl != null && Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == SwitchKey)
            {
                // If the menu is open, exit the menu.
                if(isOpen)
                {
                    isOpen = false;

                    gameModeControl.EnterMode(GameMode.Play);
                }
                // if the menu is closed, and not menu item is in control, we are in game.
                // In this case, only open the menu if we are in the permitted states.
                else if(!isOpen && !menuItemInControl && dependencyImplementation.CanEnterOperatorMenu())
                {
                    gameModeControl.ExitMode();

                    isOpen = true;
                }
            }

            // Draw the window for the menu item that is in control.
            if(menuItemInControl)
            {
                UpdateStyle();
                lastVisit.DrawWindow();
            }

            // Draw the window for the operator menu.
            if(isOpen)
            {
                UpdateStyle();
                DrawWindow();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the style of the menu.
        /// </summary>
        private static void UpdateStyle()
        {
            GUI.skin.button.fontStyle = FontStyle.Bold;
            GUI.skin.button.fontSize = 12;
            GUI.skin.button.normal.textColor = Color.white;
            GUI.skin.horizontalScrollbar.fixedHeight = ScrollBarSize;
            GUI.skin.horizontalScrollbarThumb.fixedHeight = ScrollBarSize;
            GUI.skin.horizontalScrollbarThumb.normal.background = GUI.skin.window.normal.background;
            GUI.skin.verticalScrollbar.fixedWidth = ScrollBarSize;
            GUI.skin.verticalScrollbarThumb.fixedWidth = ScrollBarSize;
            GUI.skin.verticalScrollbarThumb.normal.background = GUI.skin.window.normal.background;
            GUI.skin.window.active.background = GUI.skin.window.normal.background;
            GUI.skin.window.focused.background = GUI.skin.window.normal.background;
            GUI.skin.window.hover.background = GUI.skin.window.normal.background;
        }

        /// <summary>
        /// Draw the menu window.
        /// </summary>
        private void DrawWindow()
        {
            GUI.Window((int)OperatorMenuWindowId.OperatorMenu, new Rect(0, 0, shownSize.x, shownSize.y), DrawControls, "");
        }

        /// <summary>
        /// The delegate called by GUI.Window to create the GUI inside the window.
        /// </summary>
        /// <param name="windowId">The id of the window it's currently making GUI for.</param>
        private void DrawControls(int windowId)
        {
            scrollFrame = new Rect(ButtonSpace.x,
                                   ButtonSpace.y,
                                   shownSize.x - 2 * ButtonSpace.x,
                                   shownSize.y - 2 * ButtonSpace.y);

            var countX = visitPath.Count + 1;

            var countY = menuItems.Count;
            for(var i = 1; i < visitPath.Count; i++)
            {
                if(visitPath[i].SubMenuItems != null)
                {
                    countY += visitPath[i].SubMenuItems.Count;
                }
            }

            var fullMenu = new Rect(0, 0, countX * (ButtonSize.x + ButtonSpace.x), countY * (ButtonSize.y + ButtonSpace.y));

            scrollPosition = GUI.BeginScrollView(scrollFrame, scrollPosition, fullMenu);

            // Draw the top level menu.
            DrawMenuLevel(0, 0, menuItems);

            // Draw the submenu for those in the visit path.
            for(var i = 1; i <= visitPath.Count; i++)
            {
                DrawMenuLevel(i, GetOffsetY(i), visitPath[i - 1].SubMenuItems);
            }

            GUI.EndScrollView();
        }

        /// <summary>
        /// Get the vertical offset for a menu level.
        /// Each submenu is opened at the same Y position
        /// as its parent menu item's location.
        /// </summary>
        /// <param name="targetLevel">The 0-based level id.</param>
        /// <returns>The vertical offset of the menu level.</returns>
        private int GetOffsetY(int targetLevel)
        {
            var result = 0;

            for(var i = 0; i < targetLevel; i++)
            {
                var menuItem = visitPath[i];
                var menuList = i == 0 ? menuItems : visitPath[i - 1].SubMenuItems;

                result += menuList.IndexOf(menuItem);
            }

            return result;
        }

        /// <summary>
        /// Draw all the menu items in a menu level.
        /// </summary>
        /// <param name="level">The 0-based level id.</param>
        /// <param name="offsetY">The vertical offset of the menu level.</param>
        /// <param name="menuList">The list of menu items to be drawn in the menu level.</param>
        private void DrawMenuLevel(int level, int offsetY, IEnumerable<OperatorMenuItem> menuList)
        {
            if(menuList != null)
            {
                var rectButton = new Rect(level * (ButtonSize.x + ButtonSpace.x),
                                          offsetY * (ButtonSize.y + ButtonSpace.y),
                                          ButtonSize.x,
                                          ButtonSize.y);

                // Draw the menu level.
                foreach(var menuItem in menuList)
                {
                    // Do not draw buttons which are not visible.
                    if(rectButton.xMax >= scrollPosition.x &&
                       rectButton.xMin <= scrollPosition.x + scrollFrame.width &&
                       rectButton.yMax >= scrollPosition.y &&
                       rectButton.yMin <= scrollPosition.y + scrollFrame.height)
                    {
                        if(GUI.Button(rectButton, menuItem.Label))
                        {
                            UpdateVisitPath(menuItem);

                            if(menuItem.SubMenuItems == null &&
                               menuItem.OnClick != null)
                            {
                                menuItem.OnClick(dependencyImplementation, OnExitMenuItem);

                                isOpen = false;
                            }
                        }
                    }

                    rectButton.y += ButtonSize.y + ButtonSpace.y;
                }
            }
        }

        /// <summary>
        /// Update the linked list of the picked menu items with the latest pick.
        /// It always starts with one item in the top level that is the most
        /// ancestor of the latest pick, and ends with the latest pick.
        /// </summary>
        /// <param name="newPick">The menu item that was just picked.</param>
        private void UpdateVisitPath(OperatorMenuItem newPick)
        {
            if(newPick != null)
            {
                // If it is in the top level, restart the visit path
                // from the beginning.
                if(menuItems.Contains(newPick))
                {
                    visitPath.Clear();
                }
                // If it is in one of the sub menus, attach the new
                // pick to its parent and make it the end of the path.
                else
                {
                    for(var i = 0; i < visitPath.Count - 1; i++)
                    {
                        // Search the parent of the new pick.
                        if(visitPath[i].SubMenuItems.Contains(newPick))
                        {
                            visitPath.RemoveRange(i + 1, visitPath.Count - i - 1);
                            break;
                        }
                    }
                }

                visitPath.Add(newPick);
            }
        }

        /// <summary>
        /// The callback function to be passed on to each menu item.
        /// It will be executed when the menu item is done with its job.
        /// </summary>
        private void OnExitMenuItem()
        {
            // Open the menu when a menu item is done.
            isOpen = true;
        }

        #endregion
    }
}
