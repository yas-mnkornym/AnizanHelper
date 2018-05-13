using System;

namespace AnizanHelper.Models.Updating
{
	public class UpdateInfo
	{
		public Version Version { get; set; }
		public UpdateItem[] UpdateItems { get; set; }
	}
}
