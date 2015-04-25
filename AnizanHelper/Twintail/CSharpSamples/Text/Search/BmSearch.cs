
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSamples.Text.Search
{
	/// <summary>
	/// BM法による文字列照合アルゴリズム。大量のメモリを使用するのでだめぽ…。
	/// (参考URL: http://www2.starcat.ne.jp/~fussy/algo/algo7-4.htm)
	/// </summary>
	[Obsolete]
	public class BmSearch : ISearchable
	{
		private readonly int[] skip = new int[Char.MaxValue + 1];
		private readonly string pattern;

		/// <summary>
		/// 検索パターンを取得
		/// </summary>
		public string Pattern
		{
			get
			{
				return pattern;
			}
		}

		/// <summary>
		/// BmSearchクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public BmSearch(string key)
		{
			pattern = key;
			makeTable(key);
		}

		/// <summary>
		/// 移動量テーブルを作成
		/// </summary>
		/// <param name="patt"></param>
		private void makeTable(string key)
		{
			int len = key.Length;

			for (int i = 0; i < skip.Length; i++)
				skip[i] = len;

			int idx = 0;
			while (len > 0)
				skip[key[idx++]] = --len;
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Search(string input)
		{
			return Search(input, 0);
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public int Search(string input, int index)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (index < 0 || index >= input.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			int patlen = pattern.Length - 1;
			int endPos = (input.Length - index) - patlen;

			while (index < endPos)
			{
				int i;
				for (i = patlen; i >= 0; i--)
					if (input[index + i] != pattern[i])
						break;

				// すべて一致
				if (i < 0)
					return index;

				// テーブルから移動量を求める(負数なら移動量は2)	
				int move = skip[input[index + i]] - (patlen - i);
				index += (move > 0) ? move : 2;
			}

			return index;
		}
	}

	/// <summary>
	/// BM法による文字列照合アルゴリズム。BmSearchクラスとは違い、バイト単位で比較を行う。
	/// (参考URL: http://www2.starcat.ne.jp/~fussy/algo/algo7-4.htm)
	/// </summary>
	public class BmSearch2 : ISearchable
	{
		private readonly int[] skip = new int[Byte.MaxValue + 1];
		private readonly byte[] pattern;
		private readonly string patternS;

		/// <summary>
		/// 検索パターンを取得
		/// </summary>
		public string Pattern
		{
			get
			{
				return patternS;
			}
		}

		/// <summary>
		/// BmSearch2クラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public BmSearch2(string key)
		{
			patternS = key;
			pattern = Encoding.Unicode.GetBytes(key);
			makeTable(pattern);
		}

		/// <summary>
		/// 移動量テーブルを作成
		/// </summary>
		/// <param name="key"></param>
		private void makeTable(byte[] key)
		{
			int len = key.Length;

			for (int i = 0; i < skip.Length; i++)
				skip[i] = len;

			int idx = 0;
			while (len > 0)
				skip[key[idx++]] = --len;
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Search(string input)
		{
			return Search(input, 0);
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index">検索開始インデックス</param>
		/// <returns></returns>
		public unsafe int Search(string input, int index)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (input.Length == 0)
			{
				return -1;
			}
			if (index < 0 || index >= input.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			int length = Encoding.Unicode.GetByteCount(input);
			int keylen = pattern.Length - 1;

			fixed (char* lpsz = input)
			{
				byte* st = (byte*)lpsz;
				byte* p = (byte*)(lpsz + index);
				byte* ed = st + (length - keylen);

				while (p < ed)
				{
					int i;
					for (i = keylen; i >= 0; i--)
						if (p[i] != pattern[i])
							break;

					// すべて一致
					if (i < 0)
						return (int)(p - st) / 2;

					// テーブルから移動量を求める(負数なら2文字分移動)	
					int move = skip[p[i]] - (keylen - i);
					p += (move > 0) ? move : 4;
				}
			}

			return -1;
		}
	}
}
