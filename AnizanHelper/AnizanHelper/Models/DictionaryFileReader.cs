using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ComiketSystem.Csv;

namespace AnizanHelper.Models
{
	public class DictionaryFileReader
	{
		private List<ReplaceInfo> ReplaceList { get; } = new List<ReplaceInfo>();
		private List<AnizanSongInfo> SongPresetList { get; } = new List<AnizanSongInfo>();

		public IEnumerable<ReplaceInfo> Replaces => ReplaceList;
		public IEnumerable<AnizanSongInfo> SongPresets => SongPresetList;

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

		public void Load(string path)
		{
			var str = File.ReadAllText(path, Encoding.UTF8);
			var cs = new CsvSplitter(str);
			
			while (cs.ToNextLine()) {
				if (cs.TokenCount < 3) { continue; }

				var recordName = cs.GetString(0);
				if (recordName == "replace")
				{
					var info = new ReplaceInfo(cs.GetString(1), cs.GetString(2));

					if (cs.TokenCount > 3)
					{
						info.SongTitleConstraint = cs.GetString(3);
					}

					if (cs.TokenCount > 4)
					{
						info.Exact = cs.GetBoolOrDeraulf(4, false);
					}

					ReplaceList.Add(info);
				}
				else if (recordName == "preset")
				{
					var info = new AnizanSongInfo
					{
						ShortDescription = cs.GetString(1),
						Title = cs.GetString(2),
						Singer = cs.GetString(3),
						Genre = cs.GetString(4),
						Series = cs.GetString(5),
						SongType = cs.GetString(6)
					};

					SongPresetList.Add(info);
				}
			}
		}
	}
}
