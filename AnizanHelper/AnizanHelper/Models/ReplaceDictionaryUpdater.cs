using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComiketSystem.Csv;

namespace AnizanHelper.Models
{
	public class ReplaceDictionaryUpdater
	{

		public int GetLatestVersionNumber()
		{
			using (var client = new HttpClient())
			using (var stream = client.GetStreamAsync(Constants.ReplaceDictionaryVersionUrl).Result)
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var str = reader.ReadLine();
				return Convert.ToInt32(str);
			}
		}

		public async Task<IEnumerable<ReplaceDictionaryUpdateInfo>> GetUpdateInfoAsync()
		{
			using (var client = new HttpClient())
			{
				var content = await client.GetStringAsync(Constants.ReplaceDictionaryUpdateInfoUrl).ConfigureAwait(false);

				var cs = new CsvSplitter(content);
				var list = new List<ReplaceDictionaryUpdateInfo>();

				while (cs.ToNextLine())
				{
					if (cs.TokenCount < 3) { continue; }

					list.Add(new ReplaceDictionaryUpdateInfo
					{
						Version = cs.GetInt(0),
						Description = cs.GetString(1),
						Date = new DateTimeOffset(DateTime.ParseExact(cs.GetString(2), "yyyy/MM/dd-HH:mm", CultureInfo.CurrentCulture), TimeSpan.FromHours(9))
					});
				}

				return list;
			}
		}

		public string DownloadDictionary()
		{
			using (var client = new HttpClient())
			using (var stream = client.GetStreamAsync(Constants.ReplaceDictionaryUrl).Result)
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
	}

	public class ReplaceDictionaryUpdateInfo
	{
		public int Version { get; set; }

		public string Description { get; set; }

		public DateTimeOffset Date { get; set; }
	}
}
