using System;

namespace AnizanHelper.Models.Updating
{
	public class ReleaseNote
	{
		public Version Version { get; set; }
		public string[] Messages { get; set; }
	}
}
