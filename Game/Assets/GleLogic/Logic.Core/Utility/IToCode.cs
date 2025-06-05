namespace Logic.Core.Utility
{
	/// <summary>
	/// Implement this interface on your data types to provide a means for them to
	/// be converted to C# code.
	/// </summary>
	public interface IToCode
	{
		/// <summary>
		/// Convert the object to a C# code string.
		/// </summary>
		/// <param name="args">Context and args to assist with the code generation.</param>
		IResult ToCode(CodeGenArgs args);

		// C# 7.3 Does not allow static interface methods, to simulate using them (as they are very useful)
		// we have a bit of a coding-by-convention hack.
		// If your class implements IToCode AND you declare a static method with the signature below, then
		// we will detect it and call it during code creation.
		// Search through existing code for examples.

		// Summary: Converts a list of the current type into C# string.
		// Notes: Implementing this method is optional.
		//
		// static IResult ListToCode(CodeGenArgs args, object list) => new NotSupported();
	}
}