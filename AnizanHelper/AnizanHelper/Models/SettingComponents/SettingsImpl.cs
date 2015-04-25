using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	internal class SettingsImpl : ISettings
	{
		#region Private Field
		List<Type> knownTypes_ = new List<Type>();
		Dictionary<string, object> settingsData_ = new Dictionary<string, object>(); // 設定データ
		Dictionary<string, SettingsImpl> settingsChildren_ = new Dictionary<string, SettingsImpl>(); // 子設定
		ISettingsSerializer childSettingsSerializer_ = new DataContractSettingsSerializer(); // 子設定のシリアライザ 
		#endregion


		#region Properties
		internal Dictionary<string, object> SettingsData { get { return settingsData_; } }

		protected SettingsImpl ParentSettings { get; private set; }
		#endregion

		#region Constructors
		public SettingsImpl(string tag)
		{
			Tag = tag;
		}

		public SettingsImpl(IEnumerable<Type> knownTypes)
			: this(null, knownTypes)
		{ }

		public SettingsImpl(string tag, IEnumerable<Type> knownTypes)
			: this(tag)
		{
			if (knownTypes != null) {
				knownTypes_.AddRange(knownTypes);
			}
		}

		protected SettingsImpl(SettingsImpl parentSettings, string tag, IEnumerable<Type> knownTypes)
			: this(tag, knownTypes)
		{
			if (parentSettings == null) { throw new ArgumentNullException("parentSettings"); }
			ParentSettings = parentSettings;

			// タグ編集
			List<string> tags = new List<string>();
			var settings = this;
			while (settings != null) {
				tags.Add(settings.Tag);
				settings = settings.ParentSettings;
			}
			tags.Reverse();
			Tag = string.Join("_", tags);
		}

		#endregion

		#region ISettings メンバ
		public IList<Type> KnownTypes
		{
			get
			{
				return knownTypes_;
			}
		}

		public string Tag { get; private set; }

		public void Set<T>(string key, T value)
		{
			var actKey = key;
			bool isNew = true;
			object oldValue = null;

			if (SettingsData.ContainsKey(actKey)) {
				oldValue = SettingsData[actKey];
				if (oldValue != null) {
					isNew = (!oldValue.Equals(value));
				}
				else {
					isNew = (value != null);
				}
			}

			if (isNew) {
				var args = new SettingChangeEventArgs(key, oldValue, value);
				OnSettingChanging(args);
				SettingsData[actKey] = value;
				OnSettingChanged(args);
			}
		}

		public T Get<T>(string key, T defaultValue = default(T))
		{
			var actKey = key;
			if (SettingsData.ContainsKey(actKey)) {
				return (T)settingsData_[actKey];
			}
			else {
				return defaultValue;
			}
		}

		public void SetCrypted<T>(string key, T value)
		{
			throw new NotImplementedException();
		}

		public T GetDecrypted<T>(string key, T defaultValue = default(T))
		{
			throw new NotImplementedException();
		}

		public bool Exists(string key)
		{
			var actKey = key;
			return SettingsData.ContainsKey(actKey);
		}

		public void Remove(string key)
		{
			var actKey = key; // GetTaggedKey(key);
			if (SettingsData.ContainsKey(actKey)) {
				var args = new SettingChangeEventArgs(key, SettingsData[actKey], null);
				OnSettingChanging(args);
				SettingsData.Remove(actKey);
				OnSettingChanged(args);
			}
		}

		public void Clear()
		{
			foreach (var key in SettingsData.Keys.ToArray()) {
				Remove(key);
			}
		}

		public IEnumerable<string> Keys
		{
			get
			{
				return SettingsData.Keys;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:オブジェクトを複数回破棄しない")]
		public ISettings GetChildSettings(string tag, IEnumerable<Type> knownTypes)
		{
			if (settingsChildren_.ContainsKey(tag)) {
				return settingsChildren_[tag];
			}
			else {
				var settings = new SettingsImpl(this, tag, knownTypes);

				// 設定をロード
				var setTag = GetTaggedKey(settings.Tag, true);
				var setStr = Get<string>(setTag, null);
				if (setStr != null) {
					using (var ms = new MemoryStream())
					using (var writer = new StreamWriter(ms, Encoding.UTF8)) {
						writer.Write(setStr);
						writer.Flush();
						ms.Seek(0, SeekOrigin.Begin);
						childSettingsSerializer_.Deserialize(ms, settings);
					}
				}

				// 子設定に追加
				settings.SettingChanged += settings_SettingChanged;
				settingsChildren_[settings.Tag] = settings;
				return settings;
			}
		}

		/// <summary>
		/// 子設定が変更されたとき、その値を保存
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void settings_SettingChanged(object sender, SettingChangeEventArgs e)
		{
			var settings = sender as SettingsImpl;
			if (settings != null) {
				var tag = GetTaggedKey(settings.Tag, true);
				string str = null;
				using (var ms = new MemoryStream()) {
					childSettingsSerializer_.Serialize(ms, settings);
					ms.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(ms)) {
						str = reader.ReadToEnd();
					}
				}

				// 設定保存
				Set(tag, str);
			}
		}

		public void RemoveChildSettings(string tag)
		{
			if (settingsChildren_.ContainsKey(tag)) {
				var settings = settingsChildren_[tag];
				settingsChildren_[tag] = null;
				settings.SettingChanged -= settings_SettingChanged;
			}
		}

		public IEnumerable<string> ChildSettingsTags
		{
			get
			{
				return settingsChildren_.Keys;
			}
		}

		public event EventHandler<SettingChangeEventArgs> SettingChanging;

		public event EventHandler<SettingChangeEventArgs> SettingChanged;
		#endregion // ISettings メンバ

		#region Private Methods
		string GetTaggedKey(string key, bool isEmbed = false)
		{
			string result;
			if (Tag == null) {
				result = key;
			}
			else {
				result = string.Format("{0}.{1}", Tag, key);
			}

			if (isEmbed) {
				return "__" + result;
			}
			else {
				return result;
			}
		}

		void OnSettingChanging(SettingChangeEventArgs args)
		{
			if (SettingChanging != null) {
				SettingChanging(this, args);
			}
		}

		void OnSettingChanged(SettingChangeEventArgs args)
		{
			if (SettingChanged != null) {
				SettingChanged(this, args);
			}
		}
		#endregion
	}
}
