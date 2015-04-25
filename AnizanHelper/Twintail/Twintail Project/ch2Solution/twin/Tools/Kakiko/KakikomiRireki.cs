// KakikomiRireki.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Text;
	using Twin.Util;

	/// <summary>
	/// 書き込み履歴を作成・管理
	/// </summary>
	public class KakikomiRireki
	{
		private string baseDir;
		
		/// <summary>
		/// KakikomiRirekiクラスのインスタンスを初期化
		/// </summary>
		/// <param name="dir">書き込み履歴保存フォルダ</param>
		public KakikomiRireki(string dir)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			baseDir = dir;
		}

		/// <summary>
		/// 指定した板の書き込み履歴ファイルのパスを取得
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public string GetKomiPath(BoardInfo board)
		{
			return Path.Combine(baseDir, board.Name + ".txt");
		}

		/// <summary>
		/// 指定した板の書き込み履歴を読み込む
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public string Load(BoardInfo board)
		{
			string filePath = GetKomiPath(board);
			return FileUtility.ReadToEnd(filePath);
		}

		/// <summary>
		/// 指定した板の書き込み履歴に追加
		/// </summary>
		/// <param name="board"></param>
		/// <param name="res"></param>
		public void Append(ThreadHeader header, WroteRes res)
		{
			try {
				string filePath = GetKomiPath(header.BoardInfo);

				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("Date: {0}\r\nSubject: {1}\r\nURL: {2}\r\nFrom: {3}\r\nEmail: {4}\r\n\r\n{5}\r\n------------------------\r\n",
					res.Date.ToString(), header.Subject, header.Url, res.From, res.Email, res.Message);

				FileUtility.Write(filePath, sb.ToString(), true);
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
			}
		}

		/// <summary>
		/// 指定した板の書き込み履歴を削除
		/// </summary>
		/// <param name="board"></param>
		public void Delete(BoardInfo board)
		{
			string filePath = GetKomiPath(board);
			File.Delete(filePath);
		}

		/// <summary>
		/// 指定した板の書き込み履歴が存在するかどうかを判断
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public bool IsExists(BoardInfo board)
		{
			string filePath = GetKomiPath(board);
			return File.Exists(filePath);
		}
	}
}
