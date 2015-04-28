using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using ComiketSystem.Csv;

namespace AnizanHelper.Models
{
	public class ReplaceDictionaryUpdater
	{

		public int GetLatestVersionNumber()
		{
			using(var client = new HttpClient())
			using (var stream = client.GetStreamAsync(Constants.ReplaceDictionaryVersionUrl).Result)
			using(var reader = new StreamReader(stream, Encoding.UTF8)){
				var str = reader.ReadLine();
				return Convert.ToInt32(str);
			}
		}

		public IEnumerable<ReplaceDictionaryUpdateInfo> GetUpdateInfo()
		{
			using (var client = new HttpClient())
			using (var stream = client.GetStreamAsync(Constants.ReplaceDictionaryUpdateInfoUrl).Result)
			using (var reader = new StreamReader(stream, Encoding.UTF8)) {
				var cs = new CsvSplitter(reader.ReadToEnd());
				while (cs.ToNextLine()) {
					if (cs.TokenCount < 3) { continue; }
					yield return new ReplaceDictionaryUpdateInfo {
						Version = cs.GetInt(0),
						Description = cs.GetString(1),
						Date = new DateTimeOffset(DateTime.ParseExact(cs.GetString(2), "yyyy/MM/dd-HH:mm", CultureInfo.CurrentCulture), TimeSpan.FromHours(9))
					};
				}
			}
		}

		public string DownloadDictionary()
		{
			using (var client = new HttpClient())
			using (var stream = client.GetStreamAsync(Constants.ReplaceDictionaryUrl).Result)
			using (var reader = new StreamReader(stream, Encoding.UTF8)) {
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
