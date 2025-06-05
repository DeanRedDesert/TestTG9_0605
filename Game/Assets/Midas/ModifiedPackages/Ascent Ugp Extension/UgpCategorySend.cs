

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp
{
    /// <summary>
    /// UGP category send base class.
    /// </summary>
    [DisableCodeCoverageInspection]
    public abstract class UgpCategorySend
    {
        /// <summary>
        /// The transaction ID associated with this message.
        /// </summary>
// ReSharper disable InconsistentNaming
        public uint TransactionID { get; set; }
// ReSharper restore InconsistentNaming
    }
}
