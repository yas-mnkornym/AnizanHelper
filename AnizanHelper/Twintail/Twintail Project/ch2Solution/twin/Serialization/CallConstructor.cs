using System;

namespace Twintail3
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CallConstructorAttribute : Attribute
	{
		private object[] arguments;
		public object[] Arguments
		//		{ get; set; }
		{
			get
			{
				return arguments;
			}
			set
			{
				arguments = value;
			}
		}

		public CallConstructorAttribute()
		{
		}

		public CallConstructorAttribute(params object[] arguments)
		{
			this.Arguments = arguments;
		}
	}
}
