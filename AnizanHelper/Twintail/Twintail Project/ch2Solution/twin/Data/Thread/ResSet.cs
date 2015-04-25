// ResSet.cs

namespace Twin
{
	using System;
	using System.Text.RegularExpressions;
	using System.Runtime.Serialization;
	using Twin.Text;
	using Twin.Tools;
	using System.Collections.Generic;
	using System.Windows.Forms;

	/// <summary>
	/// １つのレスを表す構造体
	/// </summary>
	public struct ResSet
	{
		public static readonly ResSet Empty =
			new ResSet(0, String.Empty, String.Empty, String.Empty, String.Empty);

		/// <summary>
		/// IDを検索するための正規表現
		/// </summary>
		private static readonly Regex IDRegex =
			new Regex(@"(ID:(?<id>[^\s]+))|(\[(?<id>[^\\]]+)\])", RegexOptions.Compiled);
			//new Regex(@"(ID:(?<id>[^\s●]+))|(\[(?<id>[^\\]]+)\])", RegexOptions.Compiled);

		private static readonly Regex BERegex =
			new Regex(@"BE:(?<id>\d+)\-(?<rank>.+)", RegexOptions.Compiled);

		private static readonly Regex HostRegex =
			new Regex(@"<font size=1>\[\s(?<host>[^\s]+)\s]</font>", RegexOptions.Compiled);

		private static readonly Regex Host2ch =
			new Regex(@"HOST:.+|発信元:[^\s]+", RegexOptions.Compiled);

		private int index;
		private string name;
		private string email;
		private string id;
		private string body;
		private string dateString;
		private bool bookmark;
		private bool isNew;
		private bool visible;

		private string host;

		private LinkCollection links;
		private int[] refidx;

		private bool abone;
		private object tag;

		/// <summary>
		/// レス番号を取得または設定
		/// </summary>
		public int Index
		{
			set
			{
				index = value;
			}
			get
			{
				return index;
			}
		}
		
		/// <summary>
		/// 投稿者の名前を取得または設定
		/// </summary>
		public string Name
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Name");
				}
				name = value;
			}
			get
			{
				return name;
			}
		}

		/// <summary>
		/// E-mailを取得または設定
		/// </summary>
		public string Email
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Email");
				}
				email = value;
			}
			get
			{
				return email;
			}
		}

		/// <summary>
		/// IDを取得または設定します。
		/// </summary>
		public string ID
		{
			set
			{
				id = value;
			}
			get
			{
				if (id == null)
				{
					Match m = IDRegex.Match(dateString);
					if (m.Success)
						id = m.Groups["id"].Value;
					else
					{
						id = String.Empty;
					}
				}

				return id;
			}
		}

		public string Be
		{
			get
			{
				Match m = BERegex.Match(dateString);
				return m.Success ? m.Value : String.Empty;
			}
		}

		public string BeLink
		{
			get
			{
				return BERegex.Replace(Be,
					"<a href=\"http://be.2ch.net/test/p.php?i=${id}\" target=\"_blank\">?${rank}</a>");
			}
		}

		/// <summary>
		/// レスの投稿者のホスト名を取得または設定します。
		/// </summary>
		public string Host
		{
			get
			{
				if (host == null)
				{
					Match m = HostRegex.Match(dateString);
					if (m.Success)
					{
						host = m.Groups["host"].Value;
					}
					else
					{
						m = Host2ch.Match(dateString);
						if (m.Success)
							host = m.Groups[0].Value;
					}
				}
				return host;
			}
		}

		/// <summary>
		/// 本分を取得または設定
		/// </summary>
		public string Body
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Body");
				}
				body = value;
			}
			get
			{
				return body;
			}
		}

		/// <summary>
		/// 日付を文字列のまま取得または設定
		/// </summary>
		public string DateString
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("DateString");
				}
				dateString = value;
			}
			get
			{
				return dateString;
			}
		}

		/// <summary>
		/// 新着レスかどうかを取得または設定
		/// </summary>
		public bool IsNew
		{
			set
			{
				if (isNew != value)
					isNew = value;
			}
			get
			{
				return isNew;
			}
		}


		private bool serverAboned;
		/// <summary>
		/// このレスがサーバー側であぼーんされたレスかどうかを示す値を取得または設定します。
		/// </summary>
		public bool IsServerAboned
		{
			get
			{
				return serverAboned;
			}
			set
			{
				serverAboned = value;
			}
		}
	
		/// <summary>
		/// このレスにブックマークを設定または取得
		/// </summary>
		public bool Bookmark
		{
			set
			{
				if (bookmark != value)
					bookmark = value;
			}
			get
			{
				return bookmark;
			}
		}

		/// <summary>
		/// このレスを表示するかどうかを取得または設定
		/// </summary>
		public bool Visible
		{
			set
			{
				if (visible != value)
					visible = value;
			}
			get
			{
				return visible;
			}
		}

		/// <summary>
		/// 本分に含まれているリンクコレクションを取得
		/// </summary>
		public LinkCollection Links
		{
			get
			{
				if (links == null)
					links = CreateLinks();
				return links;
			}
		}

		/// <summary>
		/// このレスが参照しているレス番号の配列を取得
		/// </summary>
		public int[] RefIndices
		{
			get
			{
				if (refidx == null)
					refidx = Createrefidx();
				return refidx;
			}
		}

		/// <summary>
		/// 逆参照されているかどうかを示す値を取得します。
		/// </summary>
		public bool IsBackReferenced
		{
			get
			{
				return BackReferencedCount > 0;
			}
		}

		/// <summary>
		/// 逆参照されている回数を取得または設定します。
		/// </summary>
		public int BackReferencedCount
		{
			get
			{
				return BackReferencedList.Count;
			}
		}

		public List<int> BackReferencedList { get; private set; }

		private ABoneType aboneTyoe;

		public ABoneType ABoneType
		{
			get
			{
				return aboneTyoe;
			}
			set
			{
				aboneTyoe = value;
			}
		}
	
		/// <summary>
		/// このレスがあぼーんレスかどうかを取得または設定
		/// </summary>
		public bool IsABone
		{
			set
			{
				abone = value;
			}
			get
			{
				return abone;
			}
		}

		/// <summary>
		/// タグを取得または設定
		/// </summary>
		public object Tag
		{
			set
			{
				tag = value;
			}
			get
			{
				return tag;
			}
		}

		/// <summary>
		/// あぼーん用のレスを取得
		/// </summary>
		public static readonly ResSet ABoneResSet =
			ResSet.ABone(ResSet.Empty, false, ABoneType.Tomei, "");

		/// <summary>
		/// ResSetクラスのインスタンスを初期化
		/// </summary>
		/// <param name="index">レス番号</param>
		/// <param name="name">投稿者の名前</param>
		/// <param name="email">投稿者のE-mail</param>
		/// <param name="dateString">投稿日</param>
		/// <param name="body">本分</param>
		public ResSet(int index, string name,
			string email, string dateString, string body) : this()
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (email == null)
			{
				throw new ArgumentNullException("email");
			}
			if (dateString == null)
			{
				throw new ArgumentNullException("dateString");
			}
			if (body == null)
			{
				throw new ArgumentNullException("body");
			}

			this.index = index;
			this.name = name;
			this.email = email;
			this.body = body;
			this.dateString = dateString;
			this.host = null;
			this.id = null;
			this.tag = null;
			this.isNew = true;
			this.abone = false;
			this.visible = true;
			this.bookmark = false;
			this.serverAboned = false;
			this.links = null;
			this.refidx = null;
			this.aboneTyoe = ABoneType.NG;
			this.BackReferencedList = new List<int>();
		}

		/// <summary>
		/// 指定したレスをあぼーん
		/// </summary>
		/// <param name="resSet"></param>
		/// <param name="abone"></param>
		/// <returns></returns>
		public static ResSet ABone(ResSet resSet, bool visible, ABoneType type, string description)
		{
			resSet.IsABone = true;
			resSet.Visible = visible;

			if (type == ABoneType.Tomei)
			{
				resSet.visible = false;
				resSet.Name = resSet.Email = resSet.DateString = resSet.Body = "透明あぼーん";
			}
			else if (type == ABoneType.Normal)
			{
				resSet.visible = true;
				resSet.Name = resSet.Email = resSet.DateString = resSet.Body = "あぼーん";
			}
			else if (type == ABoneType.NG)
			{
				resSet.Name = resSet.Email = resSet.DateString = resSet.Body = "<i>NGあぼーん</i>";
			}
			else if (type == ABoneType.Chain)
			{
				resSet.Body = "連鎖あぼーん";
			}
			else if (type == ABoneType.Syria)
			{
				resSet.Body = "ｼﾘｱ語あぼーん";
				resSet.Name = resSet.Email = "<i>あぼーん</i>";
			}
			else if (type == ABoneType.NGBody)
			{
				resSet.Body = "本文あぼーん";
			}
			else if (type == ABoneType.NGID)
			{
				resSet.Body = "NGIDあぼーん";
			}
			else if (type == ABoneType.NGMail)
			{
				resSet.Body = "NGMailあぼーん";
			}
			else if (type == ABoneType.NGName)
			{
				resSet.Body = "NGNameあぼーん";
			}

			if (!String.IsNullOrEmpty(description))
				resSet.Body += ":" + description;

			resSet.Body = "<i>" + resSet.Body + "</i>";

			return resSet;
		}

		/// <summary>
		/// 本分に含まれている文字列からリンクコレクションを作成
		/// </summary>
		/// <returns></returns>
		private LinkCollection CreateLinks()
		{
			MatchCollection matches = HtmlTextUtility.LinkRegex2.Matches(body);
			LinkCollection links = new LinkCollection();

			foreach (Match m in matches)
			{
				string link = m.Value;

				// h抜きであれば修正
				if (HtmlTextUtility.IsShortHttpUrl.IsMatch(link))
					link = "http://" + m.Groups["url"].Value;

				links.Add(link);
			}

			return links;
		}

		/// <summary>
		/// このレスが参照しているレス番号の配列を作成
		/// </summary>
		/// <returns></returns>
		private int[] Createrefidx()
		{
			List<int> indices = new List<int>();

			foreach (Match m in ResReference.RefAnchor.Matches(body))
			{
				string text = m.Groups["num"].Value;
				int[] indexArray = ResReference.GetArray(text);

				indices.AddRange(indexArray);
			}

			return indices.ToArray();
		}

		public void AddBackReferenced(int index)
		{
			if (!BackReferencedList.Contains(index))
				BackReferencedList.Add(index);
		}

		/// <summary>
		/// 指定したスキンを使用して文字列形式に変換
		/// </summary>
		/// <param name="skin"></param>
		/// <returns></returns>
		public string ToString(ThreadSkinBase skin)
		{
			ResSet res = this;
			res.visible = true;

			return skin.Convert(this);
		}

		/// <summary>
		/// 指定した要素を文字列に変換
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public string ToString(ResSetElement element)
		{
			string obj = String.Empty;

			switch (element)
			{
			case ResSetElement.Name:
				obj = Name;
				break;
			case ResSetElement.Email:
				obj = Email;
				break;
			case ResSetElement.Body:
				obj = Body;
				break;
			case ResSetElement.ID:
				obj = ID;
				break;
			case ResSetElement.DateString:
				obj = DateString;
				break;
			case ResSetElement.All:
				obj = Name + " " + Email + " " + DateString + " " + Body;
				break;
			}
			return obj;
		}
	}

	public enum ABoneType
	{
		Normal,
		Tomei,
		NG,
		NGName,
		NGMail,
		NGID,
		NGBody,
		Syria,
		Chain,
	}
}
