// -----------------------------------------------------------------------
//  <copyright file = "UtpInfoOverlayController.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Random = System.Random;

    /// <summary>
    /// Get instance of UtpInfoOverlayController.
    /// </summary>
    public class UtpInfoOverlayController : MonoBehaviour
    {
        #region fields

        /// <summary>
        ///  UtpInfoOverlayController Singleton.
        /// </summary>
        private static UtpInfoOverlayController instance;

        /// <summary>
        /// Game object container.
        /// </summary>
        private static GameObject container;

        /// <summary>
        /// List of InfoOverlayItems.
        /// </summary>
        private readonly List<InfoOverlayItem> items;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the instance of UtpInfoOverlayController.
        /// </summary>
        /// <returns>Instance of the info overlay controller.</returns>
        public static UtpInfoOverlayController GetInstance()
        {
            if(!instance)
            {
                // If there's already one in the scene, get it
                instance =
                    (UtpInfoOverlayController)FindObjectsOfType(typeof(UtpInfoOverlayController)).FirstOrDefault();

                // Otherwise create a new one
                if(!instance)
                {
                    container = new GameObject { name = "UtpInfoOverlayController" };
                    instance = container.AddComponent(typeof(UtpInfoOverlayController)) as UtpInfoOverlayController;

                    var utpController = (UtpController)FindObjectOfType(typeof(UtpController));

                    if(utpController != null && instance != null)
                    {
                        instance.transform.parent = utpController.transform;
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// Add an InfoOverlayItem to the item list.
        /// Item Visible property will be false
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <returns>ID of the item added, null if it already exists.</returns>
        public string AddItem(InfoOverlayItem item)
        {
            while(items.Count(thisItem => thisItem.Id == item.Id) > 0)
            {
                item.Id += new Random().Next(0, 9999999);
            }

            items.Add(item);

            if(item.Visible && item.DisplayTime > -1)
            {
                StartCoroutine(SetVisible(item, item.DisplayTime, item.RemoveAfterDisplay));
            }
            Debug.Log(string.Format("{0} added to overlay list", item.Id));

            return item.Id;
        }

        /// <summary>
        /// Creates a box overlay item in the middle of the screen which displays text and auto hides
        /// </summary>
        /// <param name="text">Text to display in the prompt</param>
        /// <param name="cam">Camera.</param>
        /// <param name="displayTime">Time (in seconds) to display the prompt</param>
        /// <returns>The created prompt item</returns>
        public InfoOverlayItem CreatePromptItem(string text, Camera cam, float displayTime = 10)
        {
            float width = cam.pixelWidth / 2f;
            float height = cam.pixelHeight / 4f;

            var item = new InfoOverlayItem("Prompt",
                InfoOverlayItem.InfoType.Box,
                new Rect(cam.pixelWidth / 2f - width / 2, cam.pixelHeight / 2f - height / 2, width, height),
                text,
                false,
                displayTime,
                true);

            return item;
        }

        /// <summary>
        /// Removes an InfoOverlayItem from available list.
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>Ability to remove item</returns>
        public bool RemoveItem(InfoOverlayItem item)
        {
            return RemoveItem(item.Id);
        }

        /// <summary>
        /// Removes an InfoOverlayItem from available list by ID.
        /// </summary>
        /// <param name="id">Item ID to remove</param>
        /// <returns>Ability to remove item</returns>
        public bool RemoveItem(string id)
        {
            if(items.Count(idItem => idItem.Id == id) > 0)
            {
                items.RemoveAll(idItem => idItem.Id == id);
                Debug.Log(string.Format("{0} was removed from overlay list", id));
                return true;
            }
            Debug.Log(string.Format("{0} wasn't found in overlay list, cannot remove", id));
            return false;
        }

        /// <summary>
        /// Removes an entire group from available list
        /// </summary>
        /// <param name="group">Group name to remove</param>
        /// <returns>Ability to remove the group</returns>
        public bool RemoveGroup(string group)
        {
            try
            {
                var groupItems = items.Where(item => item.Group == group);

                var infoOverlayItems = groupItems as InfoOverlayItem[] ?? groupItems.ToArray();

                if(infoOverlayItems.Any())
                {
                    foreach(var item in infoOverlayItems)
                    {
                        RemoveItem(item);
                    }
                    return true;
                }
                Debug.Log(string.Format("No info overlay items found in group {0}", group));
                return false;
            }
            catch(Exception ex)
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Removes all InfoOverlayItems
        /// </summary>
        /// <returns>Ability to remove the items</returns>
        public bool RemoveAll()
        {
            try
            {
                items.Clear();
                return true;
            }
            catch(Exception ex)
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Sets an InfoOverlayItem's visibility by ID
        /// </summary>
        /// <param name="id">ID of InfoOverlayItem to set visibility</param>
        /// <param name="visible">The visibility</param>
        /// <param name="displayTime">Amount of time (in seconds) to display the item</param>
        /// <returns>Ability to set the visibility</returns>
        public bool Show(string id, bool visible = true, float displayTime = -1)
        {
            try
            {
                var itemList = items.FindAll(item => item.Id == id);
                foreach(var item in itemList)
                {
                    item.Visible = visible;
                    if(visible && displayTime > -1)
                    {
                        StartCoroutine(SetVisible(item, displayTime));
                    }
                }

                if(itemList.Count > 0)
                {
                    return true;
                }

                Debug.Log(string.Format("Overlay items list doesn't contain id {0}", id));

                return false;
            }
            catch(Exception ex)
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Sets a group of InfoOverlayItem's visibility
        /// </summary>
        /// <param name="group">Group name</param>
        /// <param name="visible">The visibility</param>
        /// <param name="displayTime">Time (in seconds) to display the group</param>
        /// <returns>Ability to set the visibility</returns>
        public bool ShowGroup(string group, bool visible = true, float displayTime = -1)
        {
            try
            {
                var groupItems = items.Where(item => item.Group == group);

                foreach(var item in groupItems)
                {
                    item.Visible = visible;
                }

                if(visible && displayTime > -1)
                {
                    StartCoroutine(SetGroupVisible(group, displayTime));
                }

                return true;
            }
            catch(Exception ex)
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Set an InfoOverlayItem's visibility
        /// </summary>
        /// <param name="item">InfoOverlayItem to set visibility</param>
        /// <param name="visible">The visibility</param>
        /// <returns>Ability to set the visibility</returns>
        public bool Show(InfoOverlayItem item, bool visible = true)
        {
            return Show(item.Id, visible);
        }

        /// <summary>
        /// Set an InfoOverlayItem's visibility to false by ID
        /// </summary>
        /// <param name="id">ID of the item to hide</param>
        /// <returns>Ability to set the visibility</returns>
        public bool Hide(string id)
        {
            return Show(id, false);
        }

        /// <summary>
        /// Set an InfoOverlayItem's visibility to false
        /// </summary>
        /// <param name="item">Item to hide</param>
        /// <returns>Ability to set the visibility</returns>
        public bool Hide(InfoOverlayItem item)
        {
            return Show(item, false);
        }

        /// <summary>
        /// Sets a group's visibility to false
        /// </summary>
        /// <param name="group">Group name</param>
        /// <returns>Ability to set the visibility</returns>
        public bool HideGroup(string group)
        {
            return ShowGroup(group, false);
        }

        /// <summary>
        /// Sets all items visibility
        /// </summary>
        /// <param name="visible">The visibility</param>
        /// <param name="displayTime">Amount of time (in seconds) to display the item</param>
        /// <returns>Ability to set the visibility</returns>
        public bool ShowAll(bool visible = true, float displayTime = -1)
        {
            foreach(var item in items)
            {
                item.Visible = visible;
            }

            if(displayTime > -1)
            {
                StartCoroutine(SetAllVisible(displayTime));
            }

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// UtpInfoOverlayItem container.
        /// </summary>
        private UtpInfoOverlayController()
        {
            items = new List<InfoOverlayItem>();
        }

        /// <summary>
        /// Draws GUI items.
        /// </summary>
        private void OnGUI()
        {
            // Display all currently active items
            var visibleItems = items.FindAll(item => item.Visible);

            foreach(var item in visibleItems)
            {
                DrawItem(item);
            }
        }

        /// <summary>
        /// Draws an item.
        /// </summary>
        /// <param name="item">The item to draw.</param>
        private void DrawItem(InfoOverlayItem item)
        {
            GUIStyle guiStyle = GetStyleOverride(item.Type);

            if(item.Color != Color.clear)
            {
                guiStyle.normal.textColor = item.Color;
            }
            if(item.FontSize != -1)
            {
                guiStyle.fontSize = item.FontSize;
            }
            if(item.Alignment != TextAnchor.MiddleCenter)
            {
                guiStyle.alignment = item.Alignment;
            }

            switch(item.Type)
            {
                case InfoOverlayItem.InfoType.Box:
                    GUI.Box(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.Label:
                    GUI.Label(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.Button:
                    GUI.Button(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.RepeatButton:
                    GUI.RepeatButton(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.TextField:
                    GUI.TextField(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.TextArea:
                    GUI.TextArea(item.Position, item.Text, guiStyle);
                    break;
                case InfoOverlayItem.InfoType.Toggle:
                    GUI.Toggle(item.Position, bool.Parse(item.Value), item.Text, guiStyle);
                    break;
                default:
                    Debug.Log(string.Format("drawItem for {0} not implemented", item.Type));
                    break;
            }
        }

        /// <summary>
        /// Creates a modified style based on the item type.
        /// </summary>
        /// <param name="type">Item type to base it on.</param>
        /// <returns>UTP specific style.</returns>
        private static GUIStyle GetStyleOverride(InfoOverlayItem.InfoType type)
        {
            GUIStyle style;
            switch(type)
            {
                case InfoOverlayItem.InfoType.Box:
                    style = new GUIStyle(GUI.skin.box);
                    break;
                case InfoOverlayItem.InfoType.Label:
                    style = new GUIStyle(GUI.skin.label);
                    break;
                case InfoOverlayItem.InfoType.Button:
                    style = new GUIStyle(GUI.skin.button);
                    break;
                case InfoOverlayItem.InfoType.RepeatButton:
                    style = new GUIStyle(GUI.skin.button);
                    break;
                case InfoOverlayItem.InfoType.TextField:
                    style = new GUIStyle(GUI.skin.textField);
                    break;
                case InfoOverlayItem.InfoType.TextArea:
                    style = new GUIStyle(GUI.skin.textArea);
                    break;
                case InfoOverlayItem.InfoType.Toggle:
                    style = new GUIStyle(GUI.skin.toggle);
                    break;
                default:
                    return null;
            }

            style.fontSize = 32;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;

            return style;
        }

        /// <summary>
        /// Set item to be visible.
        /// </summary>
        /// <param name="item">Item to set.</param>
        /// <param name="displayTime">Time to display.</param>
        /// <param name="removeAfterDisplay">Remove item if flag is set.</param>
        /// <returns></returns>
        private IEnumerator SetVisible(InfoOverlayItem item, float displayTime, bool removeAfterDisplay = false)
        {
            item.Visible = true;
            yield return new WaitForSeconds(displayTime);
            item.Visible = false;
            if(removeAfterDisplay)
            {
                RemoveItem(item);
            }
        }

        /// <summary>
        /// Set group to be visible.
        /// </summary>
        /// <param name="group">Group to set.</param>
        /// <param name="displayTime">Length of time to display.</param>
        /// <returns>Display time innumerator.</returns>
        private IEnumerator SetGroupVisible(string group, float displayTime)
        {
            var groupItems = items.Where(i => i.Group == group);

            var infoOverlayItems = groupItems as IList<InfoOverlayItem> ?? groupItems.ToList();

            foreach(var item in infoOverlayItems)
            {
                item.Visible = true;
            }

            yield return new WaitForSeconds(displayTime);

            foreach(var item in infoOverlayItems)
            {
                item.Visible = false;
            }
        }

        /// <summary>
        /// Set all items to be visible.
        /// </summary>
        /// <param name="displayTime">Length of time to display.</param>
        /// <returns>Display time inummerator.</returns>
        private IEnumerator SetAllVisible(float displayTime)
        {
            foreach(var item in items)
            {
                item.Visible = true;
            }

            yield return new WaitForSeconds(displayTime);

            foreach(var item in items)
            {
                item.Visible = false;
            }
        }

        /// <summary>
        /// Sets all items visibility to false
        /// </summary>
        /// <returns>Ability to set the visibility</returns>
        public bool HideAll()
        {
            return ShowAll(false);
        }

        #endregion Private Methods
    }
}