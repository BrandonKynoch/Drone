using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour, IEqualityComparer {
    /// Constants //////////////////////////////////////////
    private const float SENSOR_DIST_FROM_CENTER = 0.07f;
    private const float SENSOR_MAX_RANGE = 3f;

    private const int SMOOTHNESSS_ROTATION_BUFFER_COUNT = 8;

    private const int SPAWN_ROWS_COUNT = 5;
    private const float SPAWN_SPACING = 4f;

    public const float GIZMO_DIST_FROM_CAMERA = 5f;

    public const int COLLISION_LAYERS = 0x1;
    /// Constants //////////////////////////////////////////

    // Simulation variables
    public float motorStrength = 1f;

    private DroneData data = new DroneData();
    public DroneData dData { get { return data; } }

    private Rigidbody rb;
    public Rigidbody RB { get { return rb; } }

    private BoxCollider collider;

    public Transform bladesTransform;
    private Transform bladeFL, bladeFR, bladeBR, bladeBL;



    private int contactsCount = 0;

    /// Fitness Vars /////////////////////////////////////////
    private const float IDEAL_HEIGHT = 1.1f;
    private const float MAX_VELOCITY = 0.2f;

    private float heightFitness = 0;
    private float rotationFitness = 0;
    private float smoothnessFitness = 0;
    private float distFitness = 0;
    private float velocityFitness = 0;
    private float obstacleAvoidanceFitness = 0;

    public float SumFitness {
        get {
            return heightFitness + rotationFitness + smoothnessFitness + distFitness + velocityFitness + obstacleAvoidanceFitness;
        }
    }

    public float AirborneFitness { get { return heightFitness; } }
    public float RotationFitness { get { return rotationFitness; } }
    public float SmoothnessFitness { get { return smoothnessFitness; } }
    public float DistFitness { get { return distFitness; } }
    public float VelocityFitness { get { return velocityFitness; } }
    public float ObstacleAvoidanceFitness { get { return obstacleAvoidanceFitness; } }
    /// Fitness Vars /////////////////////////////////////////

    /// Debug & Simulation Vars /////////////////////////////////////////
    private DroneUI droneUI;

    private Queue<Vector3> collisionPoints = new Queue<Vector3>();
    private float debug_NextCollisionPointRemoveTime = 0;


    private List<Quaternion> rotationBuffer = new List<Quaternion>();

    private float distFromGround = 0;
    private float initialDistFromTarget = 0;
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
        collider = GetComponent<BoxCollider>();

        bladeFL = bladesTransform.Find("FL");
        bladeFR = bladesTransform.Find("FR");
        bladeBR = bladesTransform.Find("BR");
        bladeBL = bladesTransform.Find("BL");

        droneUI = GetComponentInChildren<DroneUI>();
        droneUI.SetID((int)dData.id);
    }


    public void Update() {
        /// DEBUG UPDATE //////////////////////////////////////
        if (Time.time > debug_NextCollisionPointRemoveTime && collisionPoints.Count > 0) {
            debug_NextCollisionPointRemoveTime = Time.time + 0.2f;
            collisionPoints.Dequeue();
        }
        /// DEBUG UPDATE //////////////////////////////////////
    }

    public void FixedUpdate() {
        CalculateCollision();

        GetSensorData();

        // Gyro data
        dData.angle = Vector3.Angle(transform.up, Vector3.up);
        dData.upsideDown = dData.angle > 90f;
        if (dData.angle > 90f)
            dData.angle = 180f - dData.angle;

        if (rotationBuffer.Count > 0) {
            rotationBuffer.RemoveAt(0);
            rotationBuffer.Add(transform.rotation);
        }

        RaycastHit groundHit = new RaycastHit();
        Physics.Raycast(transform.position, Vector3.down, out groundHit);
        if (groundHit.collider != null) {
            distFromGround = groundHit.distance;
        } else {
            distFromGround = float.PositiveInfinity;
        }

        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.FL]) * motorStrength, bladeFL.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.FR]) * motorStrength, bladeFR.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.BR]) * motorStrength, bladeBR.position);
        rb.AddForceAtPosition(transform.up * ((float) data.motorOutputs[DroneData.BL]) * motorStrength, bladeBL.position);

        CalculateFitness();
    }

    public void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
            collisionPoints.Enqueue(contact.point);
        }
    }

    public void OnCollisionExit(Collision collision) {
        if (collisionPoints.Count > 0) {
            collisionPoints.Dequeue();
        }
    }

    public void CalculateCollision() {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(collider.size.x * 1.3f, collider.size.y * 3, collider.size.z * 1.3f), transform.rotation, COLLISION_LAYERS);
        contactsCount = colliders.Length;
    }

    public void GetSensorData() {
        /// INFARED SENSOR DATA //////////////////////////////////////////////
        // Circle sensors
        RaycastHit hit;
        for (int i = 0; i < data.circleSensorData.Length; i++) {
            Vector3 rotationVec = (transform.localToWorldMatrix * ((Quaternion.EulerRotation(0, ((float)i / data.circleSensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * Vector3.forward)).normalized;
            hit = new RaycastHit();
            Physics.Raycast(
                transform.position + (rotationVec * SENSOR_DIST_FROM_CENTER),
                rotationVec,
                out hit,
                SENSOR_MAX_RANGE);
            SetDistanceSensor(ref data.circleSensorData[i], hit);
        }

        // Sensor top
        hit = new RaycastHit();
        Physics.Raycast(
            transform.position,
            transform.up,
            out hit,
            SENSOR_MAX_RANGE);
        SetDistanceSensor(ref data.sensorTop, hit);

        // Sensor bottom
        hit = new RaycastHit();
        Physics.Raycast(
            transform.position,
            -transform.up,
            out hit,
            SENSOR_MAX_RANGE);
        SetDistanceSensor(ref data.sensorBottom, hit);
        /// INFARED SENSOR DATA //////////////////////////////////////////////


        /// ROTATION DATA ////////////////////////////////////////////////////
        data.rotationX = ((transform.eulerAngles.x + 180f) % 360f) - 180f;
        data.rotationY = ((transform.eulerAngles.y + 180f) % 360f) - 180f;
        data.rotationZ = ((transform.eulerAngles.z + 180f) % 360f) - 180f;
        /// ROTATION DATA ////////////////////////////////////////////////////


        /// DISTANCE DATA ////////////////////////////////////////////////////
        data.distToTarget = Vector3.Distance(transform.position, MasterHandler.DroneTarget.position);
        /// DISTANCE DATA ////////////////////////////////////////////////////


        /// VELOCITY DATA ////////////////////////////////////////////////////
        data.velocity = rb.velocity.magnitude;
        /// VELOCITY DATA ////////////////////////////////////////////////////
    }

    public void SetDistanceSensor(ref double sensor, RaycastHit hit) {
        float newDist;
        if (hit.collider != null) {
            newDist = hit.distance;
        } else {
            newDist = SENSOR_MAX_RANGE;
        }

        if (!IsInContact) {
            // If obstacle is moving away from sensor -> increase obstacle avoidance fitness
            if (newDist > sensor) {
                //obstacleAvoidanceFitness += (newDist - (float)sensor) * 10 * Time.deltaTime * DroneServerHandler.StaticInstance.obstacleAvoidanceScaler;
                obstacleAvoidanceFitness += 1f * Time.deltaTime * DroneServerHandler.StaticInstance.obstacleAvoidanceScaler;
            }
        }

        sensor = newDist;
    }

    public void CalculateFitness() {
        if (transform.position.y > MasterHandler.EnvironmentRoof.position.y || rb == null) {
            return; // Drone is invalidated if it has flown through the roof
        }

        if (!IsInContact || (rb.velocity.magnitude > 0.02f && distFromGround > 0.4f)) {
            if (!IsInContact) {
                // ax^2 + c
                float currentFrameHeightFitness = Utilities.ClampMin((20 + (-1.3f * Mathf.Pow(IDEAL_HEIGHT - distFromGround, 2))), 0) / 20f;
                currentFrameHeightFitness = (currentFrameHeightFitness / Mathf.Max(0.01f, rb.velocity.y)) / 100;
                heightFitness += currentFrameHeightFitness * Time.deltaTime * DroneServerHandler.StaticInstance.airborneFitnessScaler;
            }

            if (!dData.upsideDown) {
                rotationFitness += ((90f - (float)dData.angle) / 90f) * Time.deltaTime * DroneServerHandler.StaticInstance.rotationFitnessScaler;

                float sumRotationDelta = 0f;
                for (int i = 1; i < rotationBuffer.Count; i++) {
                    sumRotationDelta += Quaternion.Angle(rotationBuffer[i - 1], rotationBuffer[i]);
                }
                sumRotationDelta = sumRotationDelta / 360f;
                smoothnessFitness -= sumRotationDelta * Time.deltaTime * DroneServerHandler.StaticInstance.smoothnessFitnessScaler;
            }


            if (!IsInContact) {
                //float distToTarget = (float)data.distToTarget;
                //distFitness -= (distToTarget / initialDistFromTarget) * Time.deltaTime * DroneServerHandler.StaticInstance.distanceFitnessScaler;
                distFitness += (new Vector3(rb.velocity.x, 0, rb.velocity.z)).magnitude * Time.deltaTime * DroneServerHandler.StaticInstance.distanceFitnessScaler;

                float currentVelocityFitness = (rb.velocity.magnitude > MAX_VELOCITY) ? rb.velocity.magnitude - MAX_VELOCITY : 0;
                velocityFitness -= currentVelocityFitness * Time.deltaTime * DroneServerHandler.StaticInstance.velocityFitnessScaler;
            }

            // Obstacle avoidance distance is calculated in GetSensorData function
        }

        if (transform.position.y < -1f) {
            heightFitness = 0;
            rotationFitness = 0;
            smoothnessFitness = 0;
            distFitness = 0;
            velocityFitness = 0;
            obstacleAvoidanceFitness = 0;
        }

        data.fitness = SumFitness;
    }

    public void ResetDrone(bool useGrid = false) {
        if (data == null) {
            Debug.LogError("Warning data is null");
            return;
        }

        data.motorOutputs[0] = 0;
        data.motorOutputs[1] = 0;
        data.motorOutputs[2] = 0;
        data.motorOutputs[3] = 0;

        distFitness = 0;
        rotationFitness = 0;
        smoothnessFitness = 0;
        heightFitness = 0;
        velocityFitness = 0;
        obstacleAvoidanceFitness = 0;
        Utilities.YieldAction(delegate () {
            data.fitness = 0;
            obstacleAvoidanceFitness = 0;
        }, 0.1f);

        initialDistFromTarget = Vector3.Distance(transform.position, MasterHandler.DroneTarget.position);

        contactsCount = 0;
        collisionPoints.Clear();
        debug_NextCollisionPointRemoveTime = Time.time;
        distFromGround = 0;

        if (useGrid) {
            transform.position =
                    (Vector3.right * (data.id % SPAWN_ROWS_COUNT) * SPAWN_SPACING) +
                    (Vector3.forward * Mathf.FloorToInt(data.id / SPAWN_ROWS_COUNT) * SPAWN_SPACING);
        } else {
            transform.position = ProceduralGenerator.GetRandomSpawnPosition();
        }

        transform.rotation = Quaternion.identity;

        rotationBuffer.Clear();
        for (int i = 0; i < SMOOTHNESSS_ROTATION_BUFFER_COUNT; i++) {
            rotationBuffer.Add(transform.rotation);
        }

        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (rb != null) {
            GetSensorData();
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
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > GIZMO_DIST_FROM_CAMERA) {
            return;
        }

        float gizmosScaler = 0.1f;

        // Forward indicator
        if (IsInContact) {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
        } else {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
        }
        Gizmos.DrawCube(transform.position, Vector3.one * gizmosScaler * 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmosScaler);
        Gizmos.DrawLine(
            transform.position + transform.forward * gizmosScaler,
            transform.position + (transform.forward * gizmosScaler * 0.75f + transform.right * gizmosScaler * 0.2f));
        Gizmos.DrawLine(
            transform.position + transform.forward * gizmosScaler,
            transform.position + (transform.forward * gizmosScaler * 0.75f - transform.right * gizmosScaler * 0.2f));

        // Circle indicator
        Gizmos.color = Color.black;
        DrawGizmosCircle(transform, SENSOR_DIST_FROM_CENTER, 60);

        // Sensor Indicators
        for (int i = 0; i < data.circleSensorData.Length; i++) {
            Vector3 rotationVec = (transform.localToWorldMatrix * ((Quaternion.EulerRotation(0, ((float)i / data.circleSensorData.Length) * Mathf.Deg2Rad * 360f, 0)) * Vector3.forward)).normalized;
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


        // Collision gizmos
        Gizmos.color = Color.black;
        foreach (Vector3 v in collisionPoints) {
            Gizmos.DrawCube(v, Vector3.one * 0.03f);
        }
    }

    private void DrawGizmosCircle(Transform t, float radius, int segments) {
        for (int i = 0; i < segments; i++) {
            for (int j = 0; j < 3; j++) {
                Vector3 r0 = ((Quaternion.EulerRotation(0, ((float)i / segments) * Mathf.Deg2Rad * 360f + (transform.eulerAngles.y * Mathf.Deg2Rad), 0)) * Vector3.forward * radius) + (Vector3.up * j * 0.003f);
                Vector3 r1 = ((Quaternion.EulerRotation(0, ((float)i / segments) * Mathf.Deg2Rad * 360f, 0)) * Vector3.forward * radius) + (Vector3.up * j * 0.003f);
                Vector3 r2 = (Quaternion.EulerRotation(0, ((float)(i + 1) / segments) * Mathf.Deg2Rad * 360f, 0)) * Vector3.forward * radius + (Vector3.up * j * 0.003f);
                Vector4 a4 = transform.localToWorldMatrix * r1;
                Vector4 b4 = transform.localToWorldMatrix * r2;
                Vector3 a = new Vector3(a4.x, a4.y, a4.z);
                Vector3 b = new Vector3(b4.x, b4.y, b4.z);

                // World plane
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(
                    transform.position + r1,
                    transform.position + r2);
                Gizmos.color = Color.black;

                // Local plane
                Gizmos.DrawLine(
                    transform.position + a,
                    transform.position + b);

                Gizmos.color = Color.cyan;
                if (j == 0) {
                    Gizmos.DrawLine(
                        transform.position + a,
                        transform.position + r0);
                }
                Gizmos.color = Color.black;
            }
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

    public double distToTarget;

    // Sensors start from directly in front and rotate around clockwise when viewed from above drone
    public double[] circleSensorData = new double[CIRCLE_circle_sensor_array_COUNT];
    public double sensorTop;
    public double sensorBottom;

    // The axis angle offset from identity (upright & forward) - wrapped around -180 to 180
    public double rotationX;
    public double rotationY;
    public double rotationZ;

    // Magnitude of the drones current velocity
    public double velocity;

    // Sim & fitness function variables
    public double angle; // The angle of the drone relative to the horizontal plane. in the range of 0-90 degrees
    public bool upsideDown;

    public double fitness;

    public int HashFunction() {
        return id.GetHashCode();
    }
}