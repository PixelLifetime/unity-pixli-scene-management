using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

using Unity.EditorCoroutines.Editor;
#endif

[CreateAssetMenu(fileName = "[Scene Reference Collection]", menuName = "[Scene Reference Collection]", order = 199)]
public class SceneReferenceCollection : ScriptableObject
{
	[SerializeField] private SceneReference[] _scenesReferences;
	public SceneReference[] _ScenesReferences => this._scenesReferences;
	
	[SerializeField] private SceneReferenceCollection[] _sceneReferenceCollections;
	public SceneReferenceCollection[] _SceneReferenceCollections => this._sceneReferenceCollections;

#if UNITY_EDITOR
	private static IEnumerator OpenSceneSingleProcess(SceneReferenceCollection sceneReferenceCollection)
	{
		Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		for (int a = 0; a < sceneReferenceCollection._scenesReferences.Length; a++)
		{
			EditorSceneManager.OpenScene(sceneReferenceCollection._scenesReferences[a].ScenePath, OpenSceneMode.Additive);
		}

		for (int a = 0; a < sceneReferenceCollection._sceneReferenceCollections.Length; a++)
		{
			SceneReferenceCollection.OpenScene(sceneReferenceCollection._sceneReferenceCollections[a], OpenSceneMode.Additive);
		}

		yield return null;

		EditorSceneManager.CloseScene(scene, true);
	}

	private static void OpenScene(SceneReferenceCollection sceneReferenceCollection, OpenSceneMode openSceneMode)
	{
		switch (openSceneMode)
		{
			case OpenSceneMode.Single:

				EditorCoroutineUtility.StartCoroutineOwnerless(SceneReferenceCollection.OpenSceneSingleProcess(sceneReferenceCollection));

				break;
			case OpenSceneMode.Additive:

				for (int a = 0; a < sceneReferenceCollection._scenesReferences.Length; a++)
				{
					EditorSceneManager.OpenScene(sceneReferenceCollection._scenesReferences[a].ScenePath, OpenSceneMode.Additive);
				}

				for (int a = 0; a < sceneReferenceCollection._sceneReferenceCollections.Length; a++)
				{
					SceneReferenceCollection.OpenScene(sceneReferenceCollection._sceneReferenceCollections[a], OpenSceneMode.Additive);
				}

				break;
			//case OpenSceneMode.AdditiveWithoutLoading:
			//	break;
		}
	}

	[OnOpenAsset(0)]
	public static bool OnOpenAsset(int instanceID, int line)
	{
		Object @object = EditorUtility.InstanceIDToObject(instanceID);

		if (@object is SceneReferenceCollection sceneReferenceCollection)
		{
			SceneReferenceCollection.OpenScene(sceneReferenceCollection, OpenSceneMode.Single);

			return true;
		}

		return false;
	}
#endif
}