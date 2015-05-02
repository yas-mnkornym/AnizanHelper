using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AnizanHelper.Models.Registries
{
	internal class RegistryEditor : IDisposable
	{
		public string RegName { get; private set; }
		RegistryKey baseKey_ = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="regName">レジストリ名</param>
		/// <param name="toMachine">trueならLocalMachineに、falseならCurrentUserに書き込む</param>
		public RegistryEditor(string regName, bool toMachine = false)
		{
			RegName = regName;
			baseKey_ = toMachine ? Registry.LocalMachine : Registry.CurrentUser;
		}

		/// <summary>
		/// レジストリの値を取得する
		/// </summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="keyName">レジストリキー名</param>
		/// <param name="defValue">デフォルト値</param>
		/// <returns></returns>
		public T GetValue<T>(string keyName, T defValue = default(T))
		{
			using (var key = GetKey(keyName, false, false)) {
				if (key != null) {
					var value = key.GetValue(RegName, defValue);
					if (value is T) {
						return (T)value;
					}
				}
			}
			return defValue;
		}


		public bool IsKeyExists(string keyName)
		{
			using (var key = GetKey(keyName, false, false)) {
				if (key == null) {
					return false;
				}
				else {
					if (key.GetValue(RegName) == null) {
						return false;
					}
					else {
						return true;
					}
				}
			}
		}

		/// <summary>
		/// レジストリの値を設定する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="keyName">レジストリキー名</param>
		/// <param name="value">レジストリの値</param>
		/// <param name="valueKind">レジストリの種類</param>
		public void SetValue<T>(string keyName, T value, RegistryValueKind valueKind)
		{
			// キー取得
			using (var key = GetKey(keyName)) {
				// 書き込む
				key.SetValue(RegName, value, valueKind);
			}
		}

		/// <summary>\
		/// レジストリキーを削除する
		/// </summary>
		/// <param name="keyName">レジストリキー名</param>
		public void DeleteKey(string keyName)
		{
			// キー取得
			using (var key = GetKey(keyName, false, true)) {
				// 削除する
				if (key != null) {
					if (key.GetValue(RegName, null) != null) {
						key.DeleteValue(RegName);
					}
				}
			}
		}


		/// <summary>
		/// レジストリキーを取得する
		/// </summary>
		/// <param name="keyName">レジストリキー名</param>
		/// <param name="createIfNotExisting">trueならキーが存在しない場合作成する</param>
		/// <returns>レジストリキー</returns>
		RegistryKey GetKey(string keyName, bool createIfNotExisting = true, bool writable = true)
		{
			RegistryKey key = null;
			// レジストリを開く
			try {
				key = baseKey_.OpenSubKey(keyName, writable);
			}
			catch (Exception ex) {
				throw new Exception(
					string.Format("レジストリキー {0} の取得に失敗しました。", keyName),
					ex);
			}

			// なかったら作る
			if (key == null && createIfNotExisting) {
				try {
					key = baseKey_.CreateSubKey(keyName);
				}
				catch (Exception ex) {
					throw new Exception(
						string.Format("レジストリキー {0} の作成に失敗しました。", keyName),
						ex);
				}
			}

			// 返す
			return key;
		}

		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				baseKey_.Dispose();
				baseKey_ = null;
			}
			isDisposed_ = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
