// -----------------------------------------------------------------------
//  <copyright file = "InfoOverlayItem.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
namespace IGT.Game.Utp.Framework
{
    using System;
    using UnityEngine;
    using Object = System.Object;

    /// <summary>
    /// An item that can be displayed on top of all other game elements
    /// </summary>
    public class InfoOverlayItem
    {
        #region Public Properties

        /// <summary>
        /// Description of info format.
        /// </summary>
        public enum InfoType
        {
            Box,
            Label,
            Button,
            RepeatButton,
            TextField,
            TextArea,
            Toggle,
            Toolbar,
            SelectionGrid,
            HorizontalSlider,
            VerticalSlider,
            HorizontalScrollbar,
            VerticalScrollbar,
            ScrollView,
            Window
        }

        /// <summary>
        /// Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public InfoType Type { get; set; }

        /// <summary>
        /// Text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Color.
        /// </summary>
        public Color Color = Color.clear;

        /// <summary>
        /// Font size.
        /// </summary>
        public int FontSize = -1;

        /// <summary>
        /// Alignment anchor.
        /// </summary>
        public TextAnchor Alignment = TextAnchor.MiddleCenter;

        /// <summary>
        /// Position.
        /// </summary>
        public Rect Position { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Display time.
        /// </summary>
        public float DisplayTime { get; set; }

        /// <summary>
        /// RemoveAfterDisplay.
        /// </summary>
        public bool RemoveAfterDisplay { get; set; }

        /// <summary>
        /// Callback Delegate.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments</param>
        public delegate void Callback(Object sender, EventArgs args);

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// InfoOverlayItem components to make up InfoOverlayItem.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="type">Type.</param>
        /// <param name="position">Position.</param>
        /// <param name="text">Text.</param>
        /// <param name="visible">Visibility.</param>
        /// <param name="displayTime">Display time.</param>
        /// <param name="removeAfterDisplay">Remove after display flag.</param>
        public InfoOverlayItem(string id, InfoType type, Rect position, string text = "", bool visible = false,
            float displayTime = -1, bool removeAfterDisplay = false)
            : this(id, "Default", type, position, text, visible, displayTime, removeAfterDisplay)
        {
        }

        /// <summary>
        /// InfoOverlayItem components to make up InfoOverlayItem including Group.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="group">Group.</param>
        /// <param name="type">Type.</param>
        /// <param name="position">Position.</param>
        /// <param name="text">Text.</param>
        /// <param name="visible">Visibility.</param>
        /// <param name="displayTime">Display time.</param>
        /// <param name="removeAfterDisplay">Remove after display flag.</param>
        public InfoOverlayItem(string id, string group, InfoType type, Rect position, string text = "",
            bool visible = false, float displayTime = -1, bool removeAfterDisplay = false)
        {
            Id = id;
            Group = group;
            Type = type;
            Position = position;
            Text = text;
            Visible = visible;
            DisplayTime = displayTime;
            RemoveAfterDisplay = removeAfterDisplay;
        }

        /// <summary>
        /// InfoOverlayItem components to make up InfoOverlayItem including Group.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="type">Type.</param>
        /// <param name="showOnHidden">Show on hidden flag.</param>
        /// <param name="text">Text.</param>
        /// <param name="visible">Visibility.</param>
        /// <param name="displayTime">Display time.</param>
        /// <param name="removeAfterDisplay">Remove after display flag.</param>
        /// <param name="gameObject">Game object to be operated on.</param>
        /// <param name="camera">The camera.</param>
        public InfoOverlayItem(string id, InfoType type, GameObject gameObject, Camera camera, bool showOnHidden = false,
            string text = "", bool visible = false, float displayTime = -1, bool removeAfterDisplay = false)
            : this(id, "Default", type, GetOverlayRect(gameObject, camera), text, visible, displayTime, removeAfterDisplay)
        {
        }

        /// <summary>
        /// InfoOverlayItem components to make up InfoOverlayItem including Group.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="group">Group.</param>
        /// <param name="type">Type.</param>
        /// <param name="showOnHidden"></param>
        /// <param name="text">Text.</param>
        /// <param name="visible">Visibility.</param>
        /// <param name="displayTime">Display time.</param>
        /// <param name="removeAfterDisplay">Remove after display flag.</param>
        /// <param name="gameObject">Game object to be operated on.</param>
        /// <param name="camera">The camera.</param>
        public InfoOverlayItem(string id, string group, InfoType type, GameObject gameObject, Camera camera, bool showOnHidden = false,
            string text = "", bool visible = false, float displayTime = -1, bool removeAfterDisplay = false)
            : this(id, group, type, GetOverlayRect(gameObject, camera), text, visible, displayTime, removeAfterDisplay)
        {
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Get the overlay rectangle.
        /// </summary>
        /// <param name="gameObject">The game object operated on.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="showOnHidden">Show on hidden flag.</param>
        /// <returns></returns>
        private static Rect GetOverlayRect(GameObject gameObject, Camera camera, bool showOnHidden = false)
        {
            var screenPosition = UtpModuleUtilities.GetObjectPosition(gameObject, camera, UtpModuleUtilities.ObjectPositionType.Screen, !showOnHidden);
            if(screenPosition == null)
            {
                Debug.Log("Couldn't find bounds for '" + gameObject.name + "'");
                return new Rect(0, 0, 0, 0);
            }

            var position = screenPosition.Value;

            // Reposition if the object has 0 width or height
            if(position.width <= 0f)
            {
                position.width = 80;
                position.x -= 40;
            }

            if(position.height <= 0f)
            {
                position.height = 50;
                position.y -= 25;
            }

            return position;
        }

        #endregion Private Methods
    }
}