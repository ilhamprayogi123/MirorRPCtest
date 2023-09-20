using System;
using UnityEngine;

namespace razz
{
	/*
	 This Readme and its Editor is based on Unity HDRP readme script.
		 */
	public class Readme : ScriptableObject
	{
		public Texture2D icon;
		public float iconMaxWidth = 128f;
		public string title;
		public Section[] sections;

		[Serializable]
		public class Section
		{
			public string heading, text, linkText, url;
		}
	}
}
