using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ComiketSystem.Csv;

namespace AnizanHelper.Models
{
	public class DictionaryFileReader
	{
		private List<ReplaceInfo> ReplaceList { get; } = new List<ReplaceInfo>();
		private List<ZanmaiSongInfo> SongPresetList { get; } = new List<ZanmaiSongInfo>();

		public IEnumerable<ReplaceInfo> Replaces => this.ReplaceList;
		public IEnumerable<ZanmaiSongInfo> SongPresets => this.SongPresetList;

		public int GetVersionNumber(string path)
		{
			if (File.Exists(path))
			{
				var str = File.ReadAllText(path, Encoding.UTF8);
				var cs = new CsvSplitter(str);
				while (cs.ToNextLine())
				{
					if (cs.TokenCount < 2) { continue; }
					if (cs.GetString(0) == "header")
					{
						return cs.GetInt(1);
					}
				}
			}
			return 0;
		}

		public async Task LoadAsync(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new StreamReader(fs, Encoding.UTF8))
			{
				var str = await reader.ReadToEndAsync().ConfigureAwait(false);
				var cs = new CsvSplitter(str);

				while (cs.ToNextLine())
				{
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

						this.ReplaceList.Add(info);
					}
					else if (recordName == "preset")
					{
						var info = new ZanmaiSongInfo
						{
							ShortDescription = cs.GetString(1),
							Title = cs.GetString(2),
							Artists = cs.GetString(3).Split(','),
							Genre = cs.GetString(4),
							Series = cs.GetString(5),
							SongType = cs.GetString(6)
						};

						this.SongPresetList.Add(info);
					}
				}
			}
		}
	}
}
