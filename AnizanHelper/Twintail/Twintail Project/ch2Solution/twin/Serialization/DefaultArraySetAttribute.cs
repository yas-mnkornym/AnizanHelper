using System;

namespace Twintail3
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultArraySetAttribute : Attribute
	{
		int length;
		public int Length
//		{ get; set; }
		{get { return length; }set { length = value; }}

		object defaultValue;
		public object DefaultValue
		//		{ get; set; }
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		public DefaultArraySetAttribute(int length)
			: this(length, null)
		{
		}

		public DefaultArraySetAttribute(int length, object defaultValue)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException();

			this.Length = length;
			this.DefaultValue = defaultValue;
		}
	}
}
