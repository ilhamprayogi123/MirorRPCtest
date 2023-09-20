using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace razz
{
	[CustomPropertyDrawer(typeof(TagFilterAttribute))]
	public class TagFilterPropertyDrawer : PropertyDrawer
	{
		public int currentIndex;
		public StringBuilder fs = new StringBuilder(128);
		public List<FieldInfo> fields = new List<FieldInfo>();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//Multiple select messes up all selections and change them all to last selected. Don't want to work on fix, instead don't show them so they won't change value unintentionally.
			if (property.hasMultipleDifferentValues) return;

			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var tagFilter = attribute as TagFilterAttribute;
			var tagType = tagFilter.Type;

			var objectFields = ReturnConst(tagType);

			var listNames = new List<string>();
			fields.Clear();

			var vv = property.intValue;

			for (var i = 0; i < objectFields.Length; i++)
			{
				var myFieldInfo = objectFields[i];
				var tagField = Attribute.GetCustomAttribute(objectFields[i], typeof(TagFieldAttribute)) as TagFieldAttribute;
				if (tagField == null) continue;

				fs.Append(tagField.categoryName).Append("/").Append(myFieldInfo.Name);
				listNames.Add(fs.ToString());
				fs.Length = 0;
				fields.Add(myFieldInfo);
				if (vv == (int) myFieldInfo.GetValue(this))
					currentIndex = fields.Count - 1;
			}

			if (listNames.Count == 0)
			{
				EditorGUI.EndProperty();
				return;
			}

			Color prev = GUI.color;
			if (currentIndex == 0)
			{
				GUI.color = new Color(1f, 0.5f, 0.5f, 1);
			}
			else
			{
				GUI.color = prev;
			}
			
			currentIndex = EditorGUI.Popup(position, property.displayName, currentIndex, listNames.ToArray());
			
			GUI.color = prev;
			
			/*EditorGUI.LabelField(new Rect(position.x + 200, position.y + 20, position.width, position.height), " ", " ID: " + property.intValue, EditorStyles.label);*/

			EditorGUILayout.HelpBox("Don't forget: Type specific settings will change every InteractorObject that uses same settings file and will keep changes made in runtime.", MessageType.Warning);

			var raw = listNames[currentIndex].Split('/');
			var name = raw[raw.Length - 1];

			var field = fields.Find(f => f.Name == name);
			property.intValue = (int) field.GetValue(this);

			EditorGUI.EndProperty();
		}

		FieldInfo[] ReturnConst(Type t)
		{
			ArrayList constants = new ArrayList();

			FieldInfo[] fieldInfos = t.GetFields(
				BindingFlags.Public | BindingFlags.Static |
				BindingFlags.FlattenHierarchy);


			foreach (FieldInfo fi in fieldInfos)

				if (fi.IsLiteral && !fi.IsInitOnly)
					constants.Add(fi);

			return (FieldInfo[]) constants.ToArray(typeof(FieldInfo));
		}
	}
}