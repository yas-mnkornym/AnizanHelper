// IArchivable.cs

namespace CSharpSamples
{
	/// <summary>
	/// IArchivable インターフェースです。
	/// </summary>
	public interface IArchivable
	{
		/// <summary>
		/// 解凍に対応していれば true、未対応なら false を返します。
		/// </summary>
		bool CanExtract
		{
			get;
		}

		/// <summary>
		/// 圧縮に対応していれば true、未対応なら false を返します。
		/// </summary>
		bool CanCompress
		{
			get;
		}
	}
}
