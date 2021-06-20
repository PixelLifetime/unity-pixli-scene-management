using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;

//TODO: think if there is any reason to keep this as MonoBehaviour.
public class SceneOperator : MonoBehaviour
{
	[SerializeField] private UnityEvent<float> _onLoadingSceneAsync;
	public UnityEvent<float> _OnLoadingSceneAsync { get { return this._onLoadingSceneAsync; } }

	public void LoadSceneAsync(int sceneBuildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single) => SceneController._Instance.LoadSceneAsync(sceneBuildIndex, loadSceneMode, this._onLoadingSceneAsync);

	#region Previous
	public void LoadPreviousScene(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadPreviousScene(loadSceneMode);
	public void LoadPreviousScene() => SceneController._Instance.LoadPreviousScene();

	//! Async
	public void LoadPreviousSceneAsync(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadPreviousSceneAsync(loadSceneMode);
	public void LoadPreviousSceneAsync() => SceneController._Instance.LoadPreviousSceneAsync();
	#endregion

	#region Active
	public void LoadActiveScene(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadActiveScene(loadSceneMode);
	public void LoadActiveScene() => SceneController._Instance.LoadActiveScene();

	//! Async		
	public void LoadActiveSceneAsync(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadActiveSceneAsync(loadSceneMode);
	public void LoadActiveSceneAsync() => SceneController._Instance.LoadActiveSceneAsync();
	#endregion

	#region Next
	public void LoadNextScene(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadNextScene(loadSceneMode);
	public void LoadNextScene() => this.LoadNextScene(LoadSceneMode.Single);

	//! Async
	public void LoadNextSceneAsync(LoadSceneMode loadSceneMode) => SceneController._Instance.LoadNextSceneAsync(loadSceneMode);
	public void LoadNextSceneAsync() => this.LoadNextSceneAsync(LoadSceneMode.Single);
	#endregion

	public void Exit() => SceneController._Instance.Exit();

#if UNITY_EDITOR
	//protected override void OnDrawGizmos()
	//{
	//}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneOperator))]
[CanEditMultipleObjects]
public class SceneOperatorEditor : Editor
{
#pragma warning disable 0219, 414
	private SceneOperator _sSceneOperator;
#pragma warning restore 0219, 414

	private void OnEnable()
	{
		this._sSceneOperator = this.target as SceneOperator;
	}

	public override void OnInspectorGUI()
	{
		this.DrawDefaultInspector();
	}
}
#endif