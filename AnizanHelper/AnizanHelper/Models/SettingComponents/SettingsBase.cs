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
		/// <summary>
		/// 設定インスタンスを取得する
		/// </summary>
		public ISettingsContainer SettingsContainer { get; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settingsContainer">設定インスタンス</param>
		/// <param name="dispatcher">ディスパッチャ</param>
		protected SettingsBase(
			ISettingsContainer settingsContainer,
			IDispatcher dispatcher = null)
			: base(dispatcher)
		{
			this.SettingsContainer = settingsContainer ?? throw new ArgumentNullException(nameof(settingsContainer));
			this.SettingsContainer.SettingChanging += this.SettingsContainer_SettingChanging;
			this.SettingsContainer.SettingChanged += this.SettingsContainer_SettingChanged;
		}

		private void SettingsContainer_SettingChanging(object sender, SettingChangeEventArgs e)
		{
			this.RaisePropertyChanging(e.Key);
		}

		private void SettingsContainer_SettingChanged(object sender, SettingChangeEventArgs e)
		{
			this.RaisePropertyChanged(e.Key);
		}

		/// <summary>
		/// 呼び出したプロパティ名の設定を取得する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="defaultValue">デフォルト値</param>
		/// <param name="key">プロパティ名</param>
		/// <returns>取得した値</returns>
		protected T GetValue<T>(T defaultValue = default, [CallerMemberName]string key = null)
		{
			return this.SettingsContainer.Get(key, defaultValue);
		}

		/// <summary>
		/// 呼び出したプロパティ名の設定を設定する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="value">値</param>
		/// <param name="key">プロパティ名</param>
		protected void SetValue<T>(T value, [CallerMemberName]string key = null)
		{
			this.SettingsContainer.Set(key, value);
		}

		/// <summary>
		/// 呼び出したプロパティ名の復号された設定を取得する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="defaultValue">デフォルト値</param>
		/// <param name="key">プロパティ名</param>
		/// <returns>取得した値</returns>
		protected T GetValueDecrypted<T>(T defaultValue, string key)
		{
			return this.SettingsContainer.GetDecrypted(key, defaultValue);
		}

		/// <summary>
		/// 呼び出したプロパティ名の暗号化された設定を設定する。
		/// </summary>
		/// <typeparam name="T">設定の型</typeparam>
		/// <param name="value">値</param>
		/// <param name="key">プロパティ名</param>
		protected void SetValueCrypted<T>(T value, string key)
		{
			this.SettingsContainer.SetCrypted(key, value);
		}

		/// 呼び出したプロパティ名の設定を削除する。
		/// </summary>
		/// <param name="key">プロパティ名</param>
		protected void RemoveValue(string key)
		{
			this.SettingsContainer.Remove(key);
		}

		/// <summary>
		/// 設定を設定する
		/// </summary>
		/// <typeparam key="T">データの型</typeparam>
		/// <param key="key">データのキー</param>
		/// <param key="value">データの値</param>
		protected void Set<T>(string key, T value)
		{
			this.SettingsContainer.Set(key, value);
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
			return this.SettingsContainer.Get(key, defaultValue);
		}

		/// <summary>
		/// データを削除する
		/// </summary>
		/// <param key="key">データのキー</param>
		protected void Remove(string key)
		{
			this.SettingsContainer.Remove(key);
		}

		/// <summary>
		/// データを全て削除する。
		/// </summary>
		protected void Clear()
		{
			this.SettingsContainer.Clear();
		}

	}
}
