using System;
using AnizanHelper.Models.SettingComponents;
using Newtonsoft.Json;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models
{
	public class Settings : SettingsBase
	{
		public static readonly Type[] KnownTypes = new Type[]{
			typeof(TimeSpan),
		};

		public Settings(ISettings settings, IDispatcher dispatcher)
			: base(settings, dispatcher)
		{ }

		public bool ClearInputAutomatically
		{
			get
			{
				return GetValue(false);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool AlwaysOnTop
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool CopyAfterParse
		{
			get
			{
				return GetValue(false);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool CopyAfterApply
		{
			get
			{
				return GetValue(false);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool IncrementSongNumberWhenCopied
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public string ServerName
		{
			get
			{
				return GetValue(Constants.DefaultServerName);
			}
			set
			{
				SetValue(value);
			}
		}

		public string BoardPath
		{
			get
			{
				return GetValue(Constants.DefaultBoardPath);
			}
			set
			{
				SetValue(value);
			}
		}

		public string ThreadKey
		{
			get
			{
				return GetValue(string.Empty);
			}
			set
			{
				SetValue(value);
			}
		}

		public string ZanmaiSearchUrl
		{
			get
			{
				return GetValue(Constants.ZanmaiSearchUrl);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool ApplySongInfoAutomatically
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool WriteAsSage
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool CheckSeriesTypeNumberAutomatically
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool SnapListWindow
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public AnizanSongInfo[] SongList
		{
			get
			{
				var ret = GetValue<string>(null);
				if (!string.IsNullOrWhiteSpace(ret))
				{
					try
					{
						return JsonConvert.DeserializeObject<AnizanSongInfo[]>(ret);
					}
					catch
					{
						return new AnizanSongInfo[] { };
					}
				}
				else
				{
					return new AnizanSongInfo[] { };
				}
			}
			set
			{
				if (value == null)
				{
					SetValue<string>(null);
				}
				else
				{
					var json = JsonConvert.SerializeObject(value);
					SetValue(json);
				}
			}
		}

		public bool CheckForUpdateAutomatically
		{
			get
			{
				return GetValue(true);
			}
			set
			{
				SetValue(value);
			}
		}

		public bool ShowParserControl
		{
			get => GetValue(false);
			set => SetValue(value);
		}

		public string MetadataStreamUri
		{
			get => this.GetValue<string>(null);
			set => this.SetValue(value);
		}

		public bool EnableMetadataStreamAutoReconnection
		{
			get => GetValue(true);
			set => SetValue(value);
		}

		public int MaxMetadataStreamAutoReconnectionTrialCount
		{
			get => GetValue(20);
			set => SetValue(value);
		}

		public TimeSpan MetadataStreamReconnectionInterval
		{
			get => GetValue(TimeSpan.FromSeconds(3));
			set => SetValue(value);
		}

		public bool ShowMetadataStreamHistory
		{
			get => GetValue(false);
			set => SetValue(value);
		}

		public bool ShowStreamMetadataRetreiver
		{
			get => GetValue(true);
			set => SetValue(value);
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
