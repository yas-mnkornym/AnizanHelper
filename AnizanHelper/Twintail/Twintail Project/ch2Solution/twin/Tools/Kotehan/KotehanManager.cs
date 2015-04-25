// KotehanManager.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Collections;
	using CSharpSamples;
	using CSharpSamples.Text.Search;

	/// <summary>
	/// コテハンを管理
	/// </summary>
	public class KotehanManager
	{
		private CSPrivateProfile profile;

		/// <summary>
		/// デフォルトのコテハンを取得または設定
		/// </summary>
		public Kotehan Default {
			set {
				Kotehan kote = (value != null) ? value : new Kotehan();
				profile.SetValue("Default", "Name", kote.Name);
				profile.SetValue("Default", "Email", kote.Email);
				profile.SetValue("Default", "Be", kote.Be);
			}
			get {
				Kotehan kote = new Kotehan();
				kote.Name = profile.GetString("Default", "Name", String.Empty);
				kote.Email = profile.GetString("Default", "Email", String.Empty);
				kote.Be = profile.GetBool("Default", "Be", false);

				return kote;
			}
		}

		/// <summary>
		/// 設定されているコテハンをすべて取得
		/// </summary>
		public Kotehan[] All {
			get {
				ArrayList list = new ArrayList();

				foreach (CSPrivateProfileSection sec in profile.Sections)
				{
					Kotehan kote = new Kotehan(sec["Name"], sec["Email"], Boolean.Parse(sec["Be"]));
					if (!kote.IsEmpty) list.Add(kote);
				}

				return (Kotehan[])list.ToArray(typeof(Kotehan));
			}
		}

		/// <summary>
		/// KotehanManagerクラスのインスタンスを初期化
		/// </summary>
		public KotehanManager()
		{
			profile = new CSPrivateProfile();
		}

		/// <summary>
		/// 指定した板のセクション名を取得
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		private string GetSection(BoardInfo board)
		{
			return board.DomainPath;
		}

		/// <summary>
		/// 指定したスレッドのセクション名を取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		private string GetSection(ThreadHeader header)
		{
			return header.BoardInfo.DomainPath + "#" + header.Key;
		}

		/// <summary>
		/// 指定したセクションのコテハンを取得。存在しなければnullを返す。
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		private Kotehan GetInternal(string key)
		{
			Kotehan kote = new Kotehan(
				profile.GetString(key, "Name", String.Empty),
				profile.GetString(key, "Email", String.Empty),
				profile.GetBool(key, "Be", false));

			return (kote.IsEmpty) ? null : kote;
		}

		/// <summary>
		/// 指定したセクションにコテハンを設定
		/// </summary>
		/// <param name="section"></param>
		/// <param name="kotehan">nullまたは空のコテハンを指定するとセクションを削除</param>
		private void SetInternal(string section, Kotehan kotehan)
		{
			if (kotehan == null || kotehan.IsEmpty)
			{
				profile.Sections.Remove(section);
				return;
			}

			profile.SetValue(section, "Name", kotehan.Name);
			profile.SetValue(section, "Email", kotehan.Email);
			profile.SetValue(section, "Be", kotehan.Be);
		}

		/// <summary>
		/// コテハンに設定されている板のサーバー情報を書き換えます。
		/// </summary>
		/// <param name="oldBoard"></param>
		/// <param name="newBoard"></param>
		public void ServerChange(BoardInfo oldBoard, BoardInfo newBoard)
		{
			ISearchable s = new BmSearch2(oldBoard.DomainPath);

			foreach (CSPrivateProfileSection sec in profile.Sections)
			{
				if (s.Search(sec.Name) >= 0)
				{
					sec.Name.Replace(oldBoard.DomainPath, newBoard.DomainPath);
				}
			}
		}
		
		/// <summary>
		/// 指定したファイルからコテハン情報を読み込む
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public void Load(string filePath)
		{
			profile.RemoveAll();

			if (File.Exists(filePath))
				profile.Read(filePath);
		}

		/// <summary>
		/// 指定したファイルにコテハン情報を保存
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="kotehan"></param>
		public void Save(string filePath)
		{
			profile.Write(filePath);
		}

		/// <summary>
		/// 指定した板にコテハンが設定されているかどうかを判断
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public bool IsExists(BoardInfo board)
		{
			string key = GetSection(board);
			return profile.Sections.ContainsSection(key);
		}

		/// <summary>
		/// 指定したスレッドにコテハンが設定されているかどうかを判断
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public bool IsExists(ThreadHeader header)
		{
			string key = GetSection(header);
			return profile.Sections.ContainsSection(key);
		}

		/// <summary>
		/// 指定した板に設定されているコテハンを取得
		/// </summary>
		/// <param name="board">コテハンを取得する板</param>
		/// <returns>存在すればKotehanクラスのインスタンスを返す。存在しなければデフォルト値</returns>
		public Kotehan Get(BoardInfo board)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			string key = GetSection(board);
			Kotehan kote = GetInternal(key);

			if (kote == null)
				kote = Default;

			return kote;
		}

		/// <summary>
		/// 指定したスレッドに設定されているコテハンを取得
		/// </summary>
		/// <param name="header">スレッド情報</param>
		/// <returns>存在すればKotehanクラスのインスタンスを返す。存在しなければデフォルト値</returns>
		public Kotehan Get(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			string key = GetSection(header);
			Kotehan kote = GetInternal(key);

			if (kote == null)
				kote = Get(header.BoardInfo);

			return kote;
		}

		/// <summary>
		/// 指定した板のコテハンを設定
		/// </summary>
		/// <param name="board">コテハンを設定する板</param>
		/// <param name="kotehan">設定する値が格納されたコテハン</param>
		public void Set(BoardInfo board, Kotehan kotehan)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			string section = GetSection(board);
			SetInternal(section, kotehan);
		}

		/// <summary>
		/// Setクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header">コテハンを設定するスレッド情報</param>
		/// <param name="kotehan">設定する値が格納されたコテハン</param>
		public void Set(ThreadHeader header, Kotehan kotehan)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			string section = GetSection(header);
			SetInternal(section, kotehan);
		}
	}
}
