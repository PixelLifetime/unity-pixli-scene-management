using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
#if UNITY_EDITOR
	private SceneAsset LoadSceneAssetAtPath()
	{
		if (string.IsNullOrEmpty(this._scenePath))
			return null;

		return AssetDatabase.LoadAssetAtPath<SceneAsset>(this._scenePath);
	}

	private string GetSceneAssetPath()
	{
		if (this._sceneAsset == null)
			return string.Empty;

		return AssetDatabase.GetAssetPath(this._sceneAsset);
	}

	[SerializeField] private SceneAsset _sceneAsset;
	public bool IsSceneAssetValid
	{
		get
		{
			if (this._sceneAsset == null)
				return false;

			return true;
		}
	}
#endif

	[SerializeField] private string _scenePath = string.Empty;

	public string ScenePath
	{
		get
		{
#if UNITY_EDITOR
			return this.GetSceneAssetPath();
#else
            return scenePath;
#endif
		}
		set
		{
			this._scenePath = value;
#if UNITY_EDITOR
			this._sceneAsset = this.LoadSceneAssetAtPath();
#endif
		}
	}

	public static implicit operator string(SceneReference sceneReference)
	{
		return sceneReference.ScenePath;
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		if (!this.IsSceneAssetValid && !string.IsNullOrEmpty(this._scenePath))
		{
			this._sceneAsset = this.LoadSceneAssetAtPath();

			if (this._sceneAsset == null)
				this._scenePath = string.Empty;

			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}
		else
		{
			this._scenePath = this.GetSceneAssetPath();
		}
#endif
	}

#if UNITY_EDITOR
	private void OnAfterDeserializeProcess()
	{
		EditorApplication.update -= this.OnAfterDeserializeProcess;

		if (this.IsSceneAssetValid)
			return;

		if (!string.IsNullOrEmpty(this._scenePath))
		{
			this._sceneAsset = this.LoadSceneAssetAtPath();

			if (this._sceneAsset == null)
				this._scenePath = string.Empty;

			if (Application.isPlaying == false)
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}
	}
#endif

	public void OnAfterDeserialize()
	{
#if UNITY_EDITOR
		EditorApplication.update += this.OnAfterDeserializeProcess;
#endif
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferencePropertyDrawer : PropertyDrawer
	{
		private static readonly RectOffset s_helpBoxPaddingRectOffset = EditorStyles.helpBox.padding;
		private static readonly float s_padding = 2.0f;
		private static readonly float s_footerHeight = 10.0f;

		private static readonly string s_sceneAssetPropertyName = "_sceneAsset";
		private static readonly string s_scenePathPropertyName = "_scenePath";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty sceneAssetSerializedProperty = property.FindPropertyRelative(relativePropertyPath: s_sceneAssetPropertyName);

			position.height -= s_footerHeight;
			GUI.Box(EditorGUI.IndentedRect(position), GUIContent.none, EditorStyles.helpBox);
			position = s_helpBoxPaddingRectOffset.Remove(position);
			position.height = EditorGUIUtility.singleLineHeight;


			label.tooltip = "The actual Scene Asset reference.\nOn serialize this is also stored as the asset's path.";

			EditorGUI.BeginProperty(position, GUIContent.none, property);
			EditorGUI.BeginChangeCheck();
			int sceneControlID = GUIUtility.GetControlID(FocusType.Passive);
			Object selectedObject = EditorGUI.ObjectField(position, label, sceneAssetSerializedProperty.objectReferenceValue, typeof(SceneAsset), false);
			BuildUtils.BuildScene buildScene = BuildUtils.GetBuildScene(selectedObject);

			if (EditorGUI.EndChangeCheck())
			{
				sceneAssetSerializedProperty.objectReferenceValue = selectedObject;

				if (buildScene.scene == null)
					property.FindPropertyRelative(relativePropertyPath: s_scenePathPropertyName).stringValue = string.Empty;
			}
			position.y += EditorGUIUtility.singleLineHeight + s_padding;

			if (buildScene.assetGUID.Empty() == false)
			{
				this.DrawSceneInfoGUI(position, buildScene, sceneControlID + 1);
			}

			EditorGUI.EndProperty();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			int lines = 2;

			SerializedProperty sceneAssetProperty = property.FindPropertyRelative("_sceneAsset");

			if (sceneAssetProperty.objectReferenceValue == null)
				lines = 1;

			return s_helpBoxPaddingRectOffset.vertical + EditorGUIUtility.singleLineHeight * lines + s_padding * (lines - 1) + s_footerHeight;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="buildScene"></param>
		/// <param name="sceneControlID"></param>
		private void DrawSceneInfoGUI(Rect position, BuildUtils.BuildScene buildScene, int sceneControlID)
		{
			bool readOnly = BuildUtils.IsReadOnly();
			string readOnlyWarning = readOnly ? "\n\nWARNING: Build Settings is not checked out and so cannot be modified." : "";

			GUIContent iconContent = new GUIContent();
			GUIContent labelContent = new GUIContent();

			// NOT in build settings.
			if (buildScene.buildIndex == -1)
			{
				iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close");
				labelContent.text = "NOT In Build";
				labelContent.tooltip = "This scene is NOT in build settings.\nIt will be NOT included in builds.";
			}
			else if (buildScene.scene.enabled)
			{
				iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max");
				labelContent.text = "BuildIndex: " + buildScene.buildIndex;
				labelContent.tooltip = "This scene is in build settings and ENABLED.\nIt will be included in builds." + readOnlyWarning;
			}
			else
			{
				iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min");
				labelContent.text = "BuildIndex: " + buildScene.buildIndex;
				labelContent.tooltip = "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
			}

			using (new EditorGUI.DisabledScope(readOnly))
			{
				Rect labelRect = DrawUtils.GetLabelRect(position);
				Rect iconRect = labelRect;

				iconRect.width = iconContent.image.width + s_padding;
				labelRect.width -= iconRect.width;
				labelRect.x += iconRect.width;

				EditorGUI.PrefixLabel(iconRect, sceneControlID, iconContent);
				EditorGUI.PrefixLabel(labelRect, sceneControlID, labelContent);
			}

			Rect buttonRect = DrawUtils.GetFieldRect(position);
			buttonRect.width = (buttonRect.width) / 3;

			string tooltipMsg = "";
			using (new EditorGUI.DisabledScope(readOnly))
			{
				// NOT in build settings.
				if (buildScene.buildIndex == -1)
				{
					buttonRect.width *= 2;
					int addIndex = EditorBuildSettings.scenes.Length;
					tooltipMsg = "Add this scene to build settings. It will be appended to the end of the build scenes as buildIndex: " + addIndex + "." + readOnlyWarning;
					if (DrawUtils.Button(buttonRect, "Add...", "Add (buildIndex " + addIndex + ")", EditorStyles.miniButtonLeft, tooltipMsg))
						BuildUtils.AddBuildScene(buildScene);
					buttonRect.width /= 2;
					buttonRect.x += buttonRect.width;
				}
				else
				{
					bool isEnabled = buildScene.scene.enabled;
					string stateString = isEnabled ? "Disable" : "Enable";
					tooltipMsg = stateString + " this scene in build settings.\n" + (isEnabled ? "It will no longer be included in builds" : "It will be included in builds") + "." + readOnlyWarning;

					if (DrawUtils.Button(buttonRect, stateString, stateString + " In Build", EditorStyles.miniButtonLeft, tooltipMsg))
						BuildUtils.SetBuildSceneState(buildScene, !isEnabled);
					buttonRect.x += buttonRect.width;

					tooltipMsg = "Completely remove this scene from build settings.\nYou will need to add it again for it to be included in builds!" + readOnlyWarning;
					if (DrawUtils.Button(buttonRect, "Remove...", "Remove from Build", EditorStyles.miniButtonMid, tooltipMsg))
						BuildUtils.RemoveBuildScene(buildScene);
				}
			}

			buttonRect.x += buttonRect.width;

			tooltipMsg = "Open the 'Build Settings' Window for managing scenes." + readOnlyWarning;
			if (DrawUtils.Button(buttonRect, "Settings", "Build Settings", EditorStyles.miniButtonRight, tooltipMsg))
			{
				// Open Build Settings window.
				EditorWindow.GetWindow(typeof(BuildPlayerWindow));
			}

		}

		private static class DrawUtils
		{
			/// <summary>
			/// 
			/// </summary>
			/// <param name="position"></param>
			/// <param name="msgShort"></param>
			/// <param name="msgLong"></param>
			/// <param name="style"></param>
			/// <param name="tooltip"></param>
			/// <returns></returns>
			public static bool Button(Rect position, string msgShort, string msgLong, GUIStyle style, string tooltip = null)
			{
				GUIContent content = new GUIContent(msgLong);
				content.tooltip = tooltip;

				float longWidth = style.CalcSize(content).x;

				if (longWidth > position.width)
					content.text = msgShort;

				return GUI.Button(position, content, style);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="position"></param>
			/// <returns></returns>
			public static Rect GetFieldRect(Rect position)
			{
				position.width -= EditorGUIUtility.labelWidth;
				position.x += EditorGUIUtility.labelWidth;

				return position;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="position"></param>
			/// <returns></returns>
			public static Rect GetLabelRect(Rect position)
			{
				position.width = EditorGUIUtility.labelWidth - s_padding;

				return position;
			}
		}

		private static class BuildUtils
		{
			public static float minCheckWait = 3;
			private static float lastTimeChecked = 0;
			private static bool cachedReadonlyVal = true;

			public struct BuildScene
			{
				public int buildIndex;
				public GUID assetGUID;
				public string assetPath;
				public EditorBuildSettingsScene scene;
			}

			public static bool IsReadOnly()
			{
				float curTime = Time.realtimeSinceStartup;
				float timeSinceLastCheck = curTime - lastTimeChecked;

				if (timeSinceLastCheck > minCheckWait)
				{
					lastTimeChecked = curTime;
					cachedReadonlyVal = QueryBuildSettingsStatus();
				}

				return cachedReadonlyVal;
			}

			private static bool QueryBuildSettingsStatus()
			{
				if (UnityEditor.VersionControl.Provider.enabled == false)
					return false;

				if (UnityEditor.VersionControl.Provider.hasCheckoutSupport == false)
					return false;

				UnityEditor.VersionControl.Task status = UnityEditor.VersionControl.Provider.Status("ProjectSettings/EditorBuildSettings.asset", false);
				status.Wait();

				if (status.assetList == null || status.assetList.Count != 1)
					return true;

				if (status.assetList[0].IsState(UnityEditor.VersionControl.Asset.States.CheckedOutLocal))
					return false;

				return true;
			}

			public static BuildScene GetBuildScene(Object sceneObject)
			{
				BuildScene entry = new BuildScene()
				{
					buildIndex = -1,
					assetGUID = new GUID(string.Empty)
				};

				if (sceneObject as SceneAsset == null)
					return entry;

				entry.assetPath = AssetDatabase.GetAssetPath(sceneObject);
				entry.assetGUID = new GUID(AssetDatabase.AssetPathToGUID(entry.assetPath));

				for (int index = 0; index < EditorBuildSettings.scenes.Length; ++index)
				{
					if (entry.assetGUID.Equals(EditorBuildSettings.scenes[index].guid))
					{
						entry.scene = EditorBuildSettings.scenes[index];
						entry.buildIndex = index;
						return entry;
					}
				}

				return entry;
			}

			public static void SetBuildSceneState(BuildScene buildScene, bool enabled)
			{
				bool modified = false;
				EditorBuildSettingsScene[] scenesToModify = EditorBuildSettings.scenes;
				foreach (var curScene in scenesToModify)
				{
					if (curScene.guid.Equals(buildScene.assetGUID))
					{
						curScene.enabled = enabled;
						modified = true;
						break;
					}
				}
				if (modified)
					EditorBuildSettings.scenes = scenesToModify;
			}

			public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true)
			{
				if (force == false)
				{
					int selection = EditorUtility.DisplayDialogComplex(
						"Add Scene To Build",
						"You are about to add scene at " + buildScene.assetPath + " To the Build Settings.",
						"Add as Enabled",       // option 0
						"Add as Disabled",      // option 1
						"Cancel (do nothing)"   // option 2
					);

					switch (selection)
					{
						case 0: // Enabled.
							enabled = true;
							break;
						case 1: // Disabled.
							enabled = false;
							break;
						default:
						case 2: // Cancel.
							return;
					}
				}

				EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(buildScene.assetGUID, enabled);
				List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();

				tempScenes.Add(newScene);

				EditorBuildSettings.scenes = tempScenes.ToArray();
			}
			
			public static void RemoveBuildScene(BuildScene buildScene, bool force = false)
			{
				bool onlyDisable = false;

				if (force == false)
				{
					int selection = -1;

					string title = "Remove Scene From Build";
					string details = string.Format(
						"You are about to remove the following scene from build settings:\n    {0}\n    buildIndex: {1}\n\n{2}",
						buildScene.assetPath,
						buildScene.buildIndex,
						"This will modify build settings, but the scene asset will remain untouched."
					);

					string confirm = "Remove From Build";
					string alt = "Just Disable";
					string cancel = "Cancel (do nothing)";

					if (buildScene.scene.enabled)
					{
						details += "\n\nIf you want, you can also just disable it instead.";
						selection = EditorUtility.DisplayDialogComplex(title, details, confirm, alt, cancel);
					}
					else
					{
						selection = EditorUtility.DisplayDialog(title, details, confirm, cancel) ? 0 : 2;
					}

					switch (selection)
					{
						case 0: // Remove.
							break;
						case 1: // Disable.
							onlyDisable = true;
							break;
						default:
						case 2: // Cancel.
							return;
					}
				}
				
				if (onlyDisable)
				{
					SetBuildSceneState(buildScene, false);
				}
				else
				{
					List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
					tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.assetGUID));

					EditorBuildSettings.scenes = tempScenes.ToArray();
				}
			}
		}
	}
#endif
}