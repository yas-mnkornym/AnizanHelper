using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComiketSystem.Csv
{
	public class CsvSplitter
	{
		private int currentLine_;
		public string Newline { get; set; }

		public string[][] ToTokenArray()
		{
			var linelistarray = this.lines.ToArray();
			List<string[]> linelist = new List<string[]>(linelistarray.Length);
			foreach (var line in linelistarray)
			{
				linelist.Add(line.ToArray());
			}
			return linelist.ToArray();
		}

		public char Delimiter
		{
			get;
			set;
		}

		private Dictionary<char, char> escapeMap_ = new Dictionary<char, char>();
		public bool UseEscape { get; set; }

		public void AddEscape(char origin, char escaped)
		{
			this.escapeMap_[escaped] = origin;
		}

		public void RemoveEscape(char origin)
		{
			try
			{
				foreach (var key in this.escapeMap_.Keys)
				{
					if (key == origin)
					{
						this.escapeMap_.Remove(key);
						break;
					}
				}
			}
			catch { }
		}

		public void RemoveAllEscapes()
		{
			this.escapeMap_.Clear();
		}

		public int CurrentLine
		{
			get
			{
				return this.currentLine_;
			}
			set
			{
				this.currentLine_ = value;
			}
		}

		public int CurrentIndex { get; set; }
		private List<List<string>> lines = new List<List<string>>();

		public int LineCount
		{
			get
			{
				return this.lines.Count;
			}
		}

		public List<string> Tokens
		{
			get
			{
				return this.lines[this.CurrentLine];
			}
		}

		public CsvSplitter()
		{
			this.Newline = "\n";
			this.UseEscape = false;
			this.Delimiter = ',';
		}

		/// <summary>
		///	直接トークンへ分解
		/// </summary>
		/// <param name="str">Csvテキスト</param>
		public CsvSplitter(string str)
			: this()
		{
			this.FromString(str);
		}

		/// <summary>
		/// 現在の行の、指定したインデックスのトークンを取得する。
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public string GetAt(int i)
		{
			try
			{
				return this.lines[this.CurrentLine][i];
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// 現在の行のトークン数を取得する。
		/// </summary>
		public int TokenCount
		{
			get
			{
				return this.TokenCountAt(this.CurrentLine);
			}
		}

		/// <summary>
		/// 特定の行のトークン数を取得する。
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public int TokenCountAt(int line)
		{
			try
			{
				return this.lines[line].Count;
			}
			catch
			{
				return 0;
			}
		}

		public void Clear()
		{
			this.lines = new List<List<string>>();
			this.CurrentLine = -1;
			this.CurrentIndex = 0;
		}

		public void Parse(string text)
		{
			this.Parse(text.Split(new string[] { this.Newline }, StringSplitOptions.None));
		}

		public void Parse(string[] textlines)
		{
			this.Clear();
			//lines = new List<List<string>>(textlines.Length);
			List<string> line = new List<string>();
			StringBuilder token = new StringBuilder();

			bool inText = false;
			char[] arr = { '\n', '\r' };
			foreach (string str in textlines)
			{
				var strc = str.TrimEnd(arr);
				strc = strc + "\0";
				for (int i = 0; i < strc.Length - 1; i++)
				{
					char c0 = strc[i];
					char c1 = strc[i + 1];

					if (c0 == '"')
					{
						if (inText)
						{
							if (c1 == '"')
							{
								token.Append(c0);
								++i;
							}
							else
							{
								inText = false;
							}
						}
						else
						{
							inText = true;
						}
					}
					else if (c0 == this.Delimiter && !inText)
					{
						line.Add((token.Length > 0) ? token.ToString() : "");
						token = new StringBuilder();
					}
					else if (c0 == '\\' && this.UseEscape)
					{
						if (this.escapeMap_.Values.Contains(c1))
						{
							token.Append(this.escapeMap_[c1]);
							++i;
						}
						else if (c1 == '\\')
						{
							token.Append('\\');
							++i;
						}
						else
						{
							token.Append(c0);
						}
					}
					else
					{
						token.Append(c0);
					}

					// 行末かどうかをチェック
					if (c1 == '\0')
					{
						if (inText)
						{
							throw new Exception("CSVのパース中に無効な文字を検出しました(文字列中にヌル文字があります)");
						}
						else
						{
							line.Add((token.Length > 0) ? token.ToString() : "");
							this.lines.Add(line);
							line = new List<string>();
							token = new StringBuilder();
						}
					}
				}
			}
		}

		/// <summary>
		///	文字列からトークンを追加
		/// </summary>
		/// <param name="str">Csvテキスト</param>
		public void FromString(string str)
		{
			this.lines.Clear();
			//	改行コード統一
			str = str.Replace("\r\n", "\n");
			str = str.Replace("\r", "\n");

			bool stringFlag = false;
			List<string> line = new List<string>(2550);
			List<char> token = new List<char>();

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '"')
				{
					if (stringFlag)
					{
						if (str.Length > i + 1)
						{
							if (str[i + 1] == '"')
							{
								token.Add('"');
								i++;
								continue;
							}
						}
						stringFlag = false;
					}
					else
					{
						stringFlag = true;
					}
				}
				else if (str[i] == this.Delimiter)
				{
					if (stringFlag)
					{
						token.Add(str[i]);
					}
					else
					{
						line.Add(new string(token.ToArray()));
						token.Clear();
					}
				}
				else if (str[i] == '\n')
				{
					if (stringFlag)
					{
						token.Add(str[i]);
					}
					else
					{
						line.Add(new string(token.ToArray()));
						token.Clear();
						this.lines.Add(line);
						line = new List<string>();
						token = new List<char>();
					}
				}
				else
				{
					token.Add(str[i]);
				}
			}
			if (token.Count > 0)
			{
				line.Add(new string(token.ToArray()));
				token.Clear();
			}
			if (line.Count > 0)
			{
				this.lines.Add(line);
			}
			this.CurrentLine = -1;
			this.CurrentIndex = 0;
		}

		/// <summary>
		/// 次の行へ
		/// </summary>
		/// <returns>成功すればtrue</returns>
		public bool ToNextLine()
		{
			if (this.CurrentLine < this.lines.Count - 1)
			{
				this.CurrentLine++;
				this.CurrentIndex = 0;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 前の行へ
		/// </summary>
		/// <returns>成功すればtrue</returns>
		public bool ToPrevLine()
		{
			if (this.CurrentLine > 0)
			{
				this.CurrentLine--;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 次のトークンを取得する。
		/// </summary>
		/// <returns>存在すれば文字列、存在しなければnull</returns>
		public string GetNext()
		{
			if (this.CurrentLine < this.lines.Count && this.CurrentIndex < this.lines[this.CurrentLine].Count)
			{
				this.CurrentIndex++;
				return this.lines[this.CurrentLine][this.CurrentIndex - 1];
			}
			else
			{
				return null;
			}
		}

		/*
		/// <summary>
		/// 指定のインデックスのトークンを取得
		/// </summary>
		/// <param name="index">取得したいインデックス</param>
		/// <returns>存在すれば文字列、でなければnull</returns>
		public string GetAt(int index)
		{
			if (index < lines.Count && index < lines[CurrentLine].Count) {
				return lines[CurrentLine][index];
			}
			else {
				return null;
			}
		}
		 */

		/// <summary>
		///	次のトークンへ
		/// </summary>
		/// <returns>成功すればtrue</returns>
		public bool ToNext()
		{
			if (this.CurrentIndex < this.lines[this.CurrentLine].Count - 1)
			{
				this.CurrentIndex++;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		///	前のトークンへ
		/// </summary>
		/// <returns>成功すればtrue</returns>
		public bool ToPrev()
		{
			if (this.CurrentIndex > 0)
			{
				this.CurrentIndex--;
				return true;
			}
			else
			{
				return false;
			}
		}

		#region GetAt

		public string GetString(int index)
		{
			return this.Tokens[index];
		}

		public bool GetBool(int index)
		{
			var str = this.Tokens[index].ToLower();
			if (str == "true")
			{
				return true;
			}
			else if (str == "false")
			{
				return false;
			}
			else
			{
				int num;
				if (!int.TryParse(str, out num))
				{
					throw new InvalidOperationException(string.Format("Invalid token:{0}, index:{1}", str, index));
				}
				else
				{
					return (num != 0);
				}
			}
		}

		public int GetInt(int index)
		{
			return int.Parse(this.Tokens[index], System.Globalization.NumberStyles.Any);
		}

		public long GetLong(int index)
		{
			return long.Parse(this.Tokens[index], System.Globalization.NumberStyles.Any);
		}

		public float GetFloat(int index)
		{
			return float.Parse(this.Tokens[index], System.Globalization.NumberStyles.Any);
		}

		public double GetDouble(int index)
		{
			return float.Parse(this.Tokens[index], System.Globalization.NumberStyles.Any);
		}

		public bool GetBoolOrDeraulf(int index, bool defValue = default(bool))
		{
			bool result;
			var ret = bool.TryParse(this.Tokens[index], out result);
			return ret ? result : defValue;
		}

		public int GetIntOrDefault(int index, int defValue = default(int))
		{
			int result;
			var ret = int.TryParse(this.Tokens[index], System.Globalization.NumberStyles.Any, null, out result);
			return ret ? result : defValue;
		}

		public long GetLongOrDefault(int index, long defValue = default(int))
		{
			long result = 0;
			var ret = long.TryParse(this.Tokens[index], System.Globalization.NumberStyles.Any, null, out result);
			return ret ? result : defValue;
		}

		public float GetFloatOrDefault(int index, float defValue = default(float))
		{
			float result = 0;
			var ret = float.TryParse(this.Tokens[index], System.Globalization.NumberStyles.Any, null, out result);
			return ret ? result : defValue;
		}

		public double GetDoubleOrDefault(int index, double defValue = default(double))
		{
			double result = 0;
			var ret = double.TryParse(this.Tokens[index], System.Globalization.NumberStyles.Any, null, out result);
			return ret ? result : defValue;
		}

		#endregion GetAt
	}
}
