// LocalThreadReader.cs

namespace Twin.IO
{
	using System;
	using System.IO;
	using System.Text;
	using Twin.Text;

	/// <summary>
	/// LocalThreadReader の概要の説明です。
	/// </summary>
	public class LocalThreadReader : ThreadReaderBase
	{
		/// <summary>
		/// LocalThreadReaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="dataParser"></param>
		public LocalThreadReader(ThreadParser dataParser)
			: base(dataParser)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		public bool __Open(string path)
		{
			if (File.Exists(path))
			{
				baseStream = new FileStream(path, FileMode.Open);
				base.length = (int)baseStream.Length;
				base.isOpen = true;
				base.index = 1;
				return true;
			}
			return false;
		}

		public override bool Open(ThreadHeader header)
		{
			throw new NotImplementedException("このメソッドは使用できません。");
		}
	}
}
