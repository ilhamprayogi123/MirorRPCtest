using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace razz
{
	[CustomEditor(typeof(Object), true, isFallback = true)]
	[CanEditMultipleObjects]
	public class EditorOverride : Editor
	{
		Dictionary<string, CacheFoldProp> cacheFolds = new Dictionary<string, CacheFoldProp>();
		List<SerializedProperty> props = new List<SerializedProperty>();
		List<MethodInfo> methods = new List<MethodInfo>();
		bool initialized;

		void OnEnable()
		{
			initialized = false;
		}

		void OnDisable()
		{
			//if (Application.wantsToQuit)
			//if (applicationIsQuitting) return;
			//	if (Toolbox.isQuittingOrChangingScene()) return;
			if (target != null)
				foreach (var c in cacheFolds)
				{
					EditorPrefs.SetBool(string.Format($"{"Interactor_Foldout_"}{c.Value.atr.name}{"_"}{c.Value.props[0].name}"), c.Value.expanded);
					c.Value.Dispose();
				}
		}

		public override bool RequiresConstantRepaint()
		{
			return EditorFramework.needToRepaint;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			Setup();

			if (props.Count == 0)
			{
				DrawDefaultInspector();
				return;
			}

			Header();
			Body();

			serializedObject.ApplyModifiedProperties();

			void Header()
			{
				using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
				{
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(props[0], true);
					EditorGUILayout.Space();
				}
			}

			void Body()
			{
				foreach (var pair in cacheFolds)
				{
					this.UseVerticalLayout(() => Foldout(pair.Value));
					EditorGUI.indentLevel = 0;
				}

				EditorGUILayout.Space();

				for (var i = 1; i < props.Count; i++)
				{
					EditorGUILayout.PropertyField(props[i], true);
				}

				EditorGUILayout.Space();

				if (methods == null) return;
				foreach (MethodInfo memberInfo in methods)
				{
					this.UseButton(memberInfo);
				}
			}

			void Foldout(CacheFoldProp cache)
			{
				cache.expanded = EditorGUILayout.Foldout(cache.expanded, cache.atr.name, true);

				if (cache.expanded)
				{
					EditorGUI.indentLevel = 1;

					for (int i = 0; i < cache.props.Count; i++)
					{
						this.UseVerticalLayout(() => Child(i));
					}
				}

				void Child(int i)
				{
					EditorGUILayout.PropertyField(cache.props[i], new GUIContent(ObjectNames.NicifyVariableName(cache.props[i].name)), true);
				}
			}

			void Setup()
			{
				EditorFramework.currentEvent = Event.current;
				if (!initialized)
				{
					List<FieldInfo> objectFields;
					FoldoutAttribute prevFold = default;

					var length = EditorTypes.Get(target, out objectFields);

					for (var i = 0; i < length; i++)
					{
						#region Folders
						var fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
						CacheFoldProp c;
						if (fold == null)
						{
							if (prevFold != null && prevFold.foldEverything)
							{
								if (!cacheFolds.TryGetValue(prevFold.name, out c))
								{
									cacheFolds.Add(prevFold.name, new CacheFoldProp { atr = prevFold, types = new HashSet<string> { objectFields[i].Name } });
								}
								else
								{
									c.types.Add(objectFields[i].Name);
								}
							}
							continue;
						}

						prevFold = fold;

						if (!cacheFolds.TryGetValue(fold.name, out c))
						{
							var expanded = EditorPrefs.GetBool(string.Format($"{"Interactor_Foldout_"}{fold.name}{"_"}{objectFields[i].Name}"), false);
							cacheFolds.Add(fold.name, new CacheFoldProp { atr = fold, types = new HashSet<string> { objectFields[i].Name }, expanded = expanded });
						}
						else c.types.Add(objectFields[i].Name);
						#endregion
					}

					var property = serializedObject.GetIterator();
					var next = property.NextVisible(true);
					if (next)
					{
						do
						{
							HandleFoldProp(property);
						} while (property.NextVisible(false));
					}

					initialized = true;
				}
			}
		}

		public void HandleFoldProp(SerializedProperty prop)
		{
			bool shouldBeFolded = false;

			foreach (var pair in cacheFolds)
			{
				if (pair.Value.types.Contains(prop.name))
				{
					var pr = prop.Copy();
					shouldBeFolded = true;
					pair.Value.props.Add(pr);

					break;
				}
			}

			if (shouldBeFolded == false)
			{
				var pr = prop.Copy();
				props.Add(pr);
			}
		}

		class CacheFoldProp
		{
			public HashSet<string> types = new HashSet<string>();
			public List<SerializedProperty> props = new List<SerializedProperty>();
			public FoldoutAttribute atr;
			public bool expanded;

			public void Dispose()
			{
				props.Clear();
				types.Clear();
				atr = null;
			}
		}
	}

	static class EditorUIHelper
	{
		public static void UseVerticalLayout(this Editor e, Action action)
		{
			EditorGUILayout.BeginVertical();
			action();
			EditorGUILayout.EndVertical();
		}

		public static void UseButton(this Editor e, MethodInfo m)
		{
			if (GUILayout.Button(m.Name))
			{
				m.Invoke(e.target, null);
			}
		}
	}

	static class StyleFramework
	{
		public static string FirstLetterToUpperCase(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return string.Empty;

			var a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}

		public static IList<Type> GetTypeTree(this Type t)
		{
			var types = new List<Type>();
			while (t.BaseType != null)
			{
				types.Add(t);
				t = t.BaseType;
			}
			return types;
		}
	}

	static class EditorTypes
	{
		public static Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>(FastComparable.Default);

		public static int Get(Object target, out List<FieldInfo> objectFields)
		{
			var t = target.GetType();
			var hash = t.GetHashCode();

			if (!fields.TryGetValue(hash, out objectFields))
			{
				var typeTree = t.GetTypeTree();
				objectFields = target.GetType()
						.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
						.OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
						.ToList();
				fields.Add(hash, objectFields);
			}
			return objectFields.Count;
		}
	}


	class FastComparable : IEqualityComparer<int>
	{
		public static FastComparable Default = new FastComparable();

		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj.GetHashCode();
		}
	}

	[InitializeOnLoad]
	public static class EditorFramework
	{
		internal static bool needToRepaint;

		internal static Event currentEvent;
		internal static float t;

		static EditorFramework()
		{
			EditorApplication.update += Updating;
		}

		static void Updating()
		{
			CheckMouse();

			if (needToRepaint)
			{
				t += Time.deltaTime;

				if (t >= 0.3f)
				{
					t -= 0.3f;
					needToRepaint = false;
				}
			}
		}

		static void CheckMouse()
		{
			var ev = currentEvent;
			if (ev == null) return;

			if (ev.type == EventType.MouseMove)
				needToRepaint = true;
		}
	}
}
