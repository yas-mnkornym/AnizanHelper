
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSamples.Text.Search
{
	/// <summary>
	/// Knuth-Morris-Prattによる文字列照合アルゴリズム。
	/// (参考URL: http://alfin.mine.utsunomiya-u.ac.jp/~niy/algo/k/kmpMatch.html)
	/// </summary>
	public class KmpSearch : ISearchable
	{
		private readonly int[] shift;
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
		/// KmpSearchクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public KmpSearch(string key)
		{
			shift = makeTable(key);
			pattern = key;
		}

		/// <summary>
		/// 移動量テーブルを作成
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private int[] makeTable(string key)
		{
			int[] table = new int[key.Length];
			int p = 0, t = 0;

			while (++t != key.Length)
			{
				table[t] = p;
				if (key[t] == key[p])
					p++;
			}
			return table;
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
			if (input.Length < pattern.Length)
				return -1;

			int endPos = input.Length;
			int patEnd = pattern.Length;
			int patIdx = 0;

			while (index != endPos)
			{
				if (input[index++] == pattern[patIdx])
					patIdx++;

				else if (patIdx != 0)
				{
					patIdx = shift[patIdx];
					index--;
				}
				if (patIdx == patEnd)
					return (index - pattern.Length);
			}
			return -1;
		}
	}
}
