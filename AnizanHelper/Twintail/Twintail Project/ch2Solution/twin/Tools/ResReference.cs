// ResReference.cs

namespace Twin.Tools
{
	using System;
	using System.Text.RegularExpressions;
	using System.Text;
	using System.Collections;
	using Twin.Text;

	/// <summary>
	/// レス参照に関連した機能を提供
	/// </summary>
	public class ResReference
	{
		/// <summary>
		/// 参照アンカー >>XX を検索する正規表現
		/// </summary>
		public static readonly Regex RefAnchor =
			new Regex(@"(＞|&gt;)(?<num>[,\d\-\+]+)", RegexOptions.Compiled);

		/// <summary>
		/// レス参照の番号(例: 1,2,3,4-5,6+7 形式)を検索するための正規表現
		/// </summary>
		public static readonly Regex RefRegex =
			new Regex(@"(?<num>[,\d\-\+]+)n?$", RegexOptions.Compiled);

		public static readonly Regex ParseRegex =
			new Regex(@"(?<num>\d+\-?\d*)$", RegexOptions.Compiled);

		/// <summary>
		/// text内に含まれているレス参照の番号をすべて数値配列に変換
		/// (例: http://.../10-20 → 10,11,12...20)
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static int[] GetArray(string text)
		{
			if (text == null) {
				throw new ArgumentNullException("text");
			}

			if (Regex.IsMatch(text, @"/\d{5,}/?$") || Regex.IsMatch(text, "(=|l)\\d+$"))// "/50l" などは無視する
				return (new int[0]);

			ArrayList list = new ArrayList();

			Match m = RefRegex.Match(
				HtmlTextUtility.ZenToHan(text));

			if (m.Success)
			{
				string[] numbers = m.Groups["num"].Value.Split(",+".ToCharArray());

				foreach (string num in numbers)
				{
					string[] array = num.Split('-');
					if (array.Length == 2)
					{
						int st=0, ed=0;

						// 数字かどうかをチェック
						if (Int32.TryParse(array[0], out st))
						{
							// "100-" (100番目以降) という数字の場合、array[1] には空文字列が格納される。
							// この形式の場合は 100番目から最後のレス(1001番目)までを含めるようにする
							if (array[1] == String.Empty)
								ed = 1001;
							else
								Int32.TryParse(array[1], out ed);

							if (st >= 1 && (ed - st) <= 1000)
							{
								for (int i = st; i <= ed; i++)
									list.Add(i);
							}
						}
					}
					else if (array.Length == 1)
					{
						if (HtmlTextUtility.IsDigit(array[0]))
						{
							int n;
							if (Int32.TryParse(array[0], out n))
								list.Add(n);
						}
					}
				}
			}
			return (int[])list.ToArray(typeof(int));
		}
	}
}
