using System;

namespace AnizanHelper.Services
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class ServiceAttribute : Attribute
	{
		public bool IsEnabled { get; set; } = true;
	}
}
