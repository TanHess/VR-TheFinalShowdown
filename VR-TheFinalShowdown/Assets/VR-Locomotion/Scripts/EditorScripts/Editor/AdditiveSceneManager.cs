using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class AdditiveSceneManager : EditorWindow
{
    SceneAsset PersistentScene;
    [SerializeField] List<SceneAsset> AdditiveScenes = new List<SceneAsset>();

    [MenuItem("Scene Manager/Additive Scene Manager")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(AdditiveSceneManager),false,"Additive Scene Manager");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label("Place your Persistent Scene here.", EditorStyles.boldLabel);
        GUILayout.Label("This should contain your Player Rig and any scene loading components.", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();

        PersistentScene = EditorGUILayout.ObjectField("Base Scene", PersistentScene, typeof(SceneAsset), false) as SceneAsset;

        EditorGUILayout.Space();

        GUILayout.Label("Place any scenes you'd like to test in-editor here.", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty ListProperty = so.FindProperty("AdditiveScenes");

        EditorGUILayout.PropertyField(ListProperty, true);

        for (int i = 0; i < AdditiveScenes.Count; i++)
        {
            if (AdditiveScenes[i] == null)
            {
                return;
            }

            if (GUILayout.Button("Open " + AdditiveScenes[i].name, GUILayout.Width(250), GUILayout.Height(50)))
            {
                string BaseScenePath = AssetDatabase.GetAssetPath(PersistentScene);
                string AdditiveScenePath = AssetDatabase.GetAssetPath(AdditiveScenes[i]);
                EditorSceneManager.OpenScene(BaseScenePath, OpenSceneMode.Single);
                EditorSceneManager.OpenScene(AdditiveScenePath, OpenSceneMode.Additive);
            }

        }

        so.ApplyModifiedProperties();

    }

}