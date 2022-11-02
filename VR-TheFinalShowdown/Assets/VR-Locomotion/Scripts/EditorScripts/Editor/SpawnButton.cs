using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player_Spawn_Location)), CanEditMultipleObjects]
class SpawnSetter : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        
        /*
        Player_Spawn_Location Spawner = (Player_Spawn_Location)target;
        Undo.RecordObject(Spawner, "Set Checkpoint");
        */

        Player_Spawn_Location Spawner = (Player_Spawn_Location)target;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty ListProperty = so.FindProperty("CurrentSpawnPoint");

        if (GUILayout.Button("Set Spawn Location Manually"))
        {
            Spawner.SetCheckpoint();
        }

        EditorGUILayout.PropertyField(ListProperty, true);
    }
}
