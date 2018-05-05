﻿using System;
using AnizanHelper.Models.SettingComponents;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models
{
	public class Settings : SettingsBase
	{
		public static readonly Type[] KnownTypes = new Type[]{
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
			get{
				return GetMe(true);
			}
			set{
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
				return GetMe(false);
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
	}
}
