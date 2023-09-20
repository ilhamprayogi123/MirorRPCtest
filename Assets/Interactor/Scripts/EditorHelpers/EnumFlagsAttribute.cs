using UnityEngine;

namespace razz
{
	public class EnumFlagsAttribute : PropertyAttribute
	{
		public string enumName;

		public EnumFlagsAttribute() { }

		public EnumFlagsAttribute(string name)
		{
			enumName = name;
		}
	}

	public class ReadOnlyAttribute : PropertyAttribute
	{

	}
}
