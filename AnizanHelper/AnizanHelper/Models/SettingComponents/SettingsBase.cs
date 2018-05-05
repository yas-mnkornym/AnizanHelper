using System;
using System.Runtime.CompilerServices;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models.SettingComponents
{
	/// <summary>
	/// 設定の基本実装を提供するクラス
	/// </summary>
	public class SettingsBase : NotificationObjectWithNotifyChaning
	{
		ISettings settings_;

		/// <summary>
		/// 設定インスタンスを取得する
		/// </summary>
		public ISettings Settings { get { return settings_; } }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings">設定インスタンス</param>
		/// <param name="dispatcher">ディスパッチャ</param>
		protected SettingsBase(
			ISettings settings, // not null
			IDispatcher dispatcher = null)
			: base(dispatcher)
		{
			settings_ = settings ?? throw new ArgumentNullException("settings");
			settings_.SettingChanging += settings_SettingChanging;
			settings_.SettingChanged += settings_SettingChanged;
		}

		void settings_SettingChanging(object sender, SettingChangeEventArgs e)
		{
			RaisePropertyChanging(e.Key);
		}

		void settings_SettingChanged(object sender, SettingChangeEventArgs e)
		{
			RaisePropertyChanged(e.Key);
		}

		/// <summary>
		/// 呼び出したプロパティ名の設定を取得する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="defaultValue">デフォルト値</param>
		/// <param name="key">プロパティ名</param>
		/// <returns>取得した値</returns>
		protected T GetMe<T>(T defaultValue, [CallerMemberName]string key = null)
		{
			return Settings.Get(key, defaultValue);
		}

		/// <summary>
		/// 呼び出したプロパティ名の設定を設定する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="value">値</param>
		/// <param name="key">プロパティ名</param>
		protected void SetMe<T>(T value, [CallerMemberName]string key = null)
		{
			Settings.Set(key, value);
		}

		/// <summary>
		/// 呼び出したプロパティ名の復号された設定を取得する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="defaultValue">デフォルト値</param>
		/// <param name="key">プロパティ名</param>
		/// <returns>取得した値</returns>
		protected T GetMeDecrypted<T>(T defaultValue, string key)
		{
			return Settings.GetDecrypted(key, defaultValue);
		}

		/// <summary>
		/// 呼び出したプロパティ名の暗号化された設定を設定する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="value">値</param>
		/// <param name="key">プロパティ名</param>
		protected void SetMeCrypted<T>(T value, string key)
		{
			Settings.SetCrypted(key, value);
		}

		/// 呼び出したプロパティ名の設定を削除する。
		/// </summary>
		/// <param name="key">プロパティ名</param>
		protected void RemoveMe(string key)
		{
			Settings.Remove(key);
		}

		/// <summary>
		/// 設定を設定する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="value">データの値</param>
		protected void Set<T>(string key, T value)
		{
			Settings.Set(key, value);
		}

		/// <summary>
		/// 設定を取得する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="defaultValue">データの値</param>
		/// <returns>データ</returns>
		protected T Get<T>(string key, T defaultValue = default(T))
		{
			return Settings.Get(key, defaultValue);
		}

		/// <summary>
		/// データを削除する
		/// </summary>
		/// <param key="key">データのキー</param>
		protected void Remove(string key)
		{
			Settings.Remove(key);
		}

		/// <summary>
		/// データを全て削除する。
		/// </summary>
		protected void Clear()
		{
			Settings.Clear();
		}

	}
}
