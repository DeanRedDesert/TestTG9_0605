namespace Logic.Core.Utility
{
	/// <summary>
	/// A generic result type that is used as a poor mans discriminated union.
	/// Used as the result of all ToString and FromString methods.
	/// It can (currently) be represented by 3 states:
	/// - Success of Value
	/// - Error of Description
	/// - NotSupported
	/// </summary>
	public interface IResult
	{
	}

	/// <summary>
	/// Base class for the Success state.
	/// </summary>
	public abstract class Success<T> : IResult
	{
		public T Value { get; protected set; }
	}

	/// <summary>
	/// Represents the successful completion of a ToString conversion.
	/// </summary>
	public sealed class StringSuccess : Success<string>
	{
		public StringSuccess(string value) => Value = value;
	}

	/// <summary>
	/// Represents the successful completion of a FromString conversion.
	/// </summary>
	public sealed class ObjectSuccess : Success<object>
	{
		public ObjectSuccess(object value) => Value = value;
	}

	/// <summary>
	/// Represents an unsuccessful completion a ToString or FromString conversion.
	/// </summary>
	public sealed class Error : IResult
	{
		public string Description { get; }
		public Error(string description) => Description = description;
	}

	/// <summary>
	/// Represents that a ToString or FromString conversion is not supported for the given type or format.
	/// </summary>
	public sealed class NotSupported : IResult
	{
	}
}