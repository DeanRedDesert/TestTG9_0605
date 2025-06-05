using System.IO;
using IGT.Game.Core.CompactSerialization;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Ascent.Ugp
{
	internal enum ProgressiveAwardEventType
	{
		Verified,
		Paid
	}

	internal sealed class ProgressiveAwardEvent : ICompactSerializable
	{
		public ProgressiveAwardEventType EventType { get; private set; }
		public int ProgressiveAwardIndex { get; private set; }
		public string ProgressiveLevelId { get; private set; }
		public ProgressiveAwardPayType? PayType { get; private set; }
		public Money Amount { get; private set; }

		public ProgressiveAwardEvent(ProgressiveAwardEventType eventType, int progressiveAwardIndex, string progressiveLevelId, ProgressiveAwardPayType? payType, Money amount)
		{
			EventType = eventType;
			ProgressiveAwardIndex = progressiveAwardIndex;
			ProgressiveLevelId = progressiveLevelId;
			PayType = payType;
			Amount = amount;
		}

		public ProgressiveAwardEvent()
		{
		}

		public void Serialize(Stream stream)
		{
			CompactSerializer.Serialize(stream, (int)EventType);
			CompactSerializer.Serialize(stream, ProgressiveAwardIndex);
			CompactSerializer.Serialize(stream, ProgressiveLevelId);
			CompactSerializer.Serialize(stream, PayType.HasValue);

			if (PayType.HasValue)
				CompactSerializer.Serialize(stream, (int)PayType.Value);

			CompactSerializer.Serialize(stream, Amount.Value.Numerator);
			CompactSerializer.Serialize(stream, Amount.Value.Denominator);
		}

		public void Deserialize(Stream stream)
		{
			EventType = (ProgressiveAwardEventType)CompactSerializer.Deserialize<int>(stream);
			ProgressiveAwardIndex = CompactSerializer.Deserialize<int>(stream);
			ProgressiveLevelId = CompactSerializer.Deserialize<string>(stream);

			if (CompactSerializer.Deserialize<bool>(stream))
				PayType = (ProgressiveAwardPayType)CompactSerializer.Deserialize<int>(stream);
			else
				PayType = null;

			var num = CompactSerializer.Deserialize<long>(stream);
			var den = CompactSerializer.Deserialize<long>(stream);
			Amount = Money.FromRationalNumber(num, den);
		}
	}
}