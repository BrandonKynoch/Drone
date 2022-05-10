using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayerController : MonoBehaviour {
    public GameObject explosionParticlesPrefab;
    public Rigidbody moveTarget;
    public Transform holdTransform;
    public Transform smoothedHipsTransform;

    private UserPlayer logic;

    private List<Vector3> previousPositionData = new List<Vector3>();
    private Vector3 smoothedHipsPosition;

    private static UserPlayerController staticInstance;

    private Vector2 inputMoveVector;

    /// Constants ///
    private const float MAX_VELOCITY = 5f;
    private const float VELOCITY_LERP_SPEED = 15f;

    private const int RAYCAST_PLANE = 1 << 10;

    private const int SMOOTHED_HIPS_POSITION_DATA_LENGTH = 3;
    /// 

    /// Properties ///
    public static PlayerLogic Logic { get { return staticInstance.logic; } }

    public static Vector3 SmoothedHipsPosition { get { return staticInstance.smoothedHipsPosition; } }
    public static Vector3 TargetVelocity { get { return Utilities.ToVector3(staticInstance.logic.targetVelocity, true); } }
    public static Vector3 TargetLookVector { get { return staticInstance.logic.targetLookVector; } }
    public static float PlayerSize { get { return staticInstance.logic.PlayerSize; } }

    public static bool IsHolding { get { return staticInstance.logic.IsHolding; } }
    public static bool IsRagdoll { get { return staticInstance.logic.PlayerHumanoid.IsRagdoll; } }
    public static PlayerLogic.NearbyPickup NearbyInteractableNextPickup { get { return staticInstance.logic.NearbyInteractableNextPickup; } }
    /// 

    public void Start() {
        staticInstance = this;

        for (int i = 0; i < SMOOTHED_HIPS_POSITION_DATA_LENGTH; i++) {
            previousPositionData.Add(transform.position);
        }
        smoothedHipsPosition = transform.position;
        smoothedHipsTransform.SetParent(null);

        //InputHandler.OnTouchUp += OnTouchUp;

        inputMoveVector = Vector2.zero;

        logic = new UserPlayer(this, moveTarget, transform, holdTransform, explosionParticlesPrefab, MAX_VELOCITY, VELOCITY_LERP_SPEED);
    }

    public void Update() {
        if (MasterHandler.IsPlaying && MasterHandler.PlayerInputEnabled) {
            logic.Update();

            /// Movement ///
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
                if (Input.GetKey(KeyCode.W)) {
                    inputMoveVector.y += VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime;
                }
                if (Input.GetKey(KeyCode.S)) {
                    inputMoveVector.y -= VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime;
                }
            } else {
                inputMoveVector.y = Mathf.Lerp(inputMoveVector.y, 0, VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime);
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
                if (Input.GetKey(KeyCode.A)) {
                    inputMoveVector.x -= VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime;
                }
                if (Input.GetKey(KeyCode.D)) {
                    inputMoveVector.x += VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime;
                }
            } else {
                inputMoveVector.x = Mathf.Lerp(inputMoveVector.x, 0, VELOCITY_LERP_SPEED * TimeHandler.CorrectedDeltaTime);
            }

            inputMoveVector.x = Mathf.Clamp(inputMoveVector.x, -MAX_VELOCITY, MAX_VELOCITY);
            inputMoveVector.y = Mathf.Clamp(inputMoveVector.y, -MAX_VELOCITY, MAX_VELOCITY);

            Vector2 rotatedVector = Vector2.zero;
            float camera_theta = (-Camera.main.transform.parent.parent.eulerAngles.y + 180) * Mathf.Deg2Rad;
            rotatedVector.x = (Mathf.Cos(camera_theta) * inputMoveVector.x) - (Mathf.Sin(camera_theta) * inputMoveVector.y);
            rotatedVector.y = ((Mathf.Sin(camera_theta) * inputMoveVector.x) + (Mathf.Cos(camera_theta) * inputMoveVector.y));

            logic.targetVelocity = rotatedVector;
            logic.targetLookVector = Utilities.ToVector3(rotatedVector, true);
            ///


            /// Smoothed Position Data ///
            previousPositionData.RemoveAt(0);
            previousPositionData.Add(logic.PlayerHumanoid.HipsTransform.position);
            smoothedHipsPosition = Vector3.zero;
            for (int i = 0; i < SMOOTHED_HIPS_POSITION_DATA_LENGTH; i++) {
                smoothedHipsPosition += previousPositionData[i];
            }
            smoothedHipsPosition /= SMOOTHED_HIPS_POSITION_DATA_LENGTH;
            smoothedHipsTransform.position = smoothedHipsPosition;
            ///


            /// Set sortval for nearby pickups ///
            //Vector3 groundHitCenterScreen = Vector3.zero;
            //RaycastHit hit = new RaycastHit();
            //Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f)), out hit, 100f, RAYCAST_PLANE);
            //if (hit.collider != null) {
             //   groundHitCenterScreen = hit.point;
                logic.SetNearbyPickUpSortValFromTargetPosition(logic.PlayerHumanoid.HipsTransform.position, 3f);
            //}
            /// 

            if (logic.NearbyInteractableNextPickup != null) {
                logic.NearbyInteractableNextPickup.interactable.viewingOutlinableTime = Time.time;
            }
        }

        /// Gizmos ///
#if UNITY_EDITOR
        Debug.DrawLine(new Vector3(transform.position.x, 0.1f, transform.position.z), new Vector3(transform.position.x, 0.1f, transform.position.z) + Vector3.up, Color.red);
        Debug.DrawLine(new Vector3(transform.position.x, 0.1f, transform.position.z), new Vector3(transform.position.x, 0.1f, transform.position.z) + (Utilities.ToVector3(logic.targetVelocity, true)).normalized * 2f, Color.black);
#endif
        ///
    }

    public void FixedUpdate() {
        logic.FixedUpdate();
    }

    /*
    public void OnTouchUp() {
        if (logic.IsHolding) {
            if (InputHandler.HoldDeltaPos.magnitude >= 0.4f && InputHandler.HoldDuration < 0.2f) {
                Ray cameraProjection = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
                Ray cameraInputProjection = Camera.main.ScreenPointToRay(new Vector3((Screen.width / 2f) - (InputHandler.HoldDeltaPos.x * Screen.width), (Screen.height / 2f) - (InputHandler.HoldDeltaPos.y * Screen.height), 0f));

                RaycastHit hitA = new RaycastHit();
                RaycastHit hitB = new RaycastHit();
                Physics.Raycast(cameraProjection, out hitA, RAYCAST_PLANE);
                Physics.Raycast(cameraInputProjection, out hitB, RAYCAST_PLANE);

                Vector2 inputToWorldDir = (new Vector2(
                    (hitA.point.x - hitB.point.x),
                    (hitA.point.z - hitB.point.z)
                    )).normalized * InputHandler.HoldDeltaPos.magnitude;

                logic.ThrowInteractable(inputToWorldDir);
            }
        }
    }*/





    public class UserPlayer : PlayerLogic {
        public UserPlayerController controller;

        public UserPlayer(UserPlayerController _controller, Rigidbody _moveTarget, Transform _instance, Transform _holdTransform, GameObject _explosionParticlePrefab, float _maxVelocity, float _velocityLerpSpeed) : base(_instance, _moveTarget, _holdTransform, _explosionParticlePrefab, _maxVelocity, _velocityLerpSpeed) {
            controller = _controller;
        }

        public override void OnDie() {
            //AudioHandler.DieSound(1f);
            //SwiftForUnity.HeavyVibration();

            //controller.moveTarget.gameObject.SetActive(false);
        }

        public override void OnRagDollEnable() {
            base.OnRagDollEnable();

            controller.moveTarget.isKinematic = true;
        }

        public override void OnRagDollDisable() {
            base.OnRagDollDisable();

            controller.moveTarget.isKinematic = true;

            controller.moveTarget.position = controller.transform.position;

            Utilities.YieldAction(delegate () {
                controller.moveTarget.position = controller.transform.position;
                controller.moveTarget.isKinematic = false;
            }, 0f);
        }
    }
}
