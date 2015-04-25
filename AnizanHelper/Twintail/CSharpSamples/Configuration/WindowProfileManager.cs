// WindowProfileManager.cs

using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSharpSamples
{
	/// <summary>
	/// Form ウインドウの状態を .ini 形式で保存/復元するクラス。
	/// </summary>
	public class WindowProfileManager
	{
		private Form form = null;
		private Rectangle normalWindowRect = Rectangle.Empty;

		/// <summary>
		/// WindowProfileManager クラスのインスタンスを初期化。
		/// </summary>	
		public WindowProfileManager(Form f)
		{
			if (f == null)
				throw new ArgumentNullException("f");
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			
			normalWindowRect = 
				new Rectangle(f.Location, f.ClientSize);

			form = f;
			form.Move += new EventHandler(OnResize);
			form.Resize += new EventHandler(OnResize);
		}

		private void OnResize(object sender, EventArgs e)
		{
			if (form.WindowState == FormWindowState.Normal)
			{
				normalWindowRect = new Rectangle(
					form.Location, form.ClientSize);
			}
		}

		public void Serialize(string fileName)
		{
			CSPrivateProfile prof = new CSPrivateProfile();
			Save(prof);

			prof.Write(fileName);
		}

		public void Deserialize(string fileName)
		{
			CSPrivateProfile prof = new CSPrivateProfile();
			prof.Read(fileName);

			Load(prof);
		}

		public virtual void Save(CSPrivateProfile prof)
		{
			prof.SetValue("Window", "Bounds", normalWindowRect);
			prof.SetValue("Window", "State", form.WindowState);
		}

		public virtual void Load(CSPrivateProfile prof)
		{
			form.WindowState = (FormWindowState)
				prof.GetEnum("Window", "State", form.WindowState);
			
			Rectangle rc = prof.GetRect("Window", "Bounds", normalWindowRect);
			form.Location = rc.Location;
			form.ClientSize = rc.Size;
		}
	}
}
