using System;
using System.Collections.Generic;
using System.Text;

namespace ComiketSystem.Csv
{
	public class CsvMaker
	{
		private List<List<string>> lines = new List<List<string>>();
		public int CurrentLine { get; set; }
		public string Newline { get; set; }

		public CsvMaker()
		{
			this.UseEscape = false;
			this.Delimiter = ',';
			this.Newline = "\n";
			this.lines.Add(new List<string>());
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
			this.escapeMap_[origin] = escaped;
		}

		public void RemoveEscape(char origin)
		{
			try
			{
				this.escapeMap_.Remove(origin);
			}
			catch { }
		}

		public void RemoveAllEscapes()
		{
			this.escapeMap_.Clear();
		}

		/// <summary>
		/// Csv文字列を取得
		/// </summary>
		/// <returns>Csv文字列</returns>
		public string GetCsvString()
		{
			StringBuilder sb = new StringBuilder();

			bool firstline = true;
			foreach (List<string> line in this.lines)
			{
				if (firstline) { firstline = false; }
				else { sb.Append(this.Newline); }
				bool first = true;
				foreach (string orgtoken in line)
				{
					var token = orgtoken;
					if (this.UseEscape)
					{
						foreach (var pair in this.escapeMap_)
						{
							token = token.Replace(pair.Key.ToString(), "\\" + pair.Value);
						}
					}

					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append(this.Delimiter);
					}

					bool flag = false;
					for (int i = 0; i < token.Length; i++)
					{
						if (token[i] == '"' || token[i] == this.Delimiter || token[i] == '\n')
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						sb.Append('"');
						sb.Append(token.Replace("\"", "\"\""));
						sb.Append('"');
					}
					else
					{
						sb.Append(token);
					}
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// トークンを全て削除
		/// </summary>
		public void Clear()
		{
			this.lines.Clear();
			this.lines.Add(new List<string>());
		}

		public void AddToken<T>(T value, bool emptyOnNull = true, string nullText = "null")
		{
			string str = "";
			try
			{
				if (value != null)
				{
					str = value.ToString();
				}
				else if (!emptyOnNull)
				{
					if (nullText == null) { throw new ArgumentNullException("nullText"); }
					str = nullText;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("値の文字列化に失敗しました。", ex);
			}

			try
			{
				if (this.lines.Count > this.CurrentLine && this.lines[this.CurrentLine] != null)
				{
					if (str != null)
					{
						this.lines[this.CurrentLine].Add(str);
					}
					else
					{
						this.lines[this.CurrentLine].Add("");
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("トークンの追加処理に失敗しました。", ex);
			}
		}

		/*
		/// <summary>
		/// 現在の行にトークンを追加
		/// </summary>
		/// <param name="str">追加する文字列</param>
		/// <returns>成功すればtrue</returns>
		public bool AddToken(string str)
		{
			if (lines.Count > CurrentLine && lines[CurrentLine] != null) {
				if (str != null) {
					lines[CurrentLine].Add(str);
				}
				else {
					lines[CurrentLine].Add("");
				}
				return true;
			}
			return false;
		}

		public bool AddToken(int value)
		{
			return AddToken(value.ToString());
		}

		public bool AddToken(char value)
		{
			return AddToken(value.ToString());
		}

		public bool AddToken(float value)
		{
			return AddToken(value.ToString());
		}

		public bool AddToken(double value)
		{
			return AddToken(value.ToString());
		}

		public bool AddToken(bool value)
		{
			return AddToken(value ? "true" : "false");
		}
		 * */

		public void AddTokenOrEmpty<T>(T value, T emptyValue)
		{
			bool equals = false;
			if (value == null)
			{
				equals = (emptyValue == null);
			}
			else
			{
				equals = value.Equals(emptyValue);
			}
			if (equals)
			{
				this.AddToken("");
			}
			else
			{
				this.AddToken(value.ToString());
			}
		}

		public bool DeleteToken(int index)
		{
			if (this.lines.Count > this.CurrentLine && this.lines[this.CurrentLine] != null)
			{
				if (this.lines[this.CurrentLine].Count > index)
				{
					this.lines[this.CurrentLine].RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		public void ToNextLine()
		{
			this.lines.Add(new List<string>());
			this.CurrentLine++;
		}

		public override string ToString()
		{
			return this.GetCsvString();
		}
	}
}
