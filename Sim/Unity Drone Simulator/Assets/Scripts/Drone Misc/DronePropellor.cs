using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DronePropellor : MonoBehaviour {
    private float propSpeed = 1;

    private Drone drone;
    private int motorIndex;

    private void Start() {
        drone = gameObject.GetComponentInParent<Drone>();

        switch (transform.name) {
            case "BR":
                motorIndex = DroneData.BR;
                break;
            case "BL":
                motorIndex = DroneData.BL;
                break;
            case "FR":
                motorIndex = DroneData.FR;
                break;
            case "FL":
                motorIndex = DroneData.FL;
                break;
            default:
                Debug.LogError("Motor index not set");
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(Vector3.forward * propSpeed * 600 * TimeHandler.CorrectedDeltaTime);
    }

    private void FixedUpdate() {
        propSpeed = (float) drone.dData.motorOutputs[motorIndex];
    }

    public void OnDrawGizmos() {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) < 2f) {

            Gizmos.color = Color.green;
            int segments = 12;
            if (!Utilities.CloseEnough(propSpeed, 0, 0.2f)) {
                for (int i = 0; i < segments; i++) {
                    for (int j = 2; j < 4; j++) {
                        float radius = 0.01f * j;
                        Vector3 r1 = ((Quaternion.EulerRotation(0, ((float)i / segments) * Mathf.Deg2Rad * 360f, 0)) * Vector3.forward * radius);
                        Vector3 r2 = r1 + (transform.forward * propSpeed * 0.01f);

                        //// Local plane
                        //Gizmos.DrawLine(
                        //    transform.position + r1,
                        //    transform.position + r2);

                        Handles.DrawBezier(
                            transform.position + r1,
                            transform.position + r2,
                            transform.position + r1,
                            transform.position + r2,
                            (propSpeed > 0) ? Color.green : Color.red,
                            null,
                            15);
                    }
                }
            }
            Gizmos.color = Color.black;
        }
    }
}
