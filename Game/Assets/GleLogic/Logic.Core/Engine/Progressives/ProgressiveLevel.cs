using System.Text;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.Engine.Progressives
{
	/// <summary>
	/// The type of progressive.
	/// </summary>
	// ReSharper disable UnusedMember.Global
	public enum ProgressiveType
	{
		Triggered,
		Mystery
	}
	// ReSharper restore UnusedMember.Global

	public sealed class ProgressiveLevel : IToString, IToCode
	{
		/// <summary>
		/// The progressive set id the level is for,
		/// </summary>
		public string ProgressiveSetId { get; }

		/// <summary>
		/// A unique identifier for the progressive level.
		/// </summary>
		public string Identifier { get; }

		/// <summary>
		/// A friendly name for the progressive level.
		/// </summary>
		public string LevelName { get; }

		/// <summary>
		/// The minimum start up value.
		/// </summary>
		public Money MinStartup { get; }

		/// <summary>
		/// The maximum start up value.
		/// </summary>
		public Money MaxStartup { get; }

		/// <summary>
		/// The main ceiling of this progressive level.
		/// </summary>
		public Money Ceiling { get; }

		/// <summary>
		/// Is the progressive triggered or mystery.
		/// </summary>
		public ProgressiveType ProgressiveType { get; }

		/// <summary>
		/// Is this progressive level standalone or linked.
		/// </summary>
		public bool IsStandalone { get; }

		/// <summary>
		/// Minimum contribution value (raw double and not a percentage).
		/// </summary>
		public double MinContribution { get; }

		/// <summary>
		/// Maximum contribution value (raw double and not a percentage).
		/// </summary>
		public double MaxContribution { get; }

		/// <summary>
		/// Hidden contribution value (raw double and not a percentage).
		/// </summary>
		public double HiddenContribution { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProgressiveLevel(string progressiveSetId, string identifier, string name, Money minStartup, Money maxStartup, Money ceiling, ProgressiveType progressiveType, bool standalone, double minContribution, double maxContribution, double hiddenContribution)
		{
			ProgressiveSetId = progressiveSetId;
			Identifier = identifier;
			LevelName = name;
			MinStartup = minStartup;
			MaxStartup = maxStartup;
			Ceiling = ceiling;
			ProgressiveType = progressiveType;
			IsStandalone = standalone;
			MinContribution = minContribution;
			MaxContribution = maxContribution;
			HiddenContribution = hiddenContribution;
		}

		/// <summary>
		/// Used to generate a string that uniquely identifies the level.
		/// </summary>
		public string GetUniqueIdString()
		{
			return ProgressiveSetId + Identifier + LevelName + HiddenContribution + MinStartup.ToCents() + MaxStartup.ToCents() + Ceiling.ToCents() + ProgressiveType + IsStandalone + MinContribution + MaxContribution;
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "SL")
			{
				var sb = new StringBuilder();
				sb.Append(Identifier);
				sb.Append(' ');
				sb.Append(LevelName);
				sb.Append(' ');
				sb.Append(MinStartup.ToString("SL"));
				sb.Append(' ');
				sb.Append(MaxStartup.ToString("SL"));
				sb.Append(' ');
				sb.Append(ProgressiveType);
				sb.Append(' ');
				sb.Append(IsStandalone);
				sb.Append(' ');
				sb.Append(MinContribution.ToString("0.0000"));
				sb.Append(' ');
				sb.Append(MaxContribution.ToString("0.0000"));
				sb.Append(' ');
				sb.Append(HiddenContribution.ToString("0.0000"));
				sb.Append(' ');
				sb.Append(Ceiling.ToString("SL"));
				return sb.ToString().ToSuccess();
			}

			return new NotSupported();
		}

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args)
		{
			var setId = CodeConverter.ToCodeOrThrow(args, ProgressiveSetId);
			var id = CodeConverter.ToCodeOrThrow(args, Identifier);
			var ln = CodeConverter.ToCodeOrThrow(args, LevelName);
			var mnSt = CodeConverter.ToCodeOrThrow(args, MinStartup);
			var mxSt = CodeConverter.ToCodeOrThrow(args, MaxStartup);
			var cl = CodeConverter.ToCodeOrThrow(args, Ceiling);
			var so = CodeConverter.ToCodeOrThrow(args, IsStandalone);
			var pt = CodeConverter.ToCodeOrThrow(args, ProgressiveType);
			var mnc = CodeConverter.ToCodeOrThrow(args, MinContribution);
			var mxc = CodeConverter.ToCodeOrThrow(args, MaxContribution);
			var hc = CodeConverter.ToCodeOrThrow(args, HiddenContribution);

			return $"new ProgressiveLevel({setId}, {id}, {ln}, {mnSt}, {mxSt}, {cl}, {pt}, {so}, {mnc}, {mxc}, {hc})".ToSuccess();
		}
	}
}