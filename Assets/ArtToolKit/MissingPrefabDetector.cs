#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ArtToolKit
{
	public class MissingPrefabDetector
	{

		public static List<string> CheckMissingPrefab(string checkPath)
		{
			List<string> scenePaths = new List<string>();

			string[] allScenes = ArtEditorUtil.GetAllScenes(checkPath);
			bool hasMissing = false;


			for (int i = 0; i < allScenes.Length; i++)
			{
				var scenePath = allScenes[i];


				var scene = EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
				var sceneRootGos = scene.GetRootGameObjects();
				for (int j = 0; j < sceneRootGos.Length; j++)
				{
					var sceneRootGo = sceneRootGos[j];
					scenePaths.AddRange(GetMissPrefabChildernTrans(sceneRootGo.transform, scenePath));
				}
			}
			
			return scenePaths;
		}
		

		public static List<string> GetMissPrefabChildernTrans(Transform tran,string scenePath)
		{
			List<string> scenePaths = new List<string>();

			
			Transform[] childTrans = tran.GetComponentsInChildren<Transform>(true);
			Transform childTran;

			for (int i = 0; i < childTrans.Length; i++)
			{
				childTran = childTrans[i];
				if (IsFindMissingPrefabInScene(childTran))
				{
					if (!scenePaths.Contains(scenePath))
					{
						scenePaths.Add(scenePath);
					}
				}
			}
			
			return scenePaths;
			
		}


		static bool IsFindMissingPrefabInScene(Transform g)
		{
			if (g.name.Contains("Missing Prefab"))
			{
				return true;
			}

			if (PrefabUtility.IsPrefabAssetMissing(g))
			{
				return true;
			}

			if (PrefabUtility.IsDisconnectedFromPrefabAsset(g))
			{
				return true;
			}

			return false;
		}

		
	}
}
#endif