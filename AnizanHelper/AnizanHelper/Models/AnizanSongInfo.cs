using Studiotaiha.Toolkit;

namespace AnizanHelper.Models
{
	internal class AnizanSongInfo : NotificationObject
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
				SetValue(ref title_, value);
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
				SetValue(ref singer_, value);
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
				SetValue(ref series_, value);
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
				SetValue(ref genre_, value);
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
				SetValue(ref songType_, value);
			}
		}
		#endregion

		/// <summary>
		/// 補足情報
		/// </summary>
		#region Additional
		string additional_ = null;
		public string Additional
		{
			get
			{
				return additional_;
			}
			set
			{
				SetValue(ref additional_, value);
			}
		}
		#endregion

		public override string ToString()
		{
			return Utils.GeneralObjectToString(this);
		}
	}
}
