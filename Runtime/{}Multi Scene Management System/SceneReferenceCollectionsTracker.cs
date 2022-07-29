using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace PixLi
{
	[CreateAssetMenu(fileName = "[Scene Reference Collections Tracker]", menuName = "[Scene Reference]/[Scene Reference Collections Tracker]", order = 199)]
	public class SceneReferenceCollectionsTracker : ScriptableObjectSingleton<SceneReferenceCollectionsTracker>
	{
		[SerializeField] private List<SceneReferenceCollection> _sceneReferenceCollections;

		public int _SceneReferenceCollectionsQuantity => this._sceneReferenceCollections.Count;

		public int ActiveSceneReferenceCollectionIndex_ { get; private set; }
		public SceneReferenceCollection _ActiveSceneReferenceCollection => this._sceneReferenceCollections[this.ActiveSceneReferenceCollectionIndex_];

		public bool SetActive(SceneReferenceCollection sceneReferenceCollection)
		{
			for (int a = 0; a < this._sceneReferenceCollections.Count; a++)
			{
				if (this._sceneReferenceCollections[a] == sceneReferenceCollection)
				{
					this.ActiveSceneReferenceCollectionIndex_ = a;

					return true;
				}
			}

			this.ActiveSceneReferenceCollectionIndex_ = -1;

#if GLOBAL_DEBUG
			Debug.LogError(message: $"Scene that was set active has index of ${this.ActiveSceneReferenceCollectionIndex_}. Scene wasn't added to tracker beforehand.\nIf you used SceneController to load a scene reference collection at runtime then it's recommended that you add scene to the tracker list, it is recommended since some functions like loading `Next` or `Previous` scenes will most likely give you errors or not work entirely.");
#endif

			return false;
		}

		public SceneReferenceCollection GetSceneReferenceCollection(int index) => this._sceneReferenceCollections[index];

		public bool RemoveSceneReferenceCollection(SceneReferenceCollection sceneReferenceCollection)
		{
			for (int a = 0; a < this._sceneReferenceCollections.Count; a++)
			{
				if (this._sceneReferenceCollections[a] == sceneReferenceCollection)
				{
					this._sceneReferenceCollections.RemoveAt(index: a);

#if GLOBAL_DEBUG
					if (a == this.ActiveSceneReferenceCollectionIndex_)
						Debug.LogError(message: "Active scene was removed from Scene Reference Collections Tracker. This may lead to errors, especially when loading scenes.");
#endif

					return true;
				}
			}

#if GLOBAL_DEBUG
			Debug.LogWarning(message: "The scene reference that you were trying to remove hasn't been added to Scene Reference Collections Tracker.");
#endif
			return false;
		}

		public void AddSceneReferenceCollection(SceneReferenceCollection sceneReferenceCollection) => this._sceneReferenceCollections.Add(item: sceneReferenceCollection);

#if UNITY_EDITOR
		protected override string GetInstanceDirectoryPath() => PathUtility.GetScriptFileDirectoryPath();
#endif
	}
}