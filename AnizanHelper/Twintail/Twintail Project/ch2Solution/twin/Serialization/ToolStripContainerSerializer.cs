using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Twintail3;

namespace Twintail.Serialization
{
	/// <summary>
	/// なんか標準のToolStripManager.LoadSettingsがちゃんと復元されないので自前で保存・復元するクラスです。
	/// </summary>
	public class ToolStripContainerSerializer
	{
		private static Dictionary<string, ToolStripPanel> GetToolStripPanelDictionary(ToolStripContainer Container)
		{
			Dictionary<string, ToolStripPanel> allPanels = new Dictionary<string,ToolStripPanel>();
			allPanels.Add("Top", Container.TopToolStripPanel);
			allPanels.Add("Left", Container.LeftToolStripPanel);
			allPanels.Add("Right", Container.RightToolStripPanel);
			allPanels.Add("Bottom", Container.BottomToolStripPanel);

			return allPanels;
		}

		/// <summary>
		/// 指定した ToolStripContainer 内のすべてのパネル内に子として存在する  ToolStrip コントロールの位置情報をファイルに保存します。
		/// </summary>
		/// <param name="Container"></param>
		/// <param name="fileName"></param>
		public static void Save(ToolStripContainer Container, string fileName)
		{
			List<ToolStripPanelSetting> panelsSetting = new List<ToolStripPanelSetting>();
			foreach (KeyValuePair<string, ToolStripPanel> panel in GetToolStripPanelDictionary(Container))
			{
				List<ToolStripSetting> rows = new List<ToolStripSetting>();
				foreach (ToolStripPanelRow row in panel.Value.Rows)
				{
					foreach (Control c in row.Controls)
					{
						ToolStrip item = c as ToolStrip;

						if (item == null)
							return;

						rows.Add(new ToolStripSetting(item.Name, item.Location));
					}
				}

				rows.Sort();
				panelsSetting.Add(new ToolStripPanelSetting(panel.Key, rows.ToArray()));
			}

			ToolStripdockContainerSetting setting = new ToolStripdockContainerSetting(fileName, panelsSetting.ToArray());
			setting.Save();
		}

		/// <summary>
		/// 設定ファイルを読み込み、指定したコンテナの状態を復元します。
		/// </summary>
		/// <param name="parentCtrl">Containerを子に持つ最上位の親コントロール、またはメインフォーム。</param>
		/// <param name="Container">状態を復元するコンテナ</param>
		/// <param name="fileName">設定ファイルの保存先ファイルパス</param>
		public static void Load(Control parentCtrl, ToolStripContainer Container, string fileName)
		{		
			ToolStripdockContainerSetting dockContainerSetting = new ToolStripdockContainerSetting(fileName);
			dockContainerSetting.Load();

			if (!dockContainerSetting.IsDeserialized)
				return;

			Dictionary<string, ToolStripPanel> allPanels = GetToolStripPanelDictionary(Container);
			Dictionary<string, ToolStrip> tsDic = new Dictionary<string, ToolStrip>();

			Dictionary<ToolStrip, ToolStripPanel> remainds = new Dictionary<ToolStrip, ToolStripPanel>();

			// 全パネル内のToolStripコントロールをすべて tsDic に入れ、一度コントロールコレクションをクリアする
			foreach (ToolStripPanel panel in allPanels.Values)
			{
				foreach (Control c in panel.Controls)
				{
					if (c is ToolStrip)
					{
						tsDic.Add(c.Name, (ToolStrip)c);
						remainds.Add((ToolStrip)c, panel);
					}
				}
				panel.Controls.Clear();
			}

			// 保存されている順に再度パネルにToolStripを追加していく
			foreach (ToolStripPanelSetting panelSetting in dockContainerSetting.Panels)
			{
				if (!allPanels.ContainsKey(panelSetting.Alignment))
					continue;

				ToolStripPanel panel = allPanels[panelSetting.Alignment];
				foreach (ToolStripSetting setting in panelSetting.Rows)
				{
					ToolStrip item = null;

					if (tsDic.ContainsKey(setting.Name))
					{
						item = tsDic[setting.Name];
					}
					else if (parentCtrl != null)
					{
						// 現在のToolStripContainer内にコントロールが見つからない場合は親のコントロール内から探す
						foreach (Control c in parentCtrl.Controls.Find(setting.Name, true))
						{
							if (c is ToolStrip)
							{
								item = (ToolStrip)c;
								break;
							}
						}
					}

					if (item != null)
					{
						panel.Join(item, setting.Location);
						remainds.Remove(item);
					}
				}
			}

			// どこにも追加されずに残ったToolStripをもともとあったパネルに追加する
			foreach (KeyValuePair<ToolStrip, ToolStripPanel> pair in remainds)
				pair.Value.Join(pair.Key, Point.Empty);
		}
	}

	public class ToolStripdockContainerSetting : ApplicationSettingsSerializer
	{
		ToolStripPanelSetting[] panels;
		[ExpandableSerialize,DefaultArrayLength(0)]
		public ToolStripPanelSetting[] Panels
		{
			get
			{
				return panels;
			}
			set
			{
				panels = value;
			}
		}
		
		public ToolStripdockContainerSetting(string fileName)
			: this(fileName, new ToolStripPanelSetting[] {})
		{
		}

		public ToolStripdockContainerSetting(string fileName, ToolStripPanelSetting[] panels)
			: base(fileName, true)
		{
			this.Panels = panels;
		}
	}

	[Serializable]
	public class ToolStripPanelSetting
	{
		string alignment;
		public string Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				alignment = value;
			}
		}

		ToolStripSetting[] rows;
		[ExpandableSerialize,DefaultArrayLength(0)]
		public ToolStripSetting[] Rows
		{
			get
			{
				return rows;
			}
			set
			{
				rows = value;
			}
		}

		public ToolStripPanelSetting()
			: this(String.Empty, new ToolStripSetting[] { })
		{
		}

		public ToolStripPanelSetting(string alignment, ToolStripSetting[] rows)
		{
			this.Alignment = alignment;
			this.Rows = rows;
		}
	}

	[Serializable]
	public class ToolStripSetting : IComparable<ToolStripSetting>
	{
		private string name;
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		private Point location;
		public Point Location
		{
			get
			{
				return location;
			}
			set
			{
				location = value;
			}
		}


		public ToolStripSetting()
			: this(String.Empty, Point.Empty)
		{
		}

		public ToolStripSetting(string name, Point location)
		{
			this.Name = name;
			this.Location = location;
		}

		#region IComparable<ToolStripSettings> メンバ

		public int CompareTo(ToolStripSetting item)
		{
			int ret = this.Location.X - item.Location.X;
			return ret != 0 ? ret : this.Location.Y - item.Location.Y;
		}

		#endregion
	}
}
