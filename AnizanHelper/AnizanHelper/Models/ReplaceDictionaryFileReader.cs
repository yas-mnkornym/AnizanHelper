using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ComiketSystem.Csv;

namespace AnizanHelper.Models
{
	public class ReplaceDictionaryFileReader
	{
		public int GetVersionNumber(string path)
		{
			if (File.Exists(path)) {
				var str = File.ReadAllText(path, Encoding.UTF8);
				var cs = new CsvSplitter(str);
				while (cs.ToNextLine()) {
					if (cs.TokenCount < 2) { continue; }
					if (cs.GetString(0) == "header") {
						return cs.GetInt(1);
					}
				}
			}
			return 0;
		}

		public IEnumerable<ReplaceInfo> Load(string path)
		{
			var str = File.ReadAllText(path, Encoding.UTF8);
			var cs = new CsvSplitter(str);
			
			while (cs.ToNextLine()) {
				if (cs.TokenCount < 3) { continue; }
				if (cs.GetString(0) == "replace") {
					var info = new ReplaceInfo(cs.GetString(1), cs.GetString(2));

					if (cs.TokenCount > 3) {
						info.SongTitleConstraint = cs.GetString(3);
					}

					if (cs.TokenCount > 4) {
						info.Exact = cs.GetBoolOrDeraulf(4, false);
					}

					yield return info;
				}
			}
		}
	}
}
