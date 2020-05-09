using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AnizanHelper.Models.Serializers;

namespace AnizanHelper.Models.Searching.Zanmai
{
	public class ZanmaiWikiSearchProviderOptions
	{
		public string IndexFilePath { get; set; }
	}

	public class ZanmaiWikiSearchProvider : ISongSearchProvider
	{
		public string Id => nameof(ZanmaiWikiSearchProvider);

		// TODO: Create efficient index
		// TODO: Support live update
		private List<ZanmaiProgram> Programs { get; } = new List<ZanmaiProgram>();
		private Task InitializationTask { get; }
		private static AnizanListSerializer AnizanListSerializer { get; } = new AnizanListSerializer();

		public ZanmaiWikiSearchProvider(ZanmaiWikiSearchProviderOptions options)
		{
			if (options == null) { throw new ArgumentNullException(nameof(options)); }

			this.InitializationTask = Task.Run(async () =>
			{
				try
				{
					using (var fs = new FileStream(options.IndexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var programs = await ZanmaiWikiSearchDataHelper.DeserializeProgramsAsync(fs)
							.ToArrayAsync()
							.ConfigureAwait(false);

						this.Programs.AddRange(programs);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Failed to load search index.");
					Debug.WriteLine(ex);
				}
			});
		}

		public Task<ZanmaiSongInfo> ConvertToZanmaiSongInfoAsync(ISongSearchResult songSearchResult, CancellationToken cancellationToken = default)
		{
			if (songSearchResult is ZanmaiSongSearchResult zanmaiSongSearchResult)
			{
				var result = zanmaiSongSearchResult.SongListItem.SongInfo;
				return Task.FromResult(result);
			}

			throw new ArgumentException($"Unsupported result type.", nameof(songSearchResult));
		}

		public async IAsyncEnumerable<ISongSearchResult> SearchAsync(string searchTerm, Dictionary<string, string> options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
		{
			if (searchTerm == null) { throw new ArgumentNullException(nameof(searchTerm)); }

			await this.InitializationTask.ConfigureAwait(false);

			var compareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase;
			foreach (var program in this.Programs)
			{
				cancellationToken.ThrowIfCancellationRequested();
				foreach (var item in program.Songs)
				{
					var song = item.SongInfo;
					var score = 0;

					if (song.IsSpecialItem)
					{
						continue;
					}

					if (string.Compare(song.Title, searchTerm, true) == 0)
					{
						score += 5;
					}
					else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(song.Title, searchTerm, compareOptions) >= 0)
					{
						score += 4;
					}

					if(song.Artists.Any(x => CultureInfo.InvariantCulture.CompareInfo.IndexOf(x, searchTerm, compareOptions) >= 0))
					{
						score += 2;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(song.Series, searchTerm, compareOptions) >= 0)
					{
						score += 1;
					}

					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(song.Additional, searchTerm, compareOptions) >= 0)
					{
						score += 1;
					}

					if (score != 0)
					{
						yield return new ZanmaiSongSearchResult
						{
							Score = score,
							SongPageLink = program.SongListPageLink,
							SongListItem = item,
						};
					}
				}
			}
		}

		public class ZanmaiSongSearchResult : ISongSearchResult
		{
			public int Score { get; set; }
			internal WikiSongListPageLink SongPageLink { get; set; }
			internal ZanmaiSongListItem SongListItem { get; set; }

			public string ProviderId => nameof(ZanmaiWikiSearchProvider);
			public string ShortProviderIdentifier => $"三昧{this.SongPageLink.Year}";
			public string[] Artists => this.SongListItem.SongInfo.Artists;
			public string Genre => this.SongListItem.SongInfo.Genre;
			public string Series => this.SongListItem.SongInfo.Series;
			public string SongType => this.SongListItem.SongInfo.SongType;
			public string Title => this.SongListItem.SongInfo.Title;

			public string Note => string.Join(
				"\n",
				string.Format("{0} / {1}", this.SongPageLink.Year, this.SongPageLink.Date),
				this.SongListItem.ProgramTitle,
				AnizanListSerializer.SerializeFull(this.SongListItem.SongInfo));
		}
	}
}
