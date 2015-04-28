using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComiketSystem.Csv
{
	public class CsvMaker
	{
		List<List<string>> lines = new List<List<string>>();
		public int CurrentLine { get; set; }
		public string Newline { get; set; }

		public CsvMaker()
		{
			UseEscape = false;
			Delimiter = ',';
			Newline = "\n";
			lines.Add(new List<string>());
		}

		public char Delimiter
		{
			get;
			set;
		}

		Dictionary<char, char> escapeMap_ = new Dictionary<char, char>();
		public bool UseEscape { get; set; }
		public void AddEscape(char origin, char escaped)
		{
			escapeMap_[origin] = escaped;
		}

		public void RemoveEscape(char origin)
		{
			try {
				escapeMap_.Remove(origin);
			}
			catch { }
		}

		public void RemoveAllEscapes()
		{
			escapeMap_.Clear();
		}

		/// <summary>
		/// Csv文字列を取得
		/// </summary>
		/// <returns>Csv文字列</returns>
		public string GetCsvString()
		{
			StringBuilder sb = new StringBuilder();

			bool firstline = true;
			foreach (List<string> line in lines) {
				if (firstline) { firstline = false; }
				else { sb.Append(Newline); }
				bool first = true;
				foreach (string orgtoken in line) {
					var token = orgtoken;
					if (UseEscape) {
						foreach (var pair in escapeMap_) {
							token = token.Replace(pair.Key.ToString(), "\\" + pair.Value);
						}
					}

					if (first) {
						first = false;
					}
					else {
						sb.Append(Delimiter);
					}

					bool flag = false;
					for (int i = 0; i < token.Length; i++) {
						if (token[i] == '"' || token[i] == Delimiter || token[i] == '\n') {
							flag = true;
							break;
						}
					}
					if (flag) {
						sb.Append('"');
						sb.Append(token.Replace("\"", "\"\""));
						sb.Append('"');
					}
					else {
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
			lines.Clear();
			lines.Add(new List<string>());
		}


		public void AddToken<T>(T value, bool emptyOnNull = true, string nullText = "null")
		{
			string str = "";
			try {
				if (value != null) {
					str = value.ToString();
				}
				else if (!emptyOnNull) {
					if (nullText == null) { throw new ArgumentNullException("nullText"); }
					str = nullText;
				}
			}
			catch (Exception ex) {
				throw new Exception("値の文字列化に失敗しました。", ex);
			}

			try {
				if (lines.Count > CurrentLine && lines[CurrentLine] != null) {
					if (str != null) {
						lines[CurrentLine].Add(str);
					}
					else {
						lines[CurrentLine].Add("");
					}
				}
			}
			catch (Exception ex) {
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
			if (value == null) {
				equals = (emptyValue == null);
			}
			else {
				equals = value.Equals(emptyValue);
			}
			if(equals){
				AddToken("");
			}
			else {
				AddToken(value.ToString());
			}
		}

		public bool DeleteToken(int index)
		{
			if (lines.Count > CurrentLine && lines[CurrentLine] != null) {
				if (lines[CurrentLine].Count > index) {
					lines[CurrentLine].RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		public void ToNextLine()
		{
			lines.Add(new List<string>());
			CurrentLine++;
		}

		public override string ToString()
		{
			return GetCsvString();
		}
	}
}
