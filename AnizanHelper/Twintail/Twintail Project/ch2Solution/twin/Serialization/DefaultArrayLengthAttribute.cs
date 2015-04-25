using System;

namespace Twintail.Serialization
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultArrayLengthAttribute : Attribute
	{
		private int length;

		public int Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
			}
		}


		private object defValue;

		public object DefaultValue
		{
			get
			{
				return defValue;
			}
			set
			{
				defValue = value;
			}
		}


		private object[] defValueArray;

		public object[] DefaultValueSet
		{
			get
			{
				return defValueArray;
			}
			set
			{
				defValueArray = value;
			}
		}
	
		
		public DefaultArrayLengthAttribute(int length)
			: this(length, null)
		{
		}

		public DefaultArrayLengthAttribute(int length, object defaultValue)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException();

			this.Length = length;
			this.DefaultValue = defaultValue;
		}

		public DefaultArrayLengthAttribute(params object[] values)
		{
			this.Length = values.Length;
			this.DefaultValueSet = values;
		}

		public Array CreateDefaultArrayInstance(Type arrayType)
		{
			Array array = (Array)Activator.CreateInstance(arrayType, new object[] {this.Length});
			for (int i = 0; i < array.Length; i++)
			{
				if (this.DefaultValueSet != null)
					array.SetValue(this.DefaultValueSet[i], i);
else
					array.SetValue(this.DefaultValue, i);
			}
			return array;
		}
	}
}
