using System;
using UnityEditor;
using UnityEngine;

namespace razz
{
	[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
	public class EnumFlagsDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			EnumFlagsAttribute flagSettings = (EnumFlagsAttribute)attribute;
			Enum targetEnum = (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue);

			string propName = flagSettings.enumName;
			if (string.IsNullOrEmpty(propName))
				propName = ObjectNames.NicifyVariableName(property.name);

			EditorGUI.BeginChangeCheck();
			EditorGUI.BeginProperty(position, label, property);

			Enum enumNew = EditorGUI.EnumFlagsField(position, propName, targetEnum);

			if (!property.hasMultipleDifferentValues || EditorGUI.EndChangeCheck())
				property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}
