using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Data
{
	internal class ReplaceInfo
	{
		public ReplaceInfo(string original, string replaced)
		{
			Original = original;
			Replaced = replaced;
		}

		public string Original { get; set; }
		public string Replaced { get; set; }
	}
}
