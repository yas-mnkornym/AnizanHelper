// NextThreadChecker.cs
// #2.0

namespace Twin.Tools
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Net;
	using Twin.IO;

	using __WordSet = System.Collections.Generic.KeyValuePair<Twin.Tools.NextThreadChecker.WordType, string>;
	using System.Diagnostics;
	using Twin.Text;

	/// <summary>
	/// 次スレ案内機能
	/// </summary>
	public class NextThreadChecker
	{
		private ThreadHeader header;
		private List<ThreadHeader> matchItems;
		private Thread thread;
		private int matchLevel;

		private static readonly Regex alpha = new Regex("\\p{Lu}|\\p{Ll}", RegexOptions.Compiled);
		private static readonly Regex space = new Regex("\\s", RegexOptions.Compiled);
		private static readonly Regex dec = new Regex("\\p{Nd}", RegexOptions.Compiled);
		private static readonly Regex hira = new Regex("\\p{IsHiragana}", RegexOptions.Compiled);
		private static readonly Regex kata = new Regex("\\p{IsKatakana}", RegexOptions.Compiled);
		private static readonly Regex kanji = new Regex("\\p{IsCJKUnifiedIdeographs}", RegexOptions.Compiled);
		private static readonly Regex hankaku = new Regex("[\uFF61-\uFF9F]", RegexOptions.Compiled);

		/// <summary>
		/// 前スレの情報を取得します。
		/// </summary>
		public ThreadHeader Item
		{
			get
			{
				return header;
			}
		}

		/// <summary>
		/// 一致判断単語数を取得または設定します。0以下の場合、自動で判断。
		/// </summary>
		public int MatchLevel
		{
			set
			{
				matchLevel = Math.Max(0, value);
			}
			get
			{
				return matchLevel;
			}
		}

		/// <summary>
		/// パターンに一致した次スレと思われるスレッド情報をすべて格納した List を取得します。
		/// </summary>
		public List<ThreadHeader> MatchItems
		{
			get {
				return matchItems;
			}
		}

		/// <summary>
		/// 次スレをチェック中であれば true、それ以外は false を返します。
		/// </summary>
		public bool IsChecking
		{
			get
			{
				return (thread != null) ? thread.IsAlive : false;
			}
		}

		/// <summary>
		/// MatchLevel プロパティの値を 0 (自動) にした場合のみ、一致精度を高くする場合は true、一つでも一致したスレを含めるなら false。
		/// </summary>
		public bool HighLevelMatching { get; set; }

		/// <summary>
		/// 次スレの検索に成功したときに発生します。
		/// </summary>
		public event ThreadHeaderEventHandler Success;

		/// <summary>
		/// NextThreadChecker
		/// </summary>
		public NextThreadChecker()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			matchItems = new List<ThreadHeader>();
			matchLevel = 0;
			thread = null;
		}

		/// <summary>
		/// 最新のスレッド一覧を取得し、item の次スレと思われるスレッドを検索します。
		/// </summary>
		/// <param name="item">次スレをチェックするスレッド。</param>
		public void Check(ThreadHeader item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (IsChecking)
			{
				throw new InvalidOperationException("チェック中です");
			}

			header = item;
			matchItems.Clear();

			Checking();
		}

		/// <summary>
		/// 非同期でチェックを始める
		/// </summary>
		/// <param name="item"></param>
		public void CheckBegin(ThreadHeader item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (IsChecking)
			{
				throw new InvalidOperationException("チェック中です");
			}

			header = item;
			header.BoardInfo.Bbs = EnsureCurrentBbs(header.BoardInfo.Bbs);
			matchItems.Clear();

			thread = new Thread(Checking);
			thread.Name = "NST_" + item.Key;
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// 常に現行スレを見るようにする
		/// </summary>
		private BbsType EnsureCurrentBbs(BbsType bbs)
		{
			return bbs.Equals(BbsType.X2chAuthenticate) ? BbsType.X2ch : bbs;
		}

		/// <summary>
		/// チェックを終了させたい時に呼ぶ
		/// </summary>
		public void CheckEnd()
		{
			if (IsChecking && thread != null)
				thread.Abort();
		}

		/// <summary>
		/// items を検索し、header の次スレと思われるスレッドをすべて返します。
		/// </summary>
		/// <param name="header"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public List<ThreadHeader> Check(ThreadHeader header,
			List<ThreadHeader> items)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			List<ThreadHeader> result = new List<ThreadHeader>();
			List<__WordSet> wordList = GetWords(header.Subject);

			bool isAutoLevel = (MatchLevel <= 0);
			int currentLevel = isAutoLevel ? 5 : this.MatchLevel;
			int maxLevel = 0;

			foreach (__WordSet s in wordList)
				Console.Write("{0}:{1}, ", s.Key, s.Value);
			Console.WriteLine();

			do
			{
				Console.WriteLine("Level={0}", currentLevel);
				foreach (ThreadHeader item in items)
				{
					int matchCount = currentLevel;

					if (IsMatch(wordList, item, ref matchCount))
					{
						maxLevel = Math.Max(maxLevel, matchCount);

						item.Tag = matchCount;
						result.Add(item);
					}
				}
			}
			while (isAutoLevel && result.Count == 0 && --currentLevel >= 2);

			// 最高レベルを調べて、その半分に満たない一致スレを除く
			int removeLevel = maxLevel / 2;
			result.RemoveAll((s) => (((int)s.Tag) < removeLevel));

			// 一致レベルでソート
			ThreadHeader[] temp = result.ToArray();
			Array.Sort(temp, new LevelComparer());

			result.Clear();
			result.AddRange(temp);

			return result;
		}

		/// <summary>
		/// 次スレチェックスレッド
		/// </summary>
		protected virtual void Checking()
		{
			ThreadListReader listReader = null;
			List<ThreadHeader> tempItems = new List<ThreadHeader>();
			BoardInfo board = header.BoardInfo;

			try
			{
				matchItems.Clear();
				listReader = TypeCreator.CreateThreadListReader(board.Bbs);

				if (listReader.Open(board))
				{
					while (listReader.Read(tempItems) != 0)
						;
					matchItems = Check(header, tempItems);
				}
				OnSuccess(this, new ThreadHeaderEventArgs(matchItems));
			}
			catch (Exception ex)
			{
				TwinDll.Output(ex);
			}
			finally
			{
				if (listReader != null)
					listReader.Close();
			}
		}

		/// <summary>
		/// 次スレかどうかを判断
		/// </summary>
		/// <param name="sourceItem">前スレ情報</param>
		/// <param name="checkItem">判断するスレッドアイテム</param>
		/// <param name="level">一致判断単語数（一致した場合は一致レベルが代入される）</param>
		/// <returns>一致したならtrue</returns>
		protected virtual bool IsMatch(ThreadHeader sourceItem, ThreadHeader checkItem, ref int level)
		{
			return IsMatch(GetWords(sourceItem.Subject), checkItem, ref level);
		}
			
		protected virtual bool IsMatch(List<__WordSet> wordList, ThreadHeader checkItem, ref int level)
		{
			if (checkItem.IsLimitOverThread)
				return false;

			float matchCount = 0;
			string input = checkItem.Subject;
			string backWord = String.Empty;

			foreach (__WordSet ws in wordList)
			{
				WordType type = ws.Key;
				string key = ws.Value;

				// ひらがな1文字は無視
				if (key.Length == 1 && type == WordType.Hira)
					continue;

				if (Regex.IsMatch(input, Regex.Escape(key), RegexOptions.IgnoreCase))
				{
					if (type == WordType.Kanji || type == WordType.Kata || type == WordType.Hankaku || type == WordType.Alpha)
					{
						matchCount += key.Length * 2.0f;
					}
					else
					{
						matchCount += 1;
					}
				}

				// key を直前の単語 backWord と結合し、ひとつのワードとして検索。
				// "BS11 3691" と "BSフジ 1125" が両方とも "BS" と "11" が一致し、同じ一致レベルとされてしまう問題があったたため
				if (backWord != "" && Regex.IsMatch(input, Regex.Escape(backWord + key), RegexOptions.IgnoreCase))
				{
					string longKey = backWord + key;
					matchCount += longKey.Length * 1.5f;
				}
			
				// スレのカウント+1　の数字が存在したら有力候補…？
				if (type == WordType.Decimal)
				{
					int dec;
					if (Int32.TryParse(key, out dec))
					{
						dec += 1;
						if (Regex.IsMatch(input, dec + "|" + HtmlTextUtility.HanToZen(dec.ToString())))
							matchCount += 3;
					}
				}

				backWord = key;
			}
		
			// スレの勢いがあればあるほど
			matchCount += new ThreadHeaderInfo(checkItem).ForceValueDay / 10000;

			if (level - matchCount <= 0)
			{
				level = (int)Math.Round(matchCount, MidpointRounding.AwayFromZero);
				Console.WriteLine("{0} level of {1}", level, input);
				return true;
			}

			return false;
		}

		/// <summary>
		/// アルファベット、数字、漢字、カタカナ、ひらがな各単位ごとの
		/// 単語を切り出して配列に格納
		/// </summary>
		/// <param name="text">単語を切り出す文字列</param>
		/// <returns>単語が格納されたstringクラスの配列</returns>
		public static List<__WordSet> GetWords(string text)
		{
			List<__WordSet> words = new List<KeyValuePair<WordType, string>>();
			WordType wordType = WordType.None;
			int index = 0;

			for (int i = 0; i < text.Length; i++)
			{
				WordType type = WordType.None;
				string ch = text[i].ToString();

				if (alpha.IsMatch(ch))
					type = WordType.Alpha;

				else if (space.IsMatch(ch))
					type = WordType.Space;

				else if (dec.IsMatch(ch))
					type = WordType.Decimal;

				else if (hira.IsMatch(ch))
					type = WordType.Hira;

				else if (kata.IsMatch(ch))
					type = WordType.Kata;

				else if (kanji.IsMatch(ch))
					type = WordType.Kanji;

				else if (hankaku.IsMatch(ch))
					type = WordType.Hankaku;
				
				else if (Char.IsPunctuation(ch[0]) || Char.IsSymbol(ch[0]))
					type = WordType.Symbol;

				if (i == 0)
				{
					wordType = type;
				}
				else if (wordType != type)
				{
					if (wordType != WordType.Space &&
						wordType != WordType.None &&
						wordType != WordType.Symbol)
					{
						string word = text.Substring(index, i - index);
						words.Add(new __WordSet(wordType, word));
					}

					index = i;
					wordType = type;
				}
			}

			if (index < text.Length &&
				wordType != WordType.Space &&
				wordType != WordType.None)
			{
				words.Add(new __WordSet(wordType, text.Substring(index, text.Length - index)));
			}

			return words;
		}

		protected void OnSuccess(object sender, ThreadHeaderEventArgs e)
		{
			if (Success != null)
				Success(sender, e);
		}

		public enum WordType
		{
			None = -1,
			Space = 0,
			Alpha,
			Decimal,
			Hira,
			Kata,
			Kanji,
			Hankaku,
			Symbol,
		}

		private class LevelComparer : IComparer
		{
			public int Compare(object a, object b)
			{
				ThreadHeader itemA = (ThreadHeader)a;
				ThreadHeader itemB = (ThreadHeader)b;

				int levela = (int)itemA.Tag;
				int levelb = (int)itemB.Tag;

				// itemA が itemB より上に表示させるならマイナスの値、itemB の方が上に表示させるならプラスの値を返す…？
				int ret = (levelb - levela);
				if (ret == 0)
				{
					// レベルが同一の場合、レス数とスレの勢いで判定する
					int a_resCount = itemA.ResCount, b_resCount = itemB.ResCount,
						a_no = itemA.No, b_no = itemB.No;
					float a_force = new ThreadHeaderInfo(itemA).ForceValueDay,  b_force = new ThreadHeaderInfo(itemB).ForceValueDay;

					// レス数は多いほうが優先
					int compareValue = 0;
					if (a_resCount != b_resCount)
						compareValue += (a_resCount - b_resCount) < 0 ? 1 : -1;
					// 勢いは多いほうが優先
					if (a_force != b_force)
						compareValue += (a_force - b_force) < 0 ? 1 : -1;
					// スレNOは小さいほうが優先
					if (a_no != b_no)
						compareValue += (a_no - b_no) < 0 ? -1 : 1;

					compareValue += String.Compare(itemA.Subject, itemB.Subject);

					return compareValue;
				}
				else
					return ret;
			}
		}
	}
}
