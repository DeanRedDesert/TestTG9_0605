//-----------------------------------------------------------------------
// <copyright file = "OperatorMenuItem.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Assets.SimulateOperatorMenu
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Encapsulate a method doing the job for a menu item upon being clicked.
    /// </summary>
    /// <param name="dependency">
    /// The dependency object needed by the operator menu to function.
    /// </param>
    /// <param name="onExit">
    /// The callback of the operator menu to be called
    /// when the menu item is done with its job.
    /// </param>
    public delegate void MenuItemAction(ISimulateOperatorMenuDependency dependency, Action onExit);

    /// <summary>
    /// This class represents a menu item in the simulate operator menu.
    /// </summary>
    public class OperatorMenuItem
    {
        /// <summary>
        /// Get or set the text to be displayed on the menu item.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Get or set the items of the sub menu.
        /// </summary>
        public List<OperatorMenuItem> SubMenuItems { get; set; }

        /// <summary>
        /// Get or set the delegate to be called when the menu item is clicked,
        /// and the menu item doesn't have a submenu configured.
        /// </summary>
        public MenuItemAction OnClick { get; set; }

        /// <summary>
        /// Get the status whether the menu item is in control of the
        /// operator much, e.g. displaying its own window.
        /// </summary>
        public bool InControl { get; protected set; }

        /// <summary>
        /// Display the menu item's GUI window.
        /// </summary>
        public virtual void DrawWindow()
        {
        }
    }
}
