using log4net.Util;

namespace Midas.Logging
{
	public sealed class DirectoryPatternString : PatternString
	{
		public DirectoryPatternString(string pattern)
			: base(pattern)
		{
		}

		public override void ActivateOptions()
		{
			AddConverter("cs", typeof(DirectoryPatternConverter));
			base.ActivateOptions();
		}
	}
}