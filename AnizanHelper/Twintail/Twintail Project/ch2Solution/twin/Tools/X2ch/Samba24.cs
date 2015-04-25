// Samba24.cs

namespace Twin.Tools
{
	using System;
	using System.Collections;
	using System.IO;
	using CSharpSamples;
	using System.Net;
	using System.Text.RegularExpressions;
using System.Diagnostics;

	/// <summary>
	/// Samba24対策に自主規制を行う
	/// </summary>
	public class Samba24
	{
		private Hashtable table;
		private CSPrivateProfile profile;
		private string filePath;

		/// <summary>
		/// 指定したサーバー名の規制秒数を取得
		/// </summary>
		public int this[string server] {
			get {
				return profile.GetInt("samba", server, 0);
			}
		}

		/// <summary>
		/// Samba24クラスのインスタンスを初期化
		/// </summary>
		public Samba24()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			table = new Hashtable();
			profile = new CSPrivateProfile();
			filePath = null;
		}

		/// <summary>
		/// Samba24クラスのインスタンスを初期化
		/// </summary>
		public Samba24(string filePath) : this()
		{
			Load(filePath);
		}

		/// <summary>
		/// samba設定ファイルを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		public void Load(string filePath)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}
			if (File.Exists(filePath))
				profile.Read(filePath);

			this.filePath = filePath;
		}

		/// <summary>
		/// 指定したサーバーのカウンター開始
		/// </summary>
		/// <param name="server"></param>
		public void CountStart(string server)
		{
			// 現在値を設定
			table[server] = Environment.TickCount;
		}

		/// <summary>
		/// 指定したサーバーの規制時間が経過したかどうかを判断
		/// </summary>
		/// <param name="server">チェックするサーバー名</param>
		/// <returns>規制時間を過ぎていたらtrue、規制時間内ならfalse</returns>
		public bool IsElapsed(string server)
		{
			int r;
			return IsElapsed(server, out r);
		}

		/// <summary>
		/// 指定したサーバーの規制時間が経過したかどうかを判断。
		/// カウンタが開始されていなければ、常にtrueを返す。
		/// </summary>
		/// <param name="server">チェックするサーバー名</param>
		/// <param name="result">残り秒数が格納される</param>
		/// <returns>規制時間を過ぎていたらtrue、規制時間内ならfalse</returns>
		public bool IsElapsed(string server, out int result)
		{
			if (table.Contains(server))
			{
				int now = Environment.TickCount;	// 現在値
				int begin = (int)table[server];		// 開始値

				// 経過秒数を計算
				int count = (now - begin) / 1000;

				// 残り秒数を計算
				result = this[server] - count;

				return (count >= this[server]) ? true : false;
			}
			else {
				//throw new ArgumentException(server + "の開始値が存在しません");
				result = 0;
				return true;
			}
		}

		/// <summary>
		/// 指定したサーバーのSambaカウントを修正
		/// </summary>
		/// <param name="server"></param>
		/// <param name="newCount"></param>
		public void Correct(string server, int newCount)
		{
			// テーブルに新しい値を設定して保存
			profile.SetValue("samba", server, newCount);
			
			if (filePath != null)
				profile.Write(filePath);
		}

		/// <summary>
		/// すべてのカウンタをリセット
		/// </summary>
		public void Reset()
		{
			table.Clear();
		}

		/// <summary>
		/// 板のトップページを取得して、最新のsamba値を取得。
		/// </summary>
		/// <param name="bi"></param>
		/// <returns>正しく更新された場合には、最新のsamba24の値を返します。それ以外は -1 を返します。</returns>
		public int Update(BoardInfo bi)
		{
			using (WebClient w = new WebClient())
			{
				string html = w.DownloadString(bi.Url);
				Match m = Regex.Match(html, @"\+Samba24=([0-9]+)", RegexOptions.RightToLeft);
				int newVal;
				if (Int32.TryParse(m.Groups[1].Value, out newVal))
				{
					Correct(bi.Server, newVal);
					Debug.WriteLine("Samba24, Update: " + newVal);
					return newVal;
				}
			}
			return -1;
		}
	}
}
