#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtToolKit
{
	/// <summary>
	/// 通用工具箱
	/// </summary>
	public class CommonEditorWindow
	{
		enum TabType
		{
			ReplaceSuffix,
			AddSuffix,
			AddTextureSizeSuffix,
			ClearMatTextureRef,
			FileSizeCalculation,
		}

		public class Type
		{
			public static GUIContent[] m_tabs;
			public static GUIContent findPath;
			public static GUIContent srcSuffix;
			public static GUIContent destSuffix;
			public readonly static string[] selectionTypeStr = {"选中文件夹操作", "选中对象操作"};
			public readonly static string[] assetTypeStr = {"全部", "贴图", "模型", "预制体", "场景"};
		}

		public class Parm
		{
			public static string m_FindPathStr = String.Empty;
			public static string srcSuffixStr = String.Empty;
			public static string destSuffixStr = String.Empty;

			public static SelectionType selectionType = SelectionType.Folder;
			public static int slectionTypeIndex = 0;

			public static AssetType assetType = AssetType.Texture;
			public static int AssetTypeIndex = 0;

		}

		private TabType tabType = TabType.ReplaceSuffix;
		private Vector2 scrollPos = Vector2.zero;
		private string log;
		protected Color oldColor;
		
		public void Clear()
		{
		}

		public void Init()
		{
			oldColor = GUI.color;
			
			Type.m_tabs = new GUIContent[]
			{
				new GUIContent("替换资源后缀"),
				new GUIContent("添加资源后缀"),
				new GUIContent("添加贴图分辨率后缀"),
				new GUIContent("清除材质贴图引用"),
				new GUIContent("计算贴图大小"),
			};

			Type.findPath = new GUIContent("查找路径:");
			Type.srcSuffix = new GUIContent("源后缀(如：_Stc):");
			Type.destSuffix = new GUIContent("目标后缀(如：_Pfb):");
			
		}


		public void DrawCommonEditorWindow()
		{
			if (tabType == null || Type.m_tabs == null)
			{
				return;
			}

			tabType = (TabType) GUILayout.Toolbar((int) tabType, Type.m_tabs);

			switch (tabType)
			{
				case TabType.ReplaceSuffix:
					DrawReplaceSuffixGUI();
					break;
				case TabType.AddSuffix:
					DrawAddSuffixGUI();
					break;
				case TabType.AddTextureSizeSuffix:
					DrawAddTextureSizeSuffixGUI();
					break;
				case TabType.ClearMatTextureRef:
					DrawClearMatTextureRefGUI();
					break;
				case TabType.FileSizeCalculation:
					DrawFileSizeCalculationGUI();
					break;
			}
		}

		private void DrawAddTextureSizeSuffixGUI()
		{
			ArtEditorUtil.DocButton(DocURL.AddTextureSizeSuffix);
			if (GUILayout.Button("选中对象添加贴图尺寸后缀"))
			{
				BatchTextureRename.AddTextureSizeSuffix();
			}
		}

		private void DrawAddSuffixGUI()
		{
			ArtEditorUtil.DocButton(DocURL.AddSuffix);

			Parm.slectionTypeIndex = EditorGUILayout.Popup("操作方式：", Parm.slectionTypeIndex, Type.selectionTypeStr);

			Parm.selectionType = (SelectionType) Parm.slectionTypeIndex;

			if (Parm.selectionType == SelectionType.Folder)
			{
				using (var z = new EditorGUILayout.HorizontalScope("Button"))
				{
					Parm.m_FindPathStr = EditorGUILayout.TextField(Type.findPath, Parm.m_FindPathStr);
					if (GUILayout.Button("更新路径"))
					{
						Parm.m_FindPathStr = ArtEditorUtil.RefreshFindPath(Selection.assetGUIDs[0]);
					}
				}
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				Parm.destSuffixStr = EditorGUILayout.TextField(Type.destSuffix, Parm.destSuffixStr);
				Parm.AssetTypeIndex = EditorGUILayout.Popup("资源类型：", Parm.AssetTypeIndex, Type.assetTypeStr);

				Parm.assetType = (AssetType) Parm.AssetTypeIndex;
			}

			if (GUILayout.Button("添加资源名后缀"))
			{
				if (Parm.selectionType == SelectionType.Folder)
				{
					ArtEditorUtil.AddSuffixs(Parm.m_FindPathStr, Parm.destSuffixStr,
						Parm.assetType);
				}
				else if (Parm.selectionType == SelectionType.Obj)
				{
					ArtEditorUtil.AddSuffixs(Parm.destSuffixStr,
						Parm.assetType);
				}
			}
		}

		private void DrawReplaceSuffixGUI()
		{
			ArtEditorUtil.DocButton(DocURL.ReplaceSuffix);
			Parm.slectionTypeIndex = EditorGUILayout.Popup("操作方式：", Parm.slectionTypeIndex, Type.selectionTypeStr);

			Parm.selectionType = (SelectionType) Parm.slectionTypeIndex;

			if (Parm.selectionType == SelectionType.Folder)
			{
				ArtEditorUtil.DrawFindPath(Type.findPath, ref Parm.m_FindPathStr);
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				Parm.srcSuffixStr = EditorGUILayout.TextField(Type.srcSuffix, Parm.srcSuffixStr);
				Parm.destSuffixStr = EditorGUILayout.TextField(Type.destSuffix, Parm.destSuffixStr);
				Parm.AssetTypeIndex = EditorGUILayout.Popup("资源类型：", Parm.AssetTypeIndex, Type.assetTypeStr);

				Parm.assetType = (AssetType) Parm.AssetTypeIndex;
			}

			if (GUILayout.Button("替换资源后缀"))
			{
				if (Parm.selectionType == SelectionType.Folder)
				{
					ArtEditorUtil.ReplaceSuffixs(Parm.m_FindPathStr, Parm.srcSuffixStr, Parm.destSuffixStr,
						Parm.assetType);
				}
				else if (Parm.selectionType == SelectionType.Obj)
				{
					ArtEditorUtil.ReplaceSuffixs(Parm.srcSuffixStr, Parm.destSuffixStr,
						Parm.assetType);
				}
			}
		}

		private void DrawClearMatTextureRefGUI()
		{
			ArtEditorUtil.DocButton(DocURL.ClearMatTextureRef);

			Parm.slectionTypeIndex = EditorGUILayout.Popup("操作方式：", Parm.slectionTypeIndex, Type.selectionTypeStr);
			Parm.selectionType = (SelectionType) Parm.slectionTypeIndex;

			if (Parm.selectionType == SelectionType.Folder)
			{
				ArtEditorUtil.DrawFindPath(Type.findPath, ref Parm.m_FindPathStr);
			}

			if (GUILayout.Button("清除材质所有贴图引用"))
			{
				ArtEditorUtil.ClearMatAllTextureRef(Parm.m_FindPathStr, Parm.selectionType);
				ArtEditorUtil.ShowTips("清除成功！");
			}
		}
		

		/// <summary>
		/// 绘制文件大小计算UI
		/// </summary>
		private void DrawFileSizeCalculationGUI()
		{
			ArtEditorUtil.DocButton(DocURL.FileSizeCalculation);
			if (GUILayout.Button("计算贴图大小"))
			{
				log = FileSizeDebug.FileSizeLog();
			}

			using (var z = new EditorGUILayout.VerticalScope("Button"))
			{
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false,
					GUILayout.Height(200));

				EditorGUILayout.LabelField(log);

				EditorGUILayout.EndScrollView();
			}
		}
	}
}
#endif