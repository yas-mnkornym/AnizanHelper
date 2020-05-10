using System;
using AnizanHelper.Models.SettingComponents;
using Newtonsoft.Json;

namespace AnizanHelper.Models
{
	public class Settings : SettingsBase
	{
		public static readonly Type[] KnownTypes = new Type[]{
			typeof(TimeSpan),
		};

		public Settings(ISettingsContainer settings)
			: base(settings)
		{ }

		public bool ClearInputAutomatically
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public bool AlwaysOnTop
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool CopyAfterParse
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public bool CopyAfterApply
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public bool IncrementSongNumberWhenCopied
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public string ServerName
		{
			get => this.GetValue(Constants.DefaultServerName);
			set => this.SetValue(value);
		}

		public string BoardPath
		{
			get => this.GetValue(Constants.DefaultBoardPath);
			set => this.SetValue(value);
		}

		public string ThreadKey
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string ZanmaiSearchUrl
		{
			get => this.GetValue(Constants.ZanmaiSearchUrl);
			set => this.SetValue(value);
		}

		public string[] DisabledSearchProviders
		{
			get => this.GetValue(Array.Empty<string>());
			set => this.SetValue(value);
		}

		public bool ApplySongInfoAutomatically
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool WriteAsSage
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool CheckSeriesTypeNumberAutomatically
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool SnapListWindow
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool ShowListWindow
		{
			get => this.GetValue<bool>();
			set => this.SetValue(value);
		}

		public ZanmaiSongInfo[] SongList
		{
			get
			{
				var ret = this.GetValue<string>(null);
				if (!string.IsNullOrWhiteSpace(ret))
				{
					try
					{
						return JsonConvert.DeserializeObject<ZanmaiSongInfo[]>(ret);
					}
					catch
					{
						return new ZanmaiSongInfo[] { };
					}
				}
				else
				{
					return new ZanmaiSongInfo[] { };
				}
			}
			set
			{
				if (value == null)
				{
					this.SetValue<string>(null);
				}
				else
				{
					var json = JsonConvert.SerializeObject(value);
					this.SetValue(json);
				}
			}
		}

		public bool CheckForDictionaryUpdateAutomatically
		{
			get => this.GetValue<bool>();
			set => this.SetValue(value);
		}

		public bool CheckForUpdateAutomatically
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool ShowParserControl
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public string MetadataStreamUri
		{
			get => this.GetValue<string>(null);
			set => this.SetValue(value);
		}

		public bool EnableMetadataStreamAutoReconnection
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public string SongMetadatSelectedEncodingName
		{
			get => this.GetValue<string>();
			set => this.SetValue(value);
		}

		public int MaxMetadataStreamAutoReconnectionTrialCount
		{
			get => this.GetValue(20);
			set => this.SetValue(value);
		}

		public TimeSpan MetadataStreamReconnectionInterval
		{
			get => this.GetValue(TimeSpan.FromSeconds(3));
			set => this.SetValue(value);
		}

		public bool ShowMetadataStreamHistory
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public bool ShowStreamMetadataRetreiver
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool ShowFrequentlyPlayedSongs
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public bool ShowSongInfoExtractorControl
		{
			get => this.GetValue(true);
			set => this.SetValue(value);
		}

		public string SongInfoExtractorRegexFormat
		{
			get => this.GetValue(DefaultSongInfoExtractorPresets[0]);
			set => this.SetValue(value);
		}

		public static string[] DefaultSongInfoExtractorPresets { get; } = new string[]{
			@"((?<Artist>.*)(\s+-\s+)(?<Title>.*)|(?<Title>.*))",
			@"((?<Title>.*)(\s+-\s+)(?<Artist>.*)|(?<Title>.*))",
			@"((?<Artist>.*)\s*-\s*(?<Title>.*)|(?<Title>.*))",
			@"((?<Title>.*)\s*-\s*(?<Artist>.*)|(?<Title>.*))",
			@"(｢(?<Title>.*)｣\s*/\s*(?<Artist>.*)\s*\()|(｢(?<Title>.*)｣\s*/\s*(?<Artist>.*))",
		};


		public string[] SongInfoExtractorPresets
		{
			get => this.GetValue(DefaultSongInfoExtractorPresets);
			set => this.SetValue(value);
		}

		public double WindowHeight
		{
			get => this.GetValue(800.0);
			set => this.SetValue(value);
		}

		public double WindowWidth
		{
			get => this.GetValue(730.0);
			set => this.SetValue(value);
		}
	}
}
