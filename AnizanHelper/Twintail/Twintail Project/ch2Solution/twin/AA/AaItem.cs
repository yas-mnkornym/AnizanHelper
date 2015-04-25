// AaItem.cs

namespace Twin.Aa
{
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// １つのAAを表す
	/// </summary>
	public class AaItem
	{
		internal AaHeader parent;
		private string text;
		private bool single;

		/// <summary>
		/// このAAが定義されているヘッダーを取得
		/// </summary>
		public AaHeader Parent {
			get { return parent; }
		}

		/// <summary>
		/// AAの表示名を取得
		/// </summary>
		public string Text {
			set {
				if (value == null) {
					throw new ArgumentNullException("Text");
				}
				if (value.IndexOf('\n') != -1) {
					throw new ArgumentException("Textに改行文字を含めることは出来ません");
				}
				text = value;
			}
			get { return text; }
		}

		/// <summary>
		/// AAのデータを取得
		/// </summary>
		public string Data {
			set {
				#region
				if (value == null) {
					throw new ArgumentNullException("Data");
				}
				if (Single) 
				{
					if (value.IndexOf('\n') != -1) {
						throw new ArgumentException("１行AAに改行文字を含めることは出来ません");
					}
					text = value;
				}
				else {
					// 複数行AAへのパス
					string aaPath = Path.Combine(
						Path.GetDirectoryName(parent.FileName), text);

					StreamWriter sw = null;

					try {
						sw = new StreamWriter(aaPath, false, TwinDll.DefaultEncoding);
						sw.Write(value);
					}
					finally {
						if (sw != null)
							sw.Close();
					}
				}
				#endregion
			}
			get {
				#region
				if (Single)
				{
					return text;
				}
				else {
					if (parent == null) {
						throw new InvalidOperationException("Parentが設定されていません");
					}

					// 複数行AAへのパス
					string aaPath = Path.Combine(
						Path.GetDirectoryName(parent.FileName), text);

					StreamReader sr = null;

					try {
						sr = new StreamReader(aaPath, TwinDll.DefaultEncoding);
						return sr.ReadToEnd();
					}
					catch (FileNotFoundException) {}
					finally {
						if (sr != null)
							sr.Close(); 
					}

					return String.Empty;
				}
				#endregion
			}
		}

		/// <summary>
		/// １行AAの場合はtrue、複数行AAの場合はfalseを返す
		/// </summary>
		public bool Single {
			get { return single; }
		}

		/// <summary>
		/// AaItemクラスのインスタンスを初期化
		/// </summary>
		/// <param name="text">ヘッダーファイル内の表示名</param>
		/// <param name="single">１行AAならtrue、複数行AAならfalseを指定</param>
		public AaItem(string text, bool single)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.text = text;
			this.single = single;
		}

		/// <summary>
		/// このインスタンスをヘッダーから削除
		/// </summary>
		public void Remove()
		{
			if (parent != null)
				parent.Items.Remove(this);
		}

		/// <summary>
		/// このインスタンスを文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0}{1}",
				single ? String.Empty : "#", text);
		}
	}
}
