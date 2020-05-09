using System;
using System.Collections.Generic;

namespace AnizanHelper.Models.SettingComponents
{
	public interface ISettingsContainer
	{
		/// <summary>
		/// シリアライズ/デシリアライズのための既知の型のリストを取得・設定する
		/// </summary>
		IList<Type> KnownTypes { get; }

		/// <summary>
		/// 設定のタグを取得する
		/// </summary>
		string Tag { get; }

		/// <summary>
		/// 設定を設定する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="value">データの値</param>
		void Set<T>(string key, T value);

		/// <summary>
		/// 設定を取得する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="defaultValue">データの値</param>
		/// <returns>データ</returns>
		T Get<T>(string key, T defaultValue = default(T));

		/// <summary>
		/// 暗号化された設定を設定する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="value">データの値</param>
		void SetCrypted<T>(string key, T value);


		/// <summary>
		/// 復号された設定を取得する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="defaultValue">データの値</param>
		/// <returns>データ</returns>
		T GetDecrypted<T>(string key, T defaultValue = default(T));

		/// <summary>
		/// データが存在するかを確認する
		/// </summary>
		/// <param key="key">データのキー</param>
		/// <returns>存在すればtrue</returns>
		bool Exists(string key);

		/// <summary>
		/// データを削除する
		/// </summary>
		/// <param key="key">データのキー</param>
		void Remove(string key);

		/// <summary>
		/// データを全て削除する
		/// </summary>
		void Clear();

		/// <summary>
		/// 設定キーをすべて取得する
		/// </summary>
		IEnumerable<string> Keys { get; }

		/// <summary>
		/// 子設定を取得する。
		/// </summary>
		/// <param name="tag">子設定のタグ</param>
		/// <param name="knownTypes">子設定をデシリアライズするための既知の型</param>
		/// <remarks>同じタグで複数回呼び出しても、同じ値が返される。</remarks>
		/// <returns>子設定</returns>
		ISettingsContainer GetChildSettings(string tag, IEnumerable<Type> knownTypes);

		/// <summary>
		/// 子設定を削除する。
		/// </summary>
		/// <param name="tag">子設定のタグ</param>
		void RemoveChildSettings(string tag);

		/// <summary>
		/// 子設定のタグ一覧を取得する
		/// </summary>
		IEnumerable<string> ChildSettingsTags { get; }



		/// <summary>
		/// 設定が変更されることを通知するイベント
		/// </summary>
		event EventHandler<SettingChangeEventArgs> SettingChanging;

		/// <summary>
		/// 設定が変更されたことを通知するイベント
		/// </summary>
		event EventHandler<SettingChangeEventArgs> SettingChanged;
	}
}
