using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Drone))]
public class DroneEditor : Editor {
    public override void OnInspectorGUI() {
        Drone d = (Drone)target;

        EditorGUI.BeginChangeCheck();

        d.motorStrength = EditorGUILayout.FloatField("Motor Strength", d.motorStrength);

        d.bladesTransform = (Transform)EditorGUILayout.ObjectField("Blades Transform", d.bladesTransform, typeof(Transform), true);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Motor Outputs");
        EditorGUILayout.LabelField("FL:", d.dData.motorOutputs[DroneData.FL].ToString());
        EditorGUILayout.LabelField("FR:", d.dData.motorOutputs[DroneData.FR].ToString());
        EditorGUILayout.LabelField("BR:", d.dData.motorOutputs[DroneData.BR].ToString());
        EditorGUILayout.LabelField("BL:", d.dData.motorOutputs[DroneData.BL].ToString());

        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
        }
    }
}
