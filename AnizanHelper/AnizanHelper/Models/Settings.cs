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
				return GetMe(false);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool AlwaysOnTop
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool CopyAfterParse
		{
			get
			{
				return GetMe(false);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool CopyAfterApply
		{
			get
			{
				return GetMe(false);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool IncrementSongNumberWhenCopied
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public string ServerName
		{
			get
			{
				return GetMe(Constants.DefaultServerName);
			}
			set
			{
				SetMe(value);
			}
		}

		public string BoardPath
		{
			get
			{
				return GetMe(Constants.DefaultBoardPath);
			}
			set
			{
				SetMe(value);
			}
		}

		public string ThreadKey
		{
			get
			{
				return GetMe(string.Empty);
			}
			set
			{
				SetMe(value);
			}
		}

		public string ZanmaiSearchUrl
		{
			get
			{
				return GetMe(Constants.ZanmaiSearchUrl);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool ApplySongInfoAutomatically
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool WriteAsSage
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool CheckSeriesTypeNumberAutomatically
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool SnapListWindow
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public AnizanSongInfo[] SongList
		{
			get
			{
				var ret = GetMe<string>(null);
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
					SetMe<string>(null);
				}
				else
				{
					var json = JsonConvert.SerializeObject(value);
					SetMe(json);
				}
			}
		}

		public bool CheckForUpdateAutomatically
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool ShowParserControl
		{
			get => GetMe(false);
			set => SetMe(value);
		}

		public string MetadataStreamUri
		{
			get => this.GetMe<string>(null);
			set => this.SetMe(value);
		}

		public bool EnableMetadataStreamAutoReconnection
		{
			get => GetMe(true);
			set => SetMe(value);
		}

		public int MaxMetadataStreamAutoReconnectionTrialCount
		{
			get => GetMe(20);
			set => SetMe(value);
		}

		public TimeSpan MetadataStreamReconnectionInterval
		{
			get => GetMe(TimeSpan.FromSeconds(3));
			set => SetMe(value);
		}

		public bool ShowMetadataStreamHistory
		{
			get => GetMe(false);
			set => SetMe(value);
		}

		public bool ShowStreamMetadataRetreiver
		{
			get => GetMe(true);
			set => SetMe(value);
		}

		public bool ShowFrequentlyPlayedSongs
		{
			get => this.GetMe(true);
			set => this.SetMe(value);
		}
	}
}
