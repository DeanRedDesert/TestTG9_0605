using Logic.Core.Utility;

namespace Logic.Core.Types.WeightScaling
{
	/// <summary>
	/// A mapping of weighted item ids that meet the criteria for the weight distributor mechanism.
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed class WeightDistributorEntry : IToCode, IToString
	{
		/// <summary>
		/// The id for the weighted item that will receive addition weight as scaling occurs.
		/// </summary>
		public string ReceiverId { get; }

		/// <summary>
		/// The id for the weighted item that will have weight removed as scaling occurs.
		/// </summary>
		public string SenderId { get; }

		/// <summary>
		/// The multiplying factor used when transferring weight.
		/// </summary>
		public uint TransferFactor { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="receiver">The id for the weighted item that will receive addition weight as scaling occurs.</param>
		/// <param name="sender">The id for the weighted item that will have weight removed as scaling occurs.</param>
		/// <param name="transferFactor">The multiplying factor used when transferring weight.</param>
		public WeightDistributorEntry(string receiver, string sender, uint transferFactor)
		{
			ReceiverId = receiver;
			SenderId = sender;
			TransferFactor = transferFactor;
		}

		/// <inheritdoc/>
		public IResult ToCode(CodeGenArgs args)
		{
			var receiverId = CodeConverter.ToCodeOrThrow(args, ReceiverId);
			var senderId = CodeConverter.ToCodeOrThrow(args, SenderId);
			var transferFactor = CodeConverter.ToCodeOrThrow(args, TransferFactor);

			return $"new WeightDistributorEntry({receiverId}, {senderId}, {transferFactor})".ToSuccess();
		}

		/// <inheritdoc/>
		public override string ToString() => $"{ReceiverId} <- {SenderId} TransferFactor:{TransferFactor}";

		/// <inheritdoc/>
		public IResult ToString(string format) => ToString().ToSuccess();
	}
}