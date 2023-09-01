#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtToolKit
{
	/// <summary>
	/// 场景工具箱
	/// </summary>
	public class SceneEditorWindow
	{
		enum TabType
		{
			MissPrefabCheck,
			NoSameScaleCheck,
			PrefabPeplace,
			FindRefNoInScene,
		}

		public class Type
		{
			public static GUIContent[] m_tabs;
			public static GUIContent findPath;

			public static GUIContent minScale;
			public static GUIContent maxScale;
			public static GUIContent uniformScale;
			public static GUIContent prefabPreset;
			public static string[] checkSceneTypeStr = {"当前场景", "自定义场景"};
		}

		public class Parm
		{
			public static string m_FindPathStr = String.Empty;
			public static float minScale = 0f;
			public static float maxScale = 100f;
			public static float uniformScale = 1f;
			public static GameObject prefabPreset = null;
			public static int listCount = 0;
			public static List<Object> objList = new List<Object>();
			public static bool folder = true;
			public static int checkSceneTypeIndex = 0;
			public static CheckSceneType checkSceneType = CheckSceneType.CurScene;
		}

		protected Color oldColor;
		private TabType tabType = TabType.MissPrefabCheck;
		Vector2 scrollPos = Vector2.one;

		private List<Object> noReferencedByObjs = new List<Object>();
		private List<string> missPrefabScenes = new List<string>();
		private List<string> failFoliageDataCheckScenes = new List<string>();
		private List<string> probeSetScenes = new List<string>();

		private List<Object> selectionOBjs = new List<Object>();
		List<GameObject> noSameScaleGos = new List<GameObject>();

		public void Clear()
		{
			noReferencedByObjs.Clear();
			missPrefabScenes.Clear();
			failFoliageDataCheckScenes.Clear();
			probeSetScenes.Clear();
			selectionOBjs.Clear();
			FindReferencesNoSceneTool.Clear();
		}

		public void Init()
		{
			oldColor = GUI.color;
			Type.m_tabs = new GUIContent[]
			{
				new GUIContent("Missing Prefab检查"),
				new GUIContent("非等比缩放检查"),
				new GUIContent("替换Prefab"),
				new GUIContent("寻找当前场景未引用对象"),
			};

			Type.findPath = new GUIContent("查找路径:");
			Type.minScale = new GUIContent("最小缩放值:");
			Type.maxScale = new GUIContent("最大缩放值:");
			Type.uniformScale = new GUIContent("统一缩放值:");
			Type.prefabPreset = new GUIContent("Prefab模板:");
		}

		public void DrawSceneEditorWindow()
		{
			if (tabType == null || Type.m_tabs == null)
			{
				return;
			}

			tabType = (TabType) GUILayout.Toolbar((int) tabType, Type.m_tabs);

			switch (tabType)
			{
				case TabType.MissPrefabCheck:
					DrawMissPrefabCheckGUI();
					break;
				case TabType.NoSameScaleCheck:
					DrawNoSameScaleCheckGUI();
					break;
				case TabType.PrefabPeplace:
					DrawPrefabReplaceGUI();
					break;
				case TabType.FindRefNoInScene:
					DrawFindRefNoInSceneGUI();
					break;
			}
		}
		
		
		private void DrawFindRefNoInSceneGUI()
		{
			ArtEditorUtil.DocButton(DocURL.FindRefNoInScene);
			Parm.checkSceneTypeIndex = EditorGUILayout.Popup("检查方式：", Parm.checkSceneTypeIndex, Type.checkSceneTypeStr);

			Parm.checkSceneType = (CheckSceneType) Parm.checkSceneTypeIndex;

			if (Parm.checkSceneType == CheckSceneType.CustomScene)
			{
				Parm.listCount = EditorGUILayout.DelayedIntField("检查场景数量：", Parm.listCount);
				for (int i = 0; i < Parm.listCount; i++)
				{
					if (Parm.objList.Count < Parm.listCount)
					{
						Parm.objList.Add(null);
					}
					else if (Parm.objList.Count > Parm.listCount)
					{
						Parm.objList.RemoveAt(Parm.objList.Count - 1);
					}
				}

				Parm.folder = EditorGUILayout.Foldout(Parm.folder, "检查场景");
				if (Parm.folder)
				{
					for (int i = 0; i < Parm.listCount; i++)
					{
						Parm.objList[i] =
							EditorGUILayout.ObjectField(new GUIContent($"场景{i + 1}"), Parm.objList[i], typeof(Object));
					}
				}
			}


			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				if (GUILayout.Button("加载选中的对象"))
				{
					selectionOBjs.Clear();
					for (int i = 0; i < Selection.objects.Length; i++)
					{
						var obj = Selection.objects[i];
						if (obj != null)
						{
							if (!selectionOBjs.Contains(obj))
							{
								selectionOBjs.Add(obj);
							}
						}
					}
				}

				if (Parm.checkSceneType == CheckSceneType.CurScene)
				{
					if (GUILayout.Button("寻找当前场景未引用对象"))
					{
						noReferencedByObjs.Clear();
						noReferencedByObjs =
							FindReferencesNoSceneTool.FindNoReferencesInSceneAsset(selectionOBjs.ToArray());
					}
				}
				else if (Parm.checkSceneType == CheckSceneType.CustomScene)
				{
					if (GUILayout.Button("寻找场景列表未引用对象"))
					{
						noReferencedByObjs.Clear();
						for (int i = 0; i < Parm.listCount; i++)
						{
							var scene = Parm.objList[i];
							scene = EditorGUILayout.ObjectField(new GUIContent($"检查场景{i + 1}"), scene, typeof(Object));
							var scenePath = AssetDatabase.GetAssetPath(scene);
							noReferencedByObjs.AddRange(
								FindReferencesNoSceneTool.FindNoReferencesInSceneAsset(selectionOBjs.ToArray(),
									scenePath));
						}
					}
				}

				if (GUILayout.Button("选中未引用对象"))
				{
					Selection.objects = noReferencedByObjs.ToArray();
				}
			}


			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
					GUILayout.Height(600));

				for (int i = 0; i < selectionOBjs.Count; i++)
				{
					var obj = selectionOBjs[i];
					if (obj != null)
					{
						if (noReferencedByObjs != null && noReferencedByObjs.Count > 0 &&
							noReferencedByObjs.Contains(obj))
						{
							GUI.color = Color.red;
						}

						if (GUILayout.Button(obj.name, GUILayout.Height(32)))
						{
							EditorGUIUtility.PingObject(obj);
						}

						GUI.color = oldColor;
					}
				}

				EditorGUILayout.EndScrollView();
			}
		}


		private void DrawPrefabReplaceGUI()
		{
			ArtEditorUtil.DocButton(DocURL.PrefabPeplace);
			Parm.prefabPreset =
				EditorGUILayout.ObjectField(Type.prefabPreset, Parm.prefabPreset, typeof(GameObject)) as
					GameObject;
			if (Parm.prefabPreset == null)
			{
				return;
			}

			if (GUILayout.Button("批量替换Prefab"))
			{
				BatchReplacePrefabWindow.BatchReplacePrefab(Parm.prefabPreset);
			}
		}


		private void DrawNoSameScaleCheckGUI()
		{
			ArtEditorUtil.DocButton(DocURL.NoSameScaleCheck);
			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				Parm.listCount = EditorGUILayout.DelayedIntField("排除目录数量：", Parm.listCount);
				for (int i = 0; i < Parm.listCount; i++)
				{
					if (Parm.objList.Count < Parm.listCount)
					{
						Parm.objList.Add(null);
					}
					else if (Parm.objList.Count > Parm.listCount)
					{
						Parm.objList.RemoveAt(Parm.objList.Count - 1);
					}
				}

				Parm.folder = EditorGUILayout.Foldout(Parm.folder, "排除目录");
				if (Parm.folder)
				{
					for (int i = 0; i < Parm.listCount; i++)
					{
						Parm.objList[i] =
							EditorGUILayout.ObjectField(new GUIContent($"目录{i + 1}"), Parm.objList[i],
								typeof(Object));
					}
				}
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				Parm.minScale = EditorGUILayout.FloatField(Type.minScale, Parm.minScale);
				Parm.maxScale = EditorGUILayout.FloatField(Type.maxScale, Parm.maxScale);

				if (GUILayout.Button("缩放范围检查(超出范围的列出)"))
				{
					noSameScaleGos.Clear();

					noSameScaleGos = SameScaleCheckTool.NoRangeScaleCheck(Parm.minScale, Parm.maxScale);

					for (int i = 0; i < Parm.objList.Count; i++)
					{
						var excludeObj = Parm.objList[i] as GameObject;
						var excludeTrans = excludeObj.GetComponentsInChildren<Transform>();
						for (int j = 0; j < excludeTrans.Length; j++)
						{
							var excludeTran = excludeTrans[j];
							if (noSameScaleGos.Contains(excludeTran.gameObject))
							{
								noSameScaleGos.Remove(excludeTran.gameObject);
							}
						}
					}

					Selection.objects = null;
					Selection.objects = noSameScaleGos.ToArray();
				}
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				// 输入框
				Parm.uniformScale = EditorGUILayout.FloatField(Type.uniformScale, Parm.uniformScale);
				if (GUILayout.Button("设置选中对象缩放"))
				{
					SetObjsUniformScale(Parm.uniformScale);
				}
			}


			if (GUILayout.Button("非等比缩放检查"))
			{
				noSameScaleGos.Clear();

				noSameScaleGos = SameScaleCheckTool.NoSameScaleCheck();

				for (int i = 0; i < Parm.objList.Count; i++)
				{
					var excludeObj = Parm.objList[i] as GameObject;
					var excludeTrans = excludeObj.GetComponentsInChildren<Transform>();
					for (int j = 0; j < excludeTrans.Length; j++)
					{
						var excludeTran = excludeTrans[j];
						if (noSameScaleGos.Contains(excludeTran.gameObject))
						{
							noSameScaleGos.Remove(excludeTran.gameObject);
						}
					}
				}

				Selection.objects = null;
				Selection.objects = noSameScaleGos.ToArray();
			}

			if (noSameScaleGos.Count <= 0)
			{
				return;
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
					GUILayout.Height(600));

				for (int i = 0; i < noSameScaleGos.Count; i++)
				{
					var go = noSameScaleGos[i];

					DrawLog(go);
				}

				EditorGUILayout.EndScrollView();
			}
		}

		private void SetObjsUniformScale(float scaleValue)
		{
			var gos = Selection.gameObjects;
			for (int i = 0; i < gos.Length; i++)
			{
				var go = gos[i];
				go.transform.localScale = Vector3.one * scaleValue;
			}
		}

		private void DrawLog(GameObject go)
		{
			GUI.color = Color.red;
			if (GUILayout.Button(go.name))
			{
				EditorGUIUtility.PingObject(go);
			}

			GUI.color = oldColor;
		}

		private void DrawMissPrefabCheckGUI()
		{
			ArtEditorUtil.DocButton(DocURL.MissPrefabCheck);
			ArtEditorUtil.DrawFindPath(Type.findPath, ref Parm.m_FindPathStr);

			if (GUILayout.Button("查找Missing Prefab的场景"))
			{
				missPrefabScenes.Clear();
				missPrefabScenes = MissingPrefabDetector.CheckMissingPrefab(Parm.m_FindPathStr);
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
					GUILayout.Height(600));

				if (missPrefabScenes.Count > 0)
				{
					GUI.color = Color.red;

					GUILayout.Label("存在Missing Prefab的场景", new GUIStyle() {fontStyle = FontStyle.Bold, fontSize = 20});

					GUI.color = oldColor;

					for (int i = 0; i < missPrefabScenes.Count; i++)
					{
						var missPrefabScene = missPrefabScenes[i];
						Object scene = AssetDatabase.LoadAssetAtPath<Object>(missPrefabScene);
						if (scene != null)
						{
							GUI.color = Color.red;

							if (GUILayout.Button(scene.name, GUILayout.Height(32)))
							{
								EditorGUIUtility.PingObject(scene);
							}

							GUI.color = oldColor;
						}
					}
				}
				else
				{
					GUI.color = Color.green;

					GUILayout.Label("不存在Missing Prefab的场景", new GUIStyle() {fontStyle = FontStyle.Bold, fontSize = 20});

					GUI.color = oldColor;
				}


				EditorGUILayout.EndScrollView();
			}
		}


		public void SceneList(List<string> scenePaths, string failText, string successText)
		{
			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
					GUILayout.Height(600));

				if (scenePaths.Count > 0)
				{
					GUI.color = Color.red;

					GUILayout.Label(failText, new GUIStyle() {fontStyle = FontStyle.Bold, fontSize = 20});

					GUI.color = oldColor;

					for (int i = 0; i < scenePaths.Count; i++)
					{
						var scenePath = scenePaths[i];
						Object scene = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
						if (scene != null)
						{
							GUI.color = Color.red;

							if (GUILayout.Button(scene.name, GUILayout.Height(32)))
							{
								EditorGUIUtility.PingObject(scene);
							}

							GUI.color = oldColor;
						}
					}
				}
				else
				{
					GUI.color = Color.green;

					GUILayout.Label(successText, new GUIStyle() {fontStyle = FontStyle.Bold, fontSize = 20});

					GUI.color = oldColor;
				}


				EditorGUILayout.EndScrollView();
			}
		}
	}
}
#endif