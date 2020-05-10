// NGWords.cs

namespace Twin
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using CSharpSamples;

	/// <summary>
	/// NGワードを一括管理
	/// </summary>
	public class NGWords
	{
		private NGWordCollection body;
		private NGWordCollection name;
		private NGWordCollection email;
		private NGWordCollection id;
		private NGWordCollection subj;

		/// <summary>
		/// 本文に適用するNGワードの文字列コレクションを取得
		/// </summary>
		public NGWordCollection Body
		{
			get
			{
				return body;
			}
		}

		/// <summary>
		/// 名前欄に適用するNGワードの文字列コレクションを取得
		/// </summary>
		public NGWordCollection Name
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// メール欄に適用するNGワードの文字列コレクションを取得
		/// </summary>
		public NGWordCollection Email
		{
			get
			{
				return email;
			}
		}

		/// <summary>
		/// IDに適用するNGワードの文字列コレクションを取得
		/// </summary>
		public NGWordCollection ID
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// スレッド名に適用するNGワードの文字列コレクションを取得
		/// </summary>
		public NGWordCollection Subject
		{
			get
			{
				return subj;
			}
		}

		/// <summary>
		/// NGWordsクラスのインスタンスを初期
		/// </summary>
		public NGWords()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			email = new NGWordCollection();
			name = new NGWordCollection();
			subj = new NGWordCollection();
			body = new NGWordCollection();
			id = new NGWordCollection();
		}

		/// <summary>
		/// NGWordsクラスのインスタンスを初期化
		/// </summary>
		public NGWords(string filePath)
			: this()
		{
			Load(filePath);
		}

		/// <summary>
		/// ファイルからNGワードを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		public void Load(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			email.Clear();
			name.Clear();
			subj.Clear();
			body.Clear();
			id.Clear();

			if (File.Exists(filePath))
			{
				KeyValuesCollection keys = new KeyValuesCollection();
				keys.Read(filePath);

				StringCollection coll;

				coll = keys["Email"];
				if (coll != null) email.AddRange(coll);

				coll = keys["Name"];
				if (coll != null) name.AddRange(coll);

				coll = keys["Subject"];
				if (coll != null) subj.AddRange(coll);

				coll = keys["Body"];
				if (coll != null) body.AddRange(coll);

				coll = keys["ID"];
				if (coll != null) id.AddRange(coll);

				keys = null;
			}
		}

		/// <summary>
		/// 指定したファイルに保存
		/// </summary>
		/// <param name="filePath"></param>
		public void Save(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			KeyValuesCollection keys = new KeyValuesCollection();

			keys.Add(new KeyValues("Subject", subj.GetPatterns()));
			keys.Add(new KeyValues("Email", email.GetPatterns()));
			keys.Add(new KeyValues("Name", name.GetPatterns()));
			keys.Add(new KeyValues("Body", body.GetPatterns()));
			keys.Add(new KeyValues("ID", id.GetPatterns()));
			keys.Write(filePath);
			keys = null;
		}

		/// <summary>
		/// textがNGワードに一致するかどうかを判断
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public bool IsMatch(string text, out ResSetElement matchedElement)
		{
			if (body.IsMatch(text))
			{
				matchedElement = ResSetElement.Body;
				return true;
			}
			else if (name.IsMatch(text))
			{
				matchedElement = ResSetElement.Name;
				return true;
			}
			else if (email.IsMatch(text))
			{
				matchedElement = ResSetElement.Email;
				return true;
			}
			else if (id.IsMatch(text))
			{
				matchedElement = ResSetElement.ID;
				return true;
			}
			else
			{
				matchedElement = ResSetElement.Unknown;
				return false;
			}
		}

		public bool IsMatch(ResSet res, out ResSetElement matchedElement, out string matchWord)
		{
			if (body.IsMatch(res.Body, out matchWord))
			{
				matchedElement = ResSetElement.Body;
				return true;
			}
			else if (name.IsMatch(res.Name, out matchWord))
			{
				matchedElement = ResSetElement.Name;
				return true;
			}
			else if (email.IsMatch(res.Email, out matchWord))
			{
				matchedElement = ResSetElement.Email;
				return true;
			}
			else if (id.IsMatch(res.ID, out matchWord))
			{
				matchedElement = ResSetElement.ID;
				return true;
			}
			else
			{
				matchedElement = ResSetElement.Unknown;
				return false;
			}
		}

		/// <summary>
		/// 指定したレスに一致するかどうかを判断
		/// </summary>
		/// <param name="res"></param>
		/// <returns></returns>
		public bool IsMatch(ResSet res, out ResSetElement matchedElement)
		{
			string matchWord;
			return IsMatch(res, out matchedElement, out matchWord);
		}

		/// <summary>
		/// subjectがNGスレッド名に一致するかどうかを判断
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public bool IsMatchSubject(string subject)
		{
			return subj.IsMatch(subject);
		}

		/// <summary>
		/// すべてのNGワードを削除
		/// </summary>
		public void Clear()
		{
			body.Clear();
			name.Clear();
			email.Clear();
			id.Clear();
			subj.Clear();
		}
	}
}
