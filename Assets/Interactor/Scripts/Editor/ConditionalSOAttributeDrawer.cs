using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace razz
{
	[CustomPropertyDrawer(typeof(ConditionalSOAttribute), true)]
	public class ConditionalSOAttributeDrawer : PropertyDrawer
	{
		const int buttonWidth = 66;
		static readonly List<string> ignoreClassFullNames = new List<string> { "TMPro.TMP_FontAsset" };

		#region Reflection helpers.
		private static MethodInfo GetMethod(object target, string methodName)
		{
			return GetAllMethods(target, m => m.Name.Equals(methodName,
					  StringComparison.InvariantCulture)).FirstOrDefault();
		}

		private static FieldInfo GetField(object target, string fieldName)
		{
			return GetAllFields(target, f => f.Name.Equals(fieldName,
				  StringComparison.InvariantCulture)).FirstOrDefault();
		}
		private static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo,
				bool> predicate)
		{
			List<Type> types = new List<Type>()
			{
				target.GetType()
			};

			while (types.Last().BaseType != null)
			{
				types.Add(types.Last().BaseType);
			}

			for (int i = types.Count - 1; i >= 0; i--)
			{
				IEnumerable<FieldInfo> fieldInfos = types[i]
					.GetFields(BindingFlags.Instance | BindingFlags.Static |
	   BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
					.Where(predicate);

				foreach (var fieldInfo in fieldInfos)
				{
					yield return fieldInfo;
				}
			}
		}
		private static IEnumerable<MethodInfo> GetAllMethods(object target,
	  Func<MethodInfo, bool> predicate)
		{
			IEnumerable<MethodInfo> methodInfos = target.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Static |
	  BindingFlags.NonPublic | BindingFlags.Public)
				.Where(predicate);

			return methodInfos;
		}
		#endregion

		private bool MeetsConditions(SerializedProperty property)
		{
			var conditionalAttribute = this.attribute as ConditionalSOAttribute;
			var target = property.serializedObject.targetObject;
			List<bool> conditionValues = new List<bool>();

			foreach (var condition in conditionalAttribute.Conditions)
			{
				FieldInfo conditionField = GetField(target, condition);
				if (conditionField != null &&
					conditionField.FieldType == typeof(bool))
				{
					conditionValues.Add((bool)conditionField.GetValue(target));
				}

				MethodInfo conditionMethod = GetMethod(target, condition);
				if (conditionMethod != null &&
					conditionMethod.ReturnType == typeof(bool) &&
					conditionMethod.GetParameters().Length == 0)
				{
					conditionValues.Add((bool)conditionMethod.Invoke(target, null));
				}
			}

			if (conditionValues.Count > 0)
			{
				bool met;
				if (conditionalAttribute.Operator == Op.And)
				{
					met = true;
					foreach (var value in conditionValues)
					{
						met = met && value;
					}
				}
				else
				{
					met = false;
					foreach (var value in conditionValues)
					{
						met = met || value;
					}
				}
				return met;
			}
			else
			{
				Debug.LogError("Invalid boolean condition fields or methods used!");
				return true;
			}
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent
					 label)
		{
			bool meetsCondition = MeetsConditions(property);
			var conditionalAttribute = this.attribute as ConditionalSOAttribute;

			if (!meetsCondition && conditionalAttribute.Action ==
										   Condition.Show)
				return 0;

			float totalHeight = EditorGUIUtility.singleLineHeight;
			if (property.objectReferenceValue == null || !AreAnySubPropertiesVisible(property))
			{
				return totalHeight;
			}
			if (property.isExpanded)
			{
				var data = property.objectReferenceValue as ScriptableObject;
				if (data == null) return EditorGUIUtility.singleLineHeight;
				SerializedObject serializedObject = new SerializedObject(data);
				SerializedProperty prop = serializedObject.GetIterator();
				if (prop.NextVisible(true))
				{
					do
					{
						if (prop.name == "m_Script") continue;
						var subProp = serializedObject.FindProperty(prop.name);
						float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
						totalHeight += height;
					}
					while (prop.NextVisible(false));
				}
				totalHeight += EditorGUIUtility.standardVerticalSpacing;
			}
			return totalHeight;
		}

		public static T _GUILayout<T>(string label, T objectReferenceValue, ref bool isExpanded) where T : ScriptableObject
		{
			return _GUILayout<T>(new GUIContent(label), objectReferenceValue, ref isExpanded);
		}

		public static T _GUILayout<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded) where T : ScriptableObject
		{
			Rect position = EditorGUILayout.BeginVertical();

			var propertyRect = Rect.zero;
			var guiContent = label;
			var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
			if (objectReferenceValue != null)
			{
				isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true);

				var indentedPosition = EditorGUI.IndentedRect(position);
				var indentOffset = indentedPosition.x - position.x;
				propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset, EditorGUIUtility.singleLineHeight);
			}
			else
			{
				foldoutRect.x += 12;
				EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true, EditorStyles.label);

				var indentedPosition = EditorGUI.IndentedRect(position);
				var indentOffset = indentedPosition.x - position.x;
				propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset - 60, EditorGUIUtility.singleLineHeight);
			}

			EditorGUILayout.BeginHorizontal();
			objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent(" "), objectReferenceValue, typeof(T), false) as T;

			if (objectReferenceValue != null)
			{

				EditorGUILayout.EndHorizontal();
				if (isExpanded)
				{
					DrawScriptableObjectChildFields(objectReferenceValue);
				}
			}
			else
			{
				if (GUILayout.Button("Create", GUILayout.Width(buttonWidth)))
				{
					string selectedAssetPath = "Assets/Interactor";
					var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
					if (newAsset != null)
					{
						objectReferenceValue = (T)newAsset;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			return objectReferenceValue;
		}

		static void DrawScriptableObjectChildFields<T>(T objectReferenceValue) where T : ScriptableObject
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical(GUI.skin.box);

			var serializedObject = new SerializedObject(objectReferenceValue);
			SerializedProperty prop = serializedObject.GetIterator();
			if (prop.NextVisible(true))
			{
				do
				{
					if (prop.name == "m_Script") continue;
					EditorGUILayout.PropertyField(prop, true);
				}
				while (prop.NextVisible(false));
			}
			if (GUI.changed)
				serializedObject.ApplyModifiedProperties();
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		public static T DrawScriptableObjectField<T>(GUIContent label, T objectReferenceValue, ref bool isExpanded) where T : ScriptableObject
		{
			Rect position = EditorGUILayout.BeginVertical();

			var propertyRect = Rect.zero;
			var guiContent = label;
			var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
			if (objectReferenceValue != null)
			{
				isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true);

				var indentedPosition = EditorGUI.IndentedRect(position);
				var indentOffset = indentedPosition.x - position.x;
				propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset, EditorGUIUtility.singleLineHeight);
			}
			else
			{
				foldoutRect.x += 12;
				EditorGUI.Foldout(foldoutRect, isExpanded, guiContent, true, EditorStyles.label);

				var indentedPosition = EditorGUI.IndentedRect(position);
				var indentOffset = indentedPosition.x - position.x;
				propertyRect = new Rect(position.x + EditorGUIUtility.labelWidth - indentOffset, position.y, position.width - EditorGUIUtility.labelWidth - indentOffset - 60, EditorGUIUtility.singleLineHeight);
			}

			EditorGUILayout.BeginHorizontal();
			objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent(" "), objectReferenceValue, typeof(T), false) as T;

			if (objectReferenceValue != null)
			{
				EditorGUILayout.EndHorizontal();
				if (isExpanded)
				{
					//TODO
				}
			}
			else
			{
				if (GUILayout.Button("Create", GUILayout.Width(buttonWidth)))
				{
					string selectedAssetPath = "Assets/Interactor";
					var newAsset = CreateAssetWithSavePrompt(typeof(T), selectedAssetPath);
					if (newAsset != null)
					{
						objectReferenceValue = (T)newAsset;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			return objectReferenceValue;
		}

		static ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
		{
			path = EditorUtility.SaveFilePanelInProject("Save Interaction Settings", type.Name + "Settings.asset", "asset", "Enter a file name for the settings asset.", path);
			if (path == "") return null;
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			EditorGUIUtility.PingObject(asset);
			return asset;
		}

		Type GetFieldType()
		{
			Type type = fieldInfo.FieldType;
			if (type.IsArray) type = type.GetElementType();
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
			return type;
		}

		static bool AreAnySubPropertiesVisible(SerializedProperty property)
		{
			var data = (ScriptableObject)property.objectReferenceValue;
			SerializedObject serializedObject = new SerializedObject(data);
			SerializedProperty prop = serializedObject.GetIterator();
			while (prop.NextVisible(true))
			{
				if (prop.name == "m_Script") continue;
				return true;
			}
			return false;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent
			   label)
		{
			bool meetsCondition = MeetsConditions(property);
			if (meetsCondition)
			{
				EditorGUI.BeginProperty(position, label, property);
				var type = GetFieldType();

				if (type == null || ignoreClassFullNames.Contains(type.FullName))
				{
					EditorGUI.PropertyField(position, property, label);
					EditorGUI.EndProperty();
					return;
				}

				ScriptableObject propertySO = null;
				if (!property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject)
				{
					propertySO = (ScriptableObject)property.serializedObject.targetObject;
				}

				var propertyRect = Rect.zero;
				var guiContent = new GUIContent(property.displayName);
				var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
				if (property.objectReferenceValue != null && AreAnySubPropertiesVisible(property))
				{
					property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true);
				}
				else
				{
					foldoutRect.x += 12;
					EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true, EditorStyles.label);
				}
				var indentedPosition = EditorGUI.IndentedRect(position);
				var indentOffset = indentedPosition.x - position.x;
				propertyRect = new Rect(position.x + (EditorGUIUtility.labelWidth - indentOffset), position.y, position.width - (EditorGUIUtility.labelWidth - indentOffset), EditorGUIUtility.singleLineHeight);

				if (propertySO != null || property.objectReferenceValue == null)
				{
					propertyRect.width -= buttonWidth;
				}

				EditorGUI.ObjectField(propertyRect, property, type, GUIContent.none);
				if (GUI.changed) property.serializedObject.ApplyModifiedProperties();

				var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

				if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
				{
					var data = (ScriptableObject)property.objectReferenceValue;

					if (property.isExpanded)
					{
						//GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, Screen.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

						EditorGUI.indentLevel++;
						SerializedObject serializedObject = new SerializedObject(data);

						SerializedProperty prop = serializedObject.GetIterator();
						float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
						if (prop.NextVisible(true))
						{
							do
							{
								if (prop.name == "m_Script") continue;
								float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
								EditorGUI.PropertyField(new Rect(position.x, y, position.width - buttonWidth, height), prop, true);
								y += height + EditorGUIUtility.standardVerticalSpacing;
							}
							while (prop.NextVisible(false));
						}
						if (GUI.changed)
							serializedObject.ApplyModifiedProperties();

						EditorGUI.indentLevel--;
					}
				}
				else
				{
					if (GUI.Button(buttonRect, "Create"))
					{
						string selectedAssetPath = "Assets/Interactor";
						if (property.serializedObject.targetObject is MonoBehaviour)
						{
							MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
							//selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
						}
						property.objectReferenceValue = CreateAssetWithSavePrompt(type, selectedAssetPath);
					}
				}
				property.serializedObject.ApplyModifiedProperties();
				EditorGUI.EndProperty();
				return;
			}

			var conditionalAttribute = this.attribute as ConditionalSOAttribute;
			if (conditionalAttribute.Action == Condition.Show)
			{
				return;
			}
			else if (conditionalAttribute.Action == Condition.Enable)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.PropertyField(position, property, label, true);
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}
