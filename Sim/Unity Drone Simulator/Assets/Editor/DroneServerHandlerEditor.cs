using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DroneServerHandler))]
public class DroneServerHandlerEditor : Editor {
    public override void OnInspectorGUI() {
        DroneServerHandler dsh = (DroneServerHandler)target;

        EditorGUI.BeginChangeCheck();

        dsh.dronePrefab = (GameObject) EditorGUILayout.ObjectField("Drone Prefab", dsh.dronePrefab, typeof(GameObject), true);
        dsh.spawnTransform = (Transform)EditorGUILayout.ObjectField("Spawn Transform", dsh.spawnTransform, typeof(Transform), true);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Distance fitness scaler");
        dsh.distanceFitnessScaler = EditorGUILayout.Slider(dsh.distanceFitnessScaler, 0, 2f);
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Rotation fitness scaler");
        dsh.rotationFitnessScaler = EditorGUILayout.Slider(dsh.rotationFitnessScaler, 0, 2f);
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Smoothness fitness scaler");
        dsh.smoothnessFitnessScaler = EditorGUILayout.Slider(dsh.smoothnessFitnessScaler, 0, 2f);
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Airborne fitness scaler");
        dsh.airborneFitnessScaler = EditorGUILayout.Slider(dsh.airborneFitnessScaler, 0, 2f);
        EditorGUILayout.Separator();

        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
        }
    }
}
