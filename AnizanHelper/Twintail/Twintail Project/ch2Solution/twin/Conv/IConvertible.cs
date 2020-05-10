// IConvertible.cs

namespace Twin.Conv
{
	using System;

	/// <summary>
	/// ログの相互コンバータのインターフェース
	/// </summary>
	public interface IConvertible
	{
		/// <summary>
		/// 指定したファイルのログを読み込む
		/// </summary>
		/// <param name="filePath">読み込むログへのファイルパス</param>
		/// <param name="header">読み込んだログのスレッド上法が格納される</param>
		/// <param name="resCollection">読み込んだログ本文が格納される</param>
		/// <returns>読み込みに成功すればtrue、失敗すればfalseを返す</returns>
		void Read(string filePath, out ThreadHeader header, out ResSetCollection resCollection);
		
		/// <summary>
		/// 指定したファイルにログを書き込む
		/// </summary>
		/// <param name="filePath">保存先ログファイルへのパス</param>
		/// <param name="header">スレッド情報</param>
		/// <param name="resCollection">保存するログ本文</param>
		/// <returns>書き込みに成功すればtrue、失敗すればfalseを返す</returns>
		void Write(string filePath, ThreadHeader header, ResSetCollection resCollection);
	}
}
