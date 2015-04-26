using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnizanHelper.Models.SettingComponents;

namespace AnizanHelper.Models
{
	internal class AnizanSongInfo : BindableBase
	{
		/// <summary>
		/// 曲名
		/// </summary>
		#region Title
		string title_ = null;
		public string Title
		{
			get
			{
				return title_;
			}
			set
			{
				SetValue(ref title_, value, GetMemberName(() => Title));
			}
		}
		#endregion

		/// <summary>
		/// 歌手
		/// </summary>
		#region Singers
		string singer_ = null;
		public string Singer
		{
			get
			{
				return singer_;
			}
			set
			{
				SetValue(ref singer_, value, GetMemberName(() => Singer));
			}
		}
		#endregion

		/// <summary>
		/// 作品名
		/// </summary>
		#region Series
		string series_ = null;
		public string Series
		{
			get
			{
				return series_;
			}
			set
			{
				SetValue(ref series_, value, GetMemberName(() => Series));
			}
		}
		#endregion

		/// <summary>
		/// ジャンル
		/// </summary>
		#region Genre
		string genre_ = null;
		public string Genre
		{
			get
			{
				return genre_;
			}
			set
			{
				SetValue(ref genre_, value, GetMemberName(() => Genre));
			}
		}
		#endregion

		/// <summary>
		/// 曲種
		/// </summary>
		#region SongType
		string songType_ = null;
		public string SongType
		{
			get
			{
				return songType_;
			}
			set
			{
				SetValue(ref songType_, value, GetMemberName(() => SongType));
			}
		}
		#endregion

		public override string ToString()
		{
			return Utils.GeneralObjectToString(this);
		}
	}
}
