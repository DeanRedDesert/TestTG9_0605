namespace Logic.Core.Utility
{
	/// <summary>
	/// Implement this interface on your data types to provide a means for them to
	/// be created from a string. This is generally used in the GLE to provide generic text
	/// editors for objects in the interface and can also be used for serialisation.
	/// </summary>
	public interface IFromString
	{
		// C# 7.3 Does not allow static interface methods, to simulate using them (as they are very useful)
		// we have a bit of a coding-by-convention hack.
		// If your class implements IFromString AND you declare either of the static methods with the signatures
		// below, then we will detect them and call them during string conversion.
		// Search through existing code for examples.

		// Summary: Converts a string into the current type, using the given format.
		// Notes: Implementing this method is mandatory.
		//
		// static IResult FromString(string s, string format);

		// Summary: Converts a string into a list of the current type, using the given format and the given list type.
		// Notes: Implementing this method is optional.
		//
		// static IResult ListFromString(string s, string format, Type listType) => new NotSupported();
	}
}