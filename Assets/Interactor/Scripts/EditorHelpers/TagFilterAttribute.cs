using UnityEngine;

namespace razz
{
	public class TagFilterAttribute : PropertyAttribute
	{
		public System.Type Type { get; private set; }

		public TagFilterAttribute(System.Type type)
		{
			Type = type;
		}
	}
}