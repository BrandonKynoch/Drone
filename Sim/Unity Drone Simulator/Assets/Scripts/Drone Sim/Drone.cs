using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour, IEqualityComparer {
    /// Constants //////////////////////////////////////////
    private const float SENSOR_DIST_FROM_CENTER = 0.15f;
    private const float SENSOR_MAX_RANGE = 2f;

    private const int SPAWN_ROWS_COUNT = 5;
    private const float SPAWN_SPACING = 4f;
    /// Constants //////////////////////////////////////////

    // Simulation variables
    public float motorStrength = 1f;

    private DroneData data = new DroneData();
    public DroneData dData { get { return data; } }

    private Rigidbody rb;
    public Rigidbody RB { get { return rb; } }

    public Transform bladesTransform;
    private Transform bladeFL, bladeFR, bladeBR, bladeBL;



    private int contactsCount = 0;

    /// Debug Vars /////////////////////////////////////////
    private Queue<Vector3> collisionPoints = new Queue<Vector3>();
    /// Debug Vars /////////////////////////////////////////

    /// Properties /////////////////////////////////////////
    public bool IsInContact { get { return contactsCount != 0; } }
    /// Properties /////////////////////////////////////////


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

        CalculateFitness();
    }

    public void OnCollisionEnter(Collision collision) {
        contactsCount++;
        foreach (ContactPoint contact in collision.contacts) {
            collisionPoints.Enqueue(contact.point);
        }
    }

    public void OnCollisionExit(Collision collision) {
        contactsCount--;
    }

    public void GetSensorData() {
        // Circle sensors
        RaycastHit hit;
        for (int i = 0; i < data.circleSensorData.Length; i++) {
            Vector3 rotationVec = ((Quaternion.EulerRotation(0, ((float)i / data.circleSensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * transform.forward).normalized;
            hit = new RaycastHit();
            Physics.Raycast(
                transform.position + (rotationVec * SENSOR_DIST_FROM_CENTER),
                rotationVec,
                out hit,
                SENSOR_MAX_RANGE);

            if (hit.collider != null) {
                data.circleSensorData[i] = hit.distance;
            } else {
                data.circleSensorData[i] = SENSOR_MAX_RANGE;
            }
        }

        // Sensor top
        hit = new RaycastHit();
        Physics.Raycast(
            transform.position,
            transform.up,
            out hit,
            SENSOR_MAX_RANGE);
        if (hit.collider != null) {
            data.sensorTop = hit.distance;
        } else {
            data.sensorTop = SENSOR_MAX_RANGE;
        }

        // Sensor bottom
        hit = new RaycastHit();
        Physics.Raycast(
            transform.position,
            -transform.up,
            out hit,
            SENSOR_MAX_RANGE);
        if (hit.collider != null) {
            data.sensorBottom = hit.distance;
        } else {
            data.sensorBottom = SENSOR_MAX_RANGE;
        }
    }

    public void CalculateFitness() {
        float distToTarget = Vector3.Distance(transform.position, MasterHandler.DroneTarget.position);
        float distFitness = 1f / (distToTarget / DroneServerHandler.MaximumDroneDistFromTarget);
        distFitness = Mathf.Clamp(distFitness, 0, 20) / 20f;

        float airborneFitness = (contactsCount == 0) ? 1 : 0;

        float totalFitness = distFitness + airborneFitness;

        data.fitness += distFitness * Time.deltaTime;
    }

    public void ResetDrone(Vector3 spawnPosition) {
        if (data == null) {
            Debug.LogError("Warning data is null");
            return;
        }

        data.fitness = 0;

        transform.position =
                spawnPosition +
                (Vector3.right * (data.id % SPAWN_ROWS_COUNT) * SPAWN_SPACING) +
                (Vector3.forward * Mathf.FloorToInt(data.id / SPAWN_ROWS_COUNT) * SPAWN_SPACING);

        transform.rotation = Quaternion.identity;

        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
        if (IsInContact) {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
        } else {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
        }
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
        for (int i = 0; i < data.circleSensorData.Length; i++) {
            Vector3 rotationVec = ((Quaternion.EulerRotation(0, ((float)i / data.circleSensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * transform.forward).normalized;
            Gizmos.DrawLine(
                transform.position + (rotationVec * SENSOR_DIST_FROM_CENTER),
                transform.position + (rotationVec * (SENSOR_DIST_FROM_CENTER + (float) data.circleSensorData[i])));
        }

        Gizmos.DrawLine(
                transform.position,
                transform.position + (transform.up * (float) data.sensorTop));
        Gizmos.DrawLine(
                transform.position,
                transform.position + (-transform.up * (float)data.sensorBottom));
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
    public const int CIRCLE_circle_sensor_array_COUNT = 8;

    // Identifiers
    public const int FL = 0; // Front left
    public const int FR = 1; // Front right
    public const int BR = 2; // Back right
    public const int BL = 3; // Back left
    /// Constants //////////////////////////////////////////

    public UInt64 id;

    public double[] motorOutputs = new double[4];
    // Sensors start from directly in front and rotate around clockwise when viewed from above drone
    public double[] circleSensorData = new double[CIRCLE_circle_sensor_array_COUNT];
    public double sensorTop;
    public double sensorBottom;

    public double fitness;

    public int HashFunction() {
        return id.GetHashCode();
    }
}