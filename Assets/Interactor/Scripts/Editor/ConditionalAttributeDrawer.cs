using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace razz
{
    [CustomPropertyDrawer(typeof(ConditionalAttribute), true)]
    public class ConditionalAttributeDrawer : PropertyDrawer
    {
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
            var conditionalAttribute = this.attribute as ConditionalAttribute;
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
            var conditionalAttribute = this.attribute as ConditionalAttribute;

            if (!meetsCondition && conditionalAttribute.Action ==
                                           Condition.Show)
                return 0;
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent
               label)
        {
            bool meetsCondition = MeetsConditions(property);
            if (meetsCondition)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var conditionalAttribute = this.attribute as ConditionalAttribute;
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
