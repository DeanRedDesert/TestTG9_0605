using System;
using System.Collections.Generic;

namespace Logic.Core.Utility
{
	/// <summary>
	/// The different C# language versions that we want to specifically target in our code gen.
	/// 
	/// Target			Version	C# language version default
	/// .NET			8.x		C# 12
	/// .NET			7.x		C# 11
	/// .NET			6.x		C# 10
	/// .NET			5.x		C# 9.0
	/// .NET Core		3.x		C# 8.0
	/// .NET Core		2.x		C# 7.3
	/// .NET Standard	2.1		C# 8.0
	/// .NET Standard	2.0		C# 7.3
	/// .NET Standard	1.x		C# 7.3
	/// .NET Framework	all		C# 7.3
	///
	/// source: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#c-language-version-reference
	/// </summary>
	public enum LanguageVersion
	{
		// ReSharper disable once InconsistentNaming
		CSharp7_3,
		CSharp11,
		Latest // Currently C# 12
	}

	public sealed class CodeGenArgs
	{
		/// <summary>
		/// The maximum version of the C# language that can we are generating C# code for.
		/// </summary>
		public LanguageVersion LanguageVersion { get; }

		/// <summary>
		/// A callback to call whenever a namespace is encountered.
		/// </summary>
		public Action<string> AddNamespace { get; }

		/// <summary>
		/// Contains object references mapped to their field names in the class.
		/// </summary>
		public IReadOnlyDictionary<object, string> FieldNames { get; }

		public CodeGenArgs(LanguageVersion languageVersion = LanguageVersion.CSharp7_3, Action<string> addNamespace = null, IReadOnlyDictionary<object, string> fieldNames = null)
		{
			LanguageVersion = languageVersion;
			AddNamespace = addNamespace ?? (s => { });
			FieldNames = fieldNames ?? new Dictionary<object, string>();
		}
	}
}