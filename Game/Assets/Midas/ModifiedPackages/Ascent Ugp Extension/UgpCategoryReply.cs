

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp
{
    using F2L.Schemas.Internal;

    /// <summary>
    /// UGP category reply base class.
    /// </summary>
    [DisableCodeCoverageInspection]
    public abstract class UgpCategoryReply
    {
        /// <summary>
        /// The reply code for this message.
        /// </summary>
        public ReplyCodeType Reply { get; set; }
    }
}
