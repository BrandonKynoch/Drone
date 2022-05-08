using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayerController : MonoBehaviour {
    public GameObject explosionParticlesPrefab;
    public Rigidbody moveTarget;
    public Transform holdTransform;

    private UserPlayer logic;

    private List<Vector3> previousPositionData = new List<Vector3>();
    private Vector3 smoothedHipsPosition;

    private static UserPlayerController staticInstance;

    /// Constants ///
    public const float INPUT_DIAMETER = 0.4f;

    private const float MAX_VELOCITY = 5f;
    private const float VELOCITY_LERP_SPEED = 8f;

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

        InputHandler.OnTouchUp += OnTouchUp;

        logic = new UserPlayer(this, moveTarget, transform, holdTransform, explosionParticlesPrefab, MAX_VELOCITY, VELOCITY_LERP_SPEED);
    }

    public void Update() {
        if (MasterHandler.IsPlaying) {
            logic.Update();

            /// Movement ///
            Vector2 inputVector = Utilities.ClampVector01(InputHandler.HoldDeltaPos / (INPUT_DIAMETER * 0.5f)) * 0.1f;
            Ray cameraProjection = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            Ray cameraInputProjection = Camera.main.ScreenPointToRay(
                new Vector3(
                    (Screen.width / 2f) - (inputVector.x * Screen.width),
                    (Screen.height / 2f) - (inputVector.y * Screen.height),
                    0f));

            //Debug.DrawRay(cameraProjection.origin, cameraProjection.direction * 10f, Color.black);
            //Debug.DrawRay(cameraInputProjection.origin, cameraInputProjection.direction * 10f, Color.green);

            RaycastHit hitA = new RaycastHit();
            RaycastHit hitB = new RaycastHit();
            Physics.Raycast(cameraProjection, out hitA, 100f, RAYCAST_PLANE);
            Physics.Raycast(cameraInputProjection, out hitB, 100f, RAYCAST_PLANE);

            Vector2 inputVelocity = (new Vector2(
                (hitA.point.x - hitB.point.x),
                (hitA.point.z - hitB.point.z)
                )).normalized * Utilities.ClampVector01(InputHandler.HoldDeltaPos / INPUT_DIAMETER).magnitude;

            /*if (InputHandler.IsHolding && InputHandler.TouchDownPositionNormalized.y > CameraHandler.CAMERA_MOVE_INPUT_SCREEN_POS) {
                logic.targetVelocity = inputVelocity * logic.MaxVelocity;
                logic.targetLookVector = transform.position - Camera.main.transform.position;
            } else {
                logic.targetVelocity = Vector2.zero;
                logic.targetLookVector = Utilities.ToVector3(inputVelocity, true);
            }*/
            ///

            /// Smoothed Position Data ///
            previousPositionData.RemoveAt(0);
            previousPositionData.Add(logic.PlayerHumanoid.HipsTransform.position);
            smoothedHipsPosition = Vector3.zero;
            for (int i = 0; i < SMOOTHED_HIPS_POSITION_DATA_LENGTH; i++) {
                smoothedHipsPosition += previousPositionData[i];
            }
            smoothedHipsPosition /= SMOOTHED_HIPS_POSITION_DATA_LENGTH;
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
    }





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
