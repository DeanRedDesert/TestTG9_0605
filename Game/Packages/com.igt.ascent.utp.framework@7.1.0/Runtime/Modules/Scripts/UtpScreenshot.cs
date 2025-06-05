//-----------------------------------------------------------------------
// <copyright file = "UtpScreenshot.cs" company = "IGT">
//     Copyright © 2015-2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Framework.Communications;
    using UnityEngine;
    using Framework;

    public class UtpScreenshot : AutomationModule
    {
        public override bool Initialize()
        {
            return true;
        }

        public override string Name
        {
            get { return "Screenshot"; }
        }

        public override Version ModuleVersion
        {
            get { return new Version(1, 1, 0); }
        }

        [ModuleCommand("GetScreenshot", "Image-JPG Screenshot", "Gets a screenshot of the current view",
            new[] { "Quality|Int|JPEG image quality compression; default is '80'" })]
        public bool GetScreenshot(AutomationCommand command, IUtpCommunication sender)
        {
            var parms = AutomationParameter.GetParameterDictionary(command);
            int quality;
            if(!int.TryParse(parms["quality"].FirstOrDefault(), out quality))
                quality = 80;

            UtpController.StartCoroutine(GetScreenShot(command, sender, quality));
            return true;
        }

        /// <summary>
        /// Gets a screenshot as a coroutine at the end of the frame and sends it to the client that requested it
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="sender">The requester</param>
        /// <param name="quality">JPG quality of the screenshot</param>
        private IEnumerator GetScreenShot(AutomationCommand command, IUtpCommunication sender, int quality)
        {
            yield return new WaitForEndOfFrame();

            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToJPG(quality);
            Destroy(tex);

            var imageData = Convert.ToBase64String(bytes);

            AutomationCommand result = new AutomationCommand(command.Module,
                command.Command,
                new List<AutomationParameter>
                {
                    new AutomationParameter("Screenshot", imageData, "Image-JPG", "Screenshot of the game")
                });
            SendCommand(result, sender);
        }
    }
}