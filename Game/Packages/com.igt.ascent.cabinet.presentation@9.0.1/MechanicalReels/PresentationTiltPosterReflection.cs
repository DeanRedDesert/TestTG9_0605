// -----------------------------------------------------------------------
// <copyright file = "${File.FileName}" company = "IGT">
//     Copyright (c) ${CurrentDate.Year} IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Tilts;

    /// <summary>
    /// This class use reflection to delegate the tilt post functions.
    /// </summary>
    internal class PresentationTiltPosterReflection : IPresentationTiltPosterProxy
    {
        #region LogicPresentationBridge

        private static PropertyInfo bridgeInitialized;
        private static MethodInfo bridgePostTilt;
        private static MethodInfo bridgeClearTilt;

        private const string Gl2PBridgeAssemblyName = "IGT.Game.Core.Communication.LogicPresentationBridge";
        private const string PresentationTiltPosterTypeName =
            "IGT.Game.Core.Communication.LogicPresentationBridge.PresentationTiltPoster";
        
        #endregion

        /// <summary>
        /// Constructor. Load the PresentationTiltPoster type by reflection.
        /// </summary>
        public PresentationTiltPosterReflection()
        {
            var bridgeAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                asm => asm.GetName().Name == Gl2PBridgeAssemblyName);
            if(bridgeAssembly == null)
            {
                throw new PresentationTiltPosterNotImplementedException(
                    $"Assembly '{Gl2PBridgeAssemblyName}' is not found.");
            }
            
            var bridgeType = bridgeAssembly.GetType(PresentationTiltPosterTypeName);

            if(bridgeType == null)
            {
                throw new PresentationTiltPosterNotImplementedException(
                    $"The type '{PresentationTiltPosterTypeName}' cannot  be found" +
                    $" from the assembly '{Gl2PBridgeAssemblyName}'.");
            }
            
            bridgeInitialized = bridgeType.GetProperty("Initialized", BindingFlags.Static | BindingFlags.Public);
            bridgePostTilt = bridgeType.GetMethod("PostTilt", BindingFlags.Static | BindingFlags.Public);
            bridgeClearTilt = bridgeType.GetMethod("ClearTilt", BindingFlags.Static | BindingFlags.Public);
            
            if(bridgeInitialized == null || bridgePostTilt == null || bridgeClearTilt == null)
            {
                throw new PresentationTiltPosterNotImplementedException(
                    $"The members of the type '{PresentationTiltPosterTypeName}' " +
                    $"does not match the interface {nameof(IPresentationTiltPosterProxy)}'s definition.");
            }
        }

        #region IPresentationTiltPosterProxy

        /// <inheritdoc />
        public bool Initialized => (bool)bridgeInitialized.GetValue(null);
        
        /// <inheritdoc />
        public void PostTilt(string tiltKey, ITilt presentationTilt)
        {
            bridgePostTilt.Invoke(null, new object[] {tiltKey, presentationTilt});
        }

        /// <inheritdoc />
        public void ClearTilt(string tiltKey)
        {
            bridgeClearTilt.Invoke(null, new object[] { tiltKey });
        }
        
        #endregion
    }
}