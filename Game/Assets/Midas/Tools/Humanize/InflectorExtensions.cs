//The Inflector class was cloned from Inflector (https://github.com/srkirkland/Inflector)

//The MIT License (MIT)

//Copyright (c) 2013 Scott Kirkland

//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#nullable enable
using System;
using System.Text.RegularExpressions;

namespace Midas.Tools.Humanize
{
	public static class InflectorExtensions
	{
		/// <summary>
		/// By default, pascalize converts strings to UpperCamelCase also removing underscores
		/// </summary>
		public static string Pascalize(this string input) =>
			Regex.Replace(input, "(?:[ _-]+|^)([a-zA-Z])", match => match
				.Groups[1]
				.Value.ToUpper());

		/// <summary>
		/// Same as Pascalize except that the first character is lower case
		/// </summary>
		public static string Camelize(this string input)
		{
			var word = input.Pascalize();
			return word.Length > 0
				? string.Concat(char.ToLower(word[0]), word.Substring(1))
				: word;
		}
	}
}