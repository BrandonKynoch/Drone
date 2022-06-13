using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour, IEqualityComparer {
    /// Constants //////////////////////////////////////////
    private const float SENSOR_DIST_FROM_CENTER = 0.15f;
    private const float SENSOR_MAX_RANGE = 2f;
    /// Constants //////////////////////////////////////////

    // Simulation variables
    public float motorStrength = 1f;

    private DroneData data = new DroneData();
    public DroneData dData { get { return data; } }

    private Rigidbody rb;

    public Transform bladesTransform;
    private Transform bladeFL, bladeFR, bladeBR, bladeBL;

    public void UpdateMotorOutputs(double fl, double fr, double br, double bl) {
        data.motorOutputs[DroneData.FL] = fl;
        data.motorOutputs[DroneData.FR] = fr;
        data.motorOutputs[DroneData.BR] = br;
        data.motorOutputs[DroneData.BL] = bl;
    }

    public void Start() {
        rb = GetComponent<Rigidbody>();

        bladeFL = bladesTransform.Find("FL");
        bladeFR = bladesTransform.Find("FR");
        bladeBR = bladesTransform.Find("BR");
        bladeBL = bladesTransform.Find("BL");
    }


    public void Update() {

    }

    public void FixedUpdate() {
        GetSensorData();

        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.FL]) * motorStrength, bladeFL.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.FR]) * motorStrength, bladeFR.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.BR]) * motorStrength, bladeBR.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.BL]) * motorStrength, bladeBL.position);
    }

    public void GetSensorData() {
        // Sensor Indicators
        for (int i = 0; i < data.sensorData.Length; i++) {
            Vector3 rotationVec = ((Quaternion.EulerRotation(0, ((float)i / data.sensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * transform.forward).normalized;
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(
                transform.position + (rotationVec * SENSOR_DIST_FROM_CENTER),
                rotationVec,
                out hit,
                SENSOR_MAX_RANGE);

            if (hit.collider != null) {
                data.sensorData[i] = hit.distance;
            } else {
                data.sensorData[i] = SENSOR_MAX_RANGE;
            }
        }
    }

    #region MISC
    /// Object overrides /// 
    bool IEqualityComparer.Equals(object x, object y) {
        if (x == null && y == null)
            return true;

        Drone dx = x as Drone;
        Drone dy = y as Drone;

        if (dx == null || dy == null)
            return false;

        return dx.data.id == dy.data.id;
    }

    int IEqualityComparer.GetHashCode(object obj) {
        return data.HashFunction();
    }
    ///
    #endregion

    #region GIZMOS
    /// Gizmos ///
    private void OnDrawGizmos() {
        float gizmosScaler = 0.1f;

        // Forward indicator
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, Vector3.one * gizmosScaler * 0.2f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmosScaler);
        Gizmos.DrawLine(
            transform.position + transform.forward * gizmosScaler,
            transform.position + (transform.forward * gizmosScaler * 0.75f + transform.right * gizmosScaler * 0.2f));
        Gizmos.DrawLine(
            transform.position + transform.forward * gizmosScaler,
            transform.position + (transform.forward * gizmosScaler * 0.75f - transform.right * gizmosScaler * 0.2f));

        // Circle indicator
        Gizmos.color = Color.cyan;
        DrawGizmosCircle(transform, SENSOR_DIST_FROM_CENTER, 20);

        // Sensor Indicators
        for (int i = 0; i < data.sensorData.Length; i++) {
            Vector3 rotationVec = ((Quaternion.EulerRotation(0, ((float)i / data.sensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * transform.forward).normalized;
            Gizmos.DrawLine(
                transform.position + (rotationVec * SENSOR_DIST_FROM_CENTER),
                transform.position + (rotationVec * (SENSOR_DIST_FROM_CENTER + (float) data.sensorData[i])));
        }
    }

    private void DrawGizmosCircle(Transform t, float radius, int segments) {
        for (int i = 0; i < segments; i++) {
            Gizmos.DrawLine(
                transform.position + ((Quaternion.EulerRotation(0, ((float)i / segments) * Mathf.Deg2Rad * 360f, 0)) * transform.forward * radius),
                transform.position + ((Quaternion.EulerRotation(0, ((float) (i + 1) / segments) * Mathf.Deg2Rad * 360f, 0)) * transform.forward * radius));
        }
    }
    ///
    #endregion
}


public class DroneData {
    /// Constants //////////////////////////////////////////
    public const int CIRCLE_SENSOR_ARRAY_COUNT = 8;

    // Identifiers
    public const int FL = 0; // Front left
    public const int FR = 1; // Front right
    public const int BR = 2; // Back right
    public const int BL = 3; // Back left
    /// Constants //////////////////////////////////////////

    public UInt64 id;

    public double[] motorOutputs = new double[4];
    // Sensors start from directly in front and rotate around clockwise when viewed from above drone
    public double[] sensorData = new double[CIRCLE_SENSOR_ARRAY_COUNT];

    // MARK: todo: Implement sensors and return to c-drone
    // sensor gizmos
    // fix player camera after spawning drones

    public int HashFunction() {
        return id.GetHashCode();
    }
}