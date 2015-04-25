using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnizanHelper.Models.SettingComponents;

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
				return GetMe(false, GetMemberName(() => ClearInputAutomatically));
			}
			set
			{
				SetMe(value, GetMemberName(() => ClearInputAutomatically));
			}
		}

		public bool AlwaysOnTop
		{
			get
			{
				return GetMe(true, GetMemberName(() => AlwaysOnTop));
			}
			set
			{
				SetMe(value, GetMemberName(() => AlwaysOnTop));
			}
		}

		public bool CopyAfterParse
		{
			get
			{
				return GetMe(false, GetMemberName(() => CopyAfterParse));
			}
			set
			{
				SetMe(value, GetMemberName(() => CopyAfterParse));
			}
		}

		public bool CopyAfterApply
		{
			get
			{
				return GetMe(false, GetMemberName(() => CopyAfterApply));
			}
			set
			{
				SetMe(value, GetMemberName(() => CopyAfterApply));
			}
		}

		public bool IncrementSongNumberWhenCopied
		{
			get{
				return GetMe(true, GetMemberName(() => IncrementSongNumberWhenCopied));
			}
			set{
				SetMe(value, GetMemberName(() => IncrementSongNumberWhenCopied));
			}
		}
	}
}
