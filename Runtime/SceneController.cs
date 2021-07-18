using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;

//TODO: refactor
public class SceneController : MonoBehaviourSingleton<SceneController>
{
	[SerializeField] private UnityEvent<float> _onLoadingSceneAsync;
	public UnityEvent<float> _OnLoadingSceneAsync { get { return this._onLoadingSceneAsync; } }

	private IEnumerator LoadSceneAsyncProcess(int sceneBuildIndex, LoadSceneMode loadSceneMode, UnityEvent<float> onLoadingSceneAsync, Action onSceneLoaded)
	{
		AsyncOperation sceneLoadingAsyncOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, loadSceneMode);

		while (!sceneLoadingAsyncOperation.isDone)
		{
			this._onLoadingSceneAsync.Invoke(sceneLoadingAsyncOperation.progress);
			onLoadingSceneAsync?.Invoke(sceneLoadingAsyncOperation.progress);

			yield return null;
		}

		onSceneLoaded?.Invoke();
	}

	private IEnumerator LoadSceneAsyncProcess(string sceneName, LoadSceneMode loadSceneMode, UnityEvent<float> onLoadingSceneAsync, Action onSceneLoaded)
	{
		AsyncOperation sceneLoadingAsyncOperation = SceneManager.LoadSceneAsync(sceneName: sceneName, loadSceneMode);

		while (!sceneLoadingAsyncOperation.isDone)
		{
			this._onLoadingSceneAsync.Invoke(sceneLoadingAsyncOperation.progress);
			onLoadingSceneAsync?.Invoke(sceneLoadingAsyncOperation.progress);

			yield return null;
		}

		onSceneLoaded?.Invoke();
	}

	private Coroutine _loadSceneAsyncProcess;

	public void LoadSceneAsync(int sceneBuildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single, UnityEvent<float> onLoadingSceneAsync = null, Action onSceneLoaded = null)
	{
		if (this._loadSceneAsyncProcess != null)
			this.StopCoroutine(this._loadSceneAsyncProcess);

		this._loadSceneAsyncProcess = this.StartCoroutine(this.LoadSceneAsyncProcess(sceneBuildIndex, loadSceneMode, onLoadingSceneAsync, onSceneLoaded));
	}

	public void LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, UnityEvent<float> onLoadingSceneAsync = null, Action onSceneLoaded = null)
	{
		if (this._loadSceneAsyncProcess != null)
			this.StopCoroutine(this._loadSceneAsyncProcess);

		this._loadSceneAsyncProcess = this.StartCoroutine(this.LoadSceneAsyncProcess(sceneName, loadSceneMode, onLoadingSceneAsync, onSceneLoaded));
	}

	#region General

	public void LoadScene(int sceneBuildIndex, LoadSceneMode loadSceneMode)
	{
		SceneManager.LoadScene(sceneBuildIndex: sceneBuildIndex, mode: loadSceneMode);
	}
	public void LoadScene(int sceneBuildIndex) => this.LoadScene(sceneBuildIndex: sceneBuildIndex, loadSceneMode: LoadSceneMode.Single);

	#endregion

	#region Previous

	public void LoadPreviousScene(LoadSceneMode loadSceneMode)
	{
		int previousSceneBuildIndex = SceneManager.GetActiveScene().buildIndex - 1;

		if (previousSceneBuildIndex >= 0)
		{
			SceneManager.LoadScene(previousSceneBuildIndex, loadSceneMode);
		}
	}
	public void LoadPreviousScene() => this.LoadPreviousScene(LoadSceneMode.Single);

	//! Async
	public void LoadPreviousSceneAsync(LoadSceneMode loadSceneMode)
	{
		int previousSceneBuildIndex = SceneManager.GetActiveScene().buildIndex - 1;

		if (previousSceneBuildIndex >= 0)
		{
			this.LoadSceneAsync(previousSceneBuildIndex, loadSceneMode);
		}
	}
	public void LoadPreviousSceneAsync() => this.LoadPreviousSceneAsync(LoadSceneMode.Single);

	#endregion

	#region Next

	public void LoadNextScene(LoadSceneMode loadSceneMode)
	{
		int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

		if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(nextSceneBuildIndex, loadSceneMode);
		}
	}
	public void LoadNextScene() => this.LoadNextScene(LoadSceneMode.Single);

	//! Async
	public void LoadNextSceneAsync(LoadSceneMode loadSceneMode)
	{
		int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

		if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
		{
			this.LoadSceneAsync(nextSceneBuildIndex, loadSceneMode);
		}
	}
	public void LoadNextSceneAsync() => this.LoadNextSceneAsync(LoadSceneMode.Single);
	
	#endregion

	#region Active

	public void LoadActiveScene(LoadSceneMode loadSceneMode)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, loadSceneMode);
	}
	public void LoadActiveScene() => this.LoadActiveScene(LoadSceneMode.Single);

	//! Async
	public void LoadActiveSceneAsync(LoadSceneMode loadSceneMode)
	{
		this.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, loadSceneMode);
	}
	public void LoadActiveSceneAsync() => this.LoadActiveSceneAsync(LoadSceneMode.Single);
	
	#endregion

	public void Exit()
	{
		Application.Quit();
	}

#if UNITY_EDITOR
	//protected override void OnDrawGizmos()
	//{
	//}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneController))]
[CanEditMultipleObjects]
public class SceneControllerEditor : Editor
{
#pragma warning disable 0219, 414
	private SceneController _sSceneController;
#pragma warning restore 0219, 414

	private void OnEnable()
	{
		this._sSceneController = this.target as SceneController;
	}

	public override void OnInspectorGUI()
	{
		this.DrawDefaultInspector();
	}
}
#endif