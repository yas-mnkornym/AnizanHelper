using Newtonsoft.Json;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models
{
	public class AnizanSongInfo : NotificationObjectWithPropertyBag
	{
		/// <summary>
		/// 曲名
		/// </summary>
		[JsonProperty]
		public string Title
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// 歌手
		/// </summary>
		[JsonProperty]
		public string Singer
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// 作品名
		/// </summary>
		[JsonProperty]
		public string Series
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// ジャンル
		/// </summary>
		[JsonProperty]
		public string Genre
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// 曲種
		/// </summary>
		[JsonProperty]
		public string SongType
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		/// <summary>
		/// 補足情報
		/// </summary>
		[JsonProperty]
		public string Additional
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		[JsonProperty]
		public int Number
		{
			get
			{
				return GetValue<int>();
			}
			set
			{
				SetValue(value);
			}
		}

		[JsonProperty]
		public string SpecialItemName
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		[JsonProperty]
		public string SpecialHeader
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		[JsonProperty]
		public bool IsSpecialItem
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

		[JsonProperty]
		public string ShortDescription
		{
			get => this.GetValue<string>();
			set => this.SetValue(value);
		}
	}
}
