namespace CSharpSamples.Text.Search
{
	/// <summary>
	/// ラビン-カープによる文字列照合アルゴリズム。これなんか遅いので、どこかに書き間違いがあるかも。
	/// (参考URL: http://alfin.mine.utsunomiya-u.ac.jp/~niy/algo/r/rkMatch.html)
	/// </summary>
	public class RkSearch : ISearchable
	{
		public const ulong Q = 33554393L; // prime number
		public const int LOG2D = 16; // 2^16 = 65536

		private readonly string pattern;
		private ulong dm;
		private ulong hash;

		/// <summary>
		/// 検索パターンを取得
		/// </summary>
		public string Pattern
		{
			get
			{
				return this.pattern;
			}
		}

		/// <summary>
		/// RkSearchクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public RkSearch(string key)
		{
			this.pattern = key;
			this.hash = this.makeHash(key, out this.dm);
		}

		/// <summary>
		/// ハッシュ値のテーブルを作成
		/// </summary>
		/// <param name="key"></param>
		/// <param name="dm"></param>
		/// <returns></returns>
		private ulong makeHash(string key, out ulong dm)
		{
			ulong h1;
			int i;

			for (i = 1, dm = 1; i < key.Length; i++)
			{
				dm = (dm << LOG2D) % Q;
			}

			for (i = 0, h1 = 0; i < key.Length; i++)
			{
				h1 = ((h1 << LOG2D) + key[i]) % Q;
			}

			return h1;
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Search(string input)
		{
			return this.Search(input, 0);
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public int Search(string input, int index)
		{
			ulong h2 = 0;

			if (this.pattern.Length == 0)
			{
				return 0;
			}

			if (input.Length < this.pattern.Length)
			{
				return -1;
			}

			for (int i = 0; i < this.pattern.Length; i++)
			{
				h2 = ((h2 << LOG2D) + input[index + i]) % Q;
			}

			int endPos = index + ((input.Length - index) - this.pattern.Length) + 1;

			while (index != endPos)
			{
				if (h2 == this.hash)
				{
					return index;
				}

				if (index + this.pattern.Length >= input.Length)
				{
					break;
				}

				h2 = (h2 + (Q << LOG2D) - input[index] * this.dm) % Q;
				h2 = ((h2 << LOG2D) + input[index + this.pattern.Length]) % Q;
				index++;
			}
			return -1;
		}

	}
}
