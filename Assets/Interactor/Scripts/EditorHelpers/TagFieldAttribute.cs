using System;

namespace razz
{
	[AttributeUsage(AttributeTargets.Field)]
	public class TagFieldAttribute : Attribute
	{
		public string categoryName;
	}
}