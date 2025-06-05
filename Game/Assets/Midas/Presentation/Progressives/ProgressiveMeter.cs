using System;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Presentation.Progressives
{
	public sealed class ProgressiveMeter
	{
		#region Lite progressive meter tweener

		private const long Precision = 10000;

		private abstract class MeterCurve
		{
			private readonly double duration;
			private readonly TimeSpan startTime;

			protected MeterCurve(double duration)
			{
				this.duration = duration;
				startTime = FrameTime.CurrentTime;
			}

			public Money? GetValue()
			{
				var t = (FrameTime.CurrentTime - startTime).TotalSeconds / duration;
				if (t >= 1)
					return null;

				return GetValue(t);
			}

			protected abstract Money GetValue(double t);
		}

		private sealed class MeterQuadCurve : MeterCurve
		{
			private readonly Money initialValue;
			private readonly double difference;

			public MeterQuadCurve(double duration, Money initialValue, Money finalValue) : base(duration)
			{
				this.initialValue = initialValue;
				difference = (finalValue - initialValue).Value.ToDouble();
			}

			protected override Money GetValue(double t)
			{
				var x = difference * (1 - (1 - t) * (1 - t)) * Precision;
				var roundedDiff = new RationalNumber((int)x, Precision);

				return initialValue + Money.FromRationalNumber(roundedDiff);
			}
		}

		private sealed class MeterCrossFade : MeterCurve
		{
			private readonly double fadeT;
			private MeterCurve a;
			private double lastAVal;
			private readonly MeterCurve b;

			public MeterCrossFade(double duration, double fadeDuration, Money a, MeterCurve b) : base(duration)
			{
				lastAVal = a.Value.ToDouble();
				fadeT = fadeDuration / duration;
				this.a = null;
				this.b = b;
			}

			public MeterCrossFade(double duration, double fadeDuration, MeterCurve a, MeterCurve b) : base(duration)
			{
				var aVal = a.GetValue();
				if (!aVal.HasValue)
					throw new InvalidOperationException("a did not return a value.");

				lastAVal = aVal.Value.Value.ToDouble();
				fadeT = fadeDuration / duration;
				this.a = a;
				this.b = b;
			}

			protected override Money GetValue(double t)
			{
				if (a != null)
				{
					if (t >= fadeT)
						a = null;
					else
					{
						var aVal = a.GetValue();
						if (aVal == null)
							a = null;
						else
							lastAVal = aVal.Value.Value.ToDouble();
					}
				}

				if (t < fadeT)
				{
					var bValDouble = b.GetValue().GetValueOrDefault().Value.ToDouble();
					var tFrac = t / fadeT;
					var aPart = (1 - tFrac) * (1 - tFrac);
					var val = lastAVal * aPart + bValDouble * (1 - aPart);
					var roundedVal = new RationalNumber((long)(val * Precision), Precision);
					return Money.FromRationalNumber(roundedVal);
				}

				return b.GetValue().GetValueOrDefault();
			}
		}

		#endregion

		private MeterCurve currentCurve;
		private Money? currentValue;

		public void SetValue(Money newValue, bool snap)
		{
			var cv = currentCurve?.GetValue() ?? currentValue;

			if (snap || cv == null || newValue < cv)
			{
				currentCurve = null;
			}
			else if (newValue != currentValue)
			{
				var newCurve = new MeterQuadCurve(10.0, cv.Value, newValue);
				currentCurve = currentCurve?.GetValue() == null
					? new MeterCrossFade(10, 2, cv.Value, newCurve)
					: new MeterCrossFade(10, 2, currentCurve, newCurve);
			}

			currentValue = newValue;
		}

		public Money? GetDisplayValue()
		{
			if (currentCurve != null)
			{
				var currentDisplayValue = currentCurve.GetValue();
				if (currentDisplayValue != null)
				{
					return currentDisplayValue;
				}

				currentCurve = null;
			}

			return currentValue;
		}
	}
}