using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnizanHelper.Models.Searching.Zanmai
{
	internal static class ZanmaiWikiSearchDataHelper
	{
		private static Task WriteRecordAsync(
			this TextWriter writer,
			string recordName,
			params object[] values)
		{
			var line = string.Join(
				"\t",
				new string[] { recordName }
					.Concat(values.Select(x => x?.ToString() ?? string.Empty))
					.Select(x => x.Replace("\\", "\\\\").Replace("\t", "\\\t")));

			return writer.WriteLineAsync(line);
		}

		public static async IAsyncEnumerable<ZanmaiProgram> DeserializeProgramsAsync(
			Stream inputStream,
			[EnumeratorCancellation]CancellationToken cancellationToken = default)
		{
			var programMap = new Dictionary<int, ZanmaiProgram>();
			var programSongCounter = new Dictionary<int, int>();


			using (var reader = new StreamReader(inputStream))
			{
				string line;
				while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
				{
					var tokens = line.Split('\t');
					if (tokens.Length == 0) { continue; }

					switch (tokens[0])
					{
						case "Program":
							if (tokens.Length > 7)
							{
								var programId = int.Parse(tokens[1]);
								programMap[programId] = new ZanmaiProgram
								{
									ProgramTitle = tokens[2],
									SongListPageLink = new WikiSongListPageLink
									{
										Year = tokens[4],
										Date = tokens[5],
										Title = tokens[6],
										Uri = new Uri(tokens[7]),
									},
									Songs = new List<ZanmaiSongListItem>(),
								};

								programSongCounter[programId] = int.Parse(tokens[3]);
							}
							break;

						case "Song":
							if (tokens.Length > 12)
							{
								var programId = int.Parse(tokens[1]);
								if (programMap.TryGetValue(programId, out var program))
								{
									program.Songs.Add(new ZanmaiSongListItem
									{
										ProgramTitle = program.ProgramTitle,
										SongInfo = new ZanmaiSongInfo
										{
											Number = int.Parse(tokens[2]),
											Title = tokens[3],
											Artists = tokens[4].Split(','),
											Genre = tokens[5],
											Series = tokens[6],
											SongType = tokens[7],
											IsSpecialItem = bool.Parse(tokens[8]),
											SpecialHeader = tokens[9],
											SpecialItemName = tokens[10],
											Additional = tokens[11],
											ShortDescription = tokens[12],
										},
									});

									programSongCounter[programId]--;
									if (programSongCounter[programId] == 0)
									{
										programMap.Remove(programId);
										yield return program;
									}
								}
							}
							break;
					}
				}

				foreach (var program in programMap.Values.ToArray())
				{
					yield return program;
				}
			}
		}

		public static async Task SerializeProgramsAsync(
			Stream outputStream,
			IEnumerable<ZanmaiProgram> programs,
			CancellationToken cancellationToken = default)
		{
			using (var writer = new StreamWriter(outputStream))
			{
				var programId = 0;
				foreach (var program in programs)
				{
					await writer.WriteRecordAsync(
						"Program",
						programId,
						program.ProgramTitle,
						program.Songs.Count,
						program.SongListPageLink.Year,
						program.SongListPageLink.Date,
						program.SongListPageLink.Title,
						program.SongListPageLink.Uri.AbsoluteUri)
						.ConfigureAwait(false);

					foreach (var song in program.Songs.Select(x => x.SongInfo))
					{
						await writer.WriteRecordAsync(
							"Song",
							programId,
							song.Number,
							song.Title,
							string.Join(",", song.Artists),
							song.Genre,
							song.Series,
							song.SongType,
							song.IsSpecialItem,
							song.SpecialHeader,
							song.SpecialItemName,
							song.Additional,
							song.ShortDescription)
							.ConfigureAwait(false);
					}

					programId++;
				}

				await writer.FlushAsync().ConfigureAwait(false);
			}
		}
	}
}
