using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AnizanHelper.Models
{
	public class DictionaryManager : IDictionaryManager
	{
		public string[] DictionaryFilePaths => new string[]
		{
			AppInfo.Current.DictionaryFilePath,
			AppInfo.Current.UserDictionaryfilePath,
		};

		private AnizanSongInfoConverter SongInfoConverter { get; }
		private SongPresetRepository SongPresetRepository { get; }

		public DictionaryManager(SongPresetRepository songPresetRepository, AnizanSongInfoConverter songInfoConverter)
		{
			this.SongPresetRepository = songPresetRepository ?? throw new ArgumentNullException(nameof(songPresetRepository));
			this.SongInfoConverter = songInfoConverter ?? throw new ArgumentNullException(nameof(songInfoConverter));
		}

		public async Task LoadAsync(CancellationToken cancellationToken = default)
		{
			var replaceList = new List<ReplaceInfo>();
			var presetList = new List<AnizanSongInfo>();

			try
			{
				if (File.Exists(AppInfo.Current.DictionaryFilePath))
				{
					var loader = new DictionaryFileReader();
					await loader.LoadAsync(AppInfo.Current.DictionaryFilePath).ConfigureAwait(false);

					replaceList.AddRange(loader.Replaces);
					presetList.AddRange(loader.SongPresets);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					string.Format("置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex)
					, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			// ユーザ辞書の内容を優先
			try
			{
				if (File.Exists(AppInfo.Current.UserDictionaryfilePath))
				{
					var loader = new DictionaryFileReader();
					await loader.LoadAsync(AppInfo.Current.UserDictionaryfilePath).ConfigureAwait(false);

					foreach (var replace in loader.Replaces)
					{
						var item = replaceList.FirstOrDefault(x => x.Original == replace.Original);
						if (item != null)
						{
							replaceList.Remove(item);
						}

						replaceList.Add(replace);
					}

					presetList.AddRange(loader.SongPresets);
				}
				else
				{
					try
					{
						File.WriteAllText(AppInfo.Current.UserDictionaryfilePath, "rem,これはユーザ定義辞書ファイルです。dictionary.txtを参考にUTF-8で記述して下さい。");
					}
					catch { }
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					App.Current.MainWindow,
					string.Format("ユーザ定義置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex),
					"エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			this.SongInfoConverter.Replaces = replaceList
				.Where(x => !string.IsNullOrEmpty(x.Original))
				.ToArray();

			this.SongPresetRepository.Presets = presetList.ToArray();
		}
	}
}
