namespace Logic.Core.Utility
{
	/// <summary>
	/// Implement this interface on your data types to provide a means for them to
	/// be converted to a string. These strings will be shown in the GLE and can be used
	/// for serialisation.
	/// </summary>
	public interface IToString
	{
		/// <summary>
		/// Convert the object to a string based on the specified format:
		///  SL = Single line
		///  ML = Multi line
		/// </summary>
		IResult ToString(string format);

		// C# 7.3 Does not allow static interface methods, to simulate using them (as they are very useful)
		// we have a bit of a coding-by-convention hack.
		// If your class implements IToString AND you declare a static method with the signature below, then
		// we will detect it and call it during string conversion.
		// Search through existing code for examples.

		// Summary: Converts a list of the current type into string, using the given format.
		// Notes: Implementing this method is optional.
		//
		// static IResult ListToString(object list, string format) => new NotSupported();
	}
}