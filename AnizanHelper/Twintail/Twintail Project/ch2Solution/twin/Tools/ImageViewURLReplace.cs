using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Twin.Tools
{
	public class ImageViewUrlReplace
	{
		private string fileName;
		public string FileName
		{
			get
			{
				return fileName;
			}
		}

		private List<ImageViewUrlItem> list = new List<ImageViewUrlItem>();
		public List<ImageViewUrlItem> Items
		{
			get
			{
				return list;
			}
		}
	
		public ImageViewUrlReplace(string fileName)
		{
			Load(fileName);
		}

		/// <summary>
		/// ImageViewURLReplace.dat を .NET の Regex用正規表現に直す
		/// </summary>
		/// <param name="pattern"></param>
		/// <returns></returns>
		private string CorrectRegex(string pattern)
		{
			pattern = Regex.Replace(pattern, @"$\d", @"$\{${1}}");
			pattern = Regex.Replace(pattern, "$&", @"$\{0}");

			return pattern;
		}

		protected virtual void Load(string fileName)
		{
			this.fileName = fileName;


			// sample
			//  元のURL(正規表現) タブ文字(\t) 置換先URL(正規表現) タブ文字(\t) 置換先URLに渡すリファラー
			// "http://www.sage.com/\thttp://www.age.com/\thttp://www.age.com/index.html"

			list.Clear();

			string text = String.Empty;

			if (!File.Exists(fileName))
				return;

			using (StreamReader sr = new StreamReader(fileName, TwinDll.DefaultEncoding))
			{
				text = sr.ReadToEnd();
			}

			foreach (string line in Regex.Split(text, "\r\n|\r|\n"))
			{
				string[] elements = line.Split('\t');

				if (elements.Length >= 2)
				{
					string key = CorrectRegex(elements[0]);
					string repl = CorrectRegex(elements[1]);
					string refe = elements.Length >= 3 ? CorrectRegex(elements[2]) : String.Empty;

					list.Add(new ImageViewUrlItem(key, repl, refe));
				}
			}
		}

		public void Refresh()
		{
			list.Clear();
			Load(fileName);
		}

		public bool Replace(ref string url, out string referer)
		{
			foreach (ImageViewUrlItem item in list)
			{
				if (item.Regex.IsMatch(url))
				{
					referer = item.Regex.Replace(url, item.Referer);
					url = item.Regex.Replace(url, item.Replacement);

					return true;
				}
			}

			referer = String.Empty;

			return false;
		}
	}

	public class ImageViewUrlItem
	{
		private Regex regex;
		/// <summary>
		/// 
		/// </summary>
		public Regex Regex
		{
			get
			{
				return regex;
			}
		}
	
	

		private string replacement;
		/// <summary>
		/// 
		/// </summary>
		public string Replacement
		{
			get
			{
				return replacement;
			}
		}


		private string referer;
		/// <summary>
		/// 
		/// </summary>
		public string Referer
		{
			get
			{
				return referer;
			}
		}

		public ImageViewUrlItem(string key, string replacement, string referer)
		{
			this.regex = new Regex(key, RegexOptions.IgnoreCase);
			this.replacement = replacement;
			this.referer = referer;
		}
	}

}
