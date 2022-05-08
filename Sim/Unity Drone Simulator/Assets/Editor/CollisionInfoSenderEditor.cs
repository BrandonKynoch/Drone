using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CollisionInfoSender))]
public class CollisionInfoSenderEditor : Editor {
    public override void OnInspectorGUI() {
        CollisionInfoSender cis = (CollisionInfoSender)target;

        if (cis.receivers == null)
            cis.receivers = new List<GameObject>();

        for (int i = 0; i < cis.receivers.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            cis.receivers[i] = ((GameObject) EditorGUILayout.ObjectField("Receiver " + (i + 1), cis.receivers[i], typeof(GameObject), true));
            
            if (cis.receivers[i] != null && cis.receivers[i].GetComponent<ICollisionInfoReciever>() == null)
                cis.receivers[i] = null;

            if (GUILayout.Button("X", GUILayout.Width(80f))) {
                cis.receivers.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+", GUILayout.Width(150f))) {
            cis.receivers.Add(null);
        }
        EditorGUILayout.EndHorizontal();
    }
}
