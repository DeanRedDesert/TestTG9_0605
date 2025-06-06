//-----------------------------------------------------------------------
// <copyright file = "MachineSupervisionCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using F2XTransport;
    using Schemas.Internal.MachineSupervision;

    /// <summary>
    /// Implementation of the F2X <see cref="MachineSupervision"/> category.
    /// The MachineSupervision category of messages is used to monitor and affect the EGM machine state.
    /// Category: 154; Major Version: 1
    /// </summary>
    public class MachineSupervisionCategory : F2XCategoryBase<MachineSupervision>, IMachineSupervisionCategory
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="MachineSupervisionCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public MachineSupervisionCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.MachineSupervision;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IMachineSupervisionCategory Members

        /// <inheritdoc/>
        public void RequestMachineReboot()
        {
            var request = CreateBasicRequest<RequestMachineRebootSend>();

            var reply = SendMessageAndGetReply<RequestMachineRebootReply>(Channel.Game, request);
            CheckReply(reply.Exception);
        }

        #endregion

    }

}

