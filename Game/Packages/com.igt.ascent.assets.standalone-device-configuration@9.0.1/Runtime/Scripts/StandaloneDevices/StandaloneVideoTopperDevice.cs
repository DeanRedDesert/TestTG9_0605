//-----------------------------------------------------------------------
// <copyright file = "StandaloneVideoTopperDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using IGT.Game.Core.Communication.Cabinet;
    using IGT.Game.Core.Communication.Cabinet.Standalone;

    #endregion

    public class StandaloneVideoTopperDevice : StandaloneDeviceBase<IVideoTopper>
    {
        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            var videoTopper = new VirtualVideoTopper();

            return videoTopper;
        }
    }
}