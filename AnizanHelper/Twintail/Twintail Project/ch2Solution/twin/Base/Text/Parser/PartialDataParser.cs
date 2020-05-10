// PartialDataParser.cs

namespace Twin.Text
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	/// 部分的なデータを解析するパーサ
	/// </summary>
	public abstract class PartialDataParser<T>
	{
		protected readonly BbsType bbsType;
		protected readonly Encoding encoding;

		protected MemoryStream memory;
		protected int capacity;
		protected int remainderLength;

		/// <summary>
		/// PartialDataParserクラスのインスタンスを初期化
		/// </summary>
		/// <param name="bbs">解析する掲示板の種類</param>
		/// <param name="enc">テキストのエンコーディング</param>
		/// <param name="type">初期化するクラスの型</param>
		protected PartialDataParser(BbsType bbs, Encoding enc)
		{
			encoding = enc;
			bbsType = bbs;

			memory = new MemoryStream();
			remainderLength = 0;
			capacity = 4096;
		}

		public Encoding Encoding { get { return this.encoding; } }

		/// <summary>
		/// メモリ内に残った余りの長さを取得
		/// </summary>
		public int RemainderLength {
			get { return remainderLength; }
		}

		/// <summary>
		/// バッファを空にする
		/// </summary>
		public virtual void Empty()
		{
			memory = new MemoryStream();
			remainderLength = 0;
		}

		/// <summary>
		/// dataを解析しコレクションに格納
		/// </summary>
		/// <param name="bytes">解析するバイトデータ</param>
		/// <param name="length">dataの長さ</param>
		/// <param name="parsed">解析されたデータの長さ</param>
		/// <returns></returns>
		public virtual T[] Parse(byte[] data, int length, out int parsed)
		{
			if (data == null) {
				throw new ArgumentNullException("data");
			}

			List<T> result = new List<T>();
			int index = 0;

			// 前回の余りデータとdataを結合
			if (memory.Length > 0)
			{			
				memory.Write(data, 0, length);
				data = memory.ToArray();
				length = data.Length;
			}

			// 解析可能な部分の終わりを探す
			int tokenLength;
			int token = GetEndToken(data, 0, length, out tokenLength);

			if (token != -1)
			{
				// 前回の余りデータの末尾に足して１つのデータにする
				index = token + tokenLength;
				
				// 余りの長さを求める
				remainderLength = length - index;

				// 文字列に変換後に解析
				string dataText = encoding.GetString(data, 0, index);
				T[] array = ParseData(dataText);

				if (array != null)
				{
					result.AddRange(array);
				}
				else {
					TwinDll.Output("データの解析に失敗: " + dataText);
				}
			}
			else {
				// 解析可能な部分データがなければすべてを余りデータとして
				// 前回の余りデータに書き足し次回の解析に使用
				remainderLength = length;
				index = 0;
			}

			// 解析できなかった余りデータをメモリに貯める
			memory.Close();
			memory = new MemoryStream(capacity);
			memory.Write(data, index, remainderLength);

			// 実際に解析された長さを取得
			parsed = (length - remainderLength);

			return result.ToArray();
		}

		/// <summary>
		/// データを解析しオブジェクトを作成
		/// </summary>
		/// <param name="lineData"></param>
		/// <returns></returns>
		protected abstract T[] ParseData(string dataText);

		/// <summary>
		/// 最後のトークンの位置を検索
		/// </summary>
		/// <param name="bytes">解析データ</param>
		/// <param name="index">検索開始位置</param>
		/// <param name="length">データの長さ</param>
		/// <param name="tokenLength">トークンが見つかればその長さが格納される</param>
		/// <returns>トークンが見つかった位置</returns>
		protected abstract int GetEndToken(byte[] bytes, int index, int length, out int tokenLength);
	}
}
