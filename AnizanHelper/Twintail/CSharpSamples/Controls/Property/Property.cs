// Property.cs

namespace CSharpSamples
{
	using System;
	using System.Collections;
	
	/// <summary>
	/// １つのプロパティを表す
	/// </summary>
	public class Property
	{
		private string caption;
		private object data;

		/// <summary>
		/// キャプションを取得または設定
		/// </summary>
		public string Caption {
			set { caption = value; }
			get { return caption; }
		}

		/// <summary>
		/// データを取得または設定
		/// </summary>
		public object Data {
			set { data = value; }
			get { return data; }
		}


		/// <summary>
		/// Propertyクラスをコレクション管理
		/// </summary>
		public class PropertyCollection : CollectionBase
		{
			private PropertyDialog owner;

			/// <summary>
			/// indexor
			/// </summary>
			public Property this[int index] {
				get {
					return (Property)List[index];
				}
			}

			internal PropertyCollection(PropertyDialog owner)
			{
				if (owner == null) {
					throw new ArgumentNullException("owner");
				}

				this.owner = owner;
			}

			public int Add(Property property)
			{
				owner.CreatePage(property);
				return List.Add(property);
			}

			public void AddRange(PropertyCollection properties)
			{
				foreach (Property property in properties)
					Add(property);
			}

			public void AddRange(Property[] properties)
			{
				foreach (Property property in properties)
					Add(property);
			}

			public void Remove(Property property)
			{
				owner.RemovePage(property);
				List.Remove(property);
			}
		}
	}
}
