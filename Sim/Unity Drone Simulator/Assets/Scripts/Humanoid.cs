using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Humanoid : MonoBehaviour, IDamagable, ICollisionInfoReciever {
    public GameObject uiPrefab;

    private PlayerLogic playerLogic;

    public bool invincible = false;
    public bool ragdollOnContact = true;
    public bool showUI = false;

    public float health;
    private float maxHealth;
    private float lastDamageTime;
    private float dieTime;
    private bool dieFinalRunOnce;
    private float ragdollOnCollisionDamage;

    private Animator anim;
    private HumanoidUI uiInstance;

    private Material humanoidMat;

    public Collider humanoidCollider;
    private List<Collider> ragdollColliders = new List<Collider>();
    private List<Rigidbody> ragdollRigidbodies = new List<Rigidbody>();

    public List<Collider> ignoreColliders = new List<Collider>();

    // All children transforms
    private BodyPart[] bodyParts;

    private CharacterJointData[] characterJointsData;

    private bool isRagdoll;
    private RagdollState ragdollState;
    private float ragdollStartTime = float.PositiveInfinity;
    private float ragdollEndTime = -100f;
    private Transform hipsTransform;
    private Rigidbody hipsRigidbody;

    private Vector3 lastHipsPosition;

    /// Events ///
    public MasterHandler.CallbackEvent OnRagDollEnableEvent;
    public MasterHandler.CallbackEvent OnRagDollDisableEvent;
    /// 

    /// Constants ///
    public const float GROUND_PLANE_OFFSET = 0f;

    private const float MIN_CONTACT_VELOCITY_ENABLE_RAGDOLL = 1;
    private const float CONTACT_INHERIT_VELOCITY = 1f;

    private const float RAGDOLL_TO_MECANIM_BLEND_TIME = 0.5f;
    private const float MIN_RAGDOLL_VELOCITY = 1f;
    private const float MIN_RAGDOLL_HEIGHT = 1.5f;

    private const float MIN_DAMAGE_INTERVALS = 0.5f;
    private const float DIE_FADE_OUT_DURATION = 2f;

    private const int RAGDOLL_IGNORE_COLLISION_LAYERS = (1 << 8);
    /// 

    /// Properties ///
    public bool IsRagdoll { get { return isRagdoll; } }
    public float RagDollDisableTime { get { return ragdollEndTime; } }

    public float MaxHealth { get { return maxHealth; } }
    public float HealthPercentage { get { return Mathf.Clamp01(health / maxHealth); } }

    public bool IsAlive { get { return health > 0; } }

    public Transform HipsTransform { get { return hipsTransform; } }

    public Material HumanoidMat { get { return humanoidMat; } }
    public HumanoidUI UIInstance { get { return uiInstance; } }
    public PlayerLogic Logic { get { return playerLogic; ; } }
    /// 

    public void Start() {
        isRagdoll = false;

        dieTime = float.PositiveInfinity;
        dieFinalRunOnce = false;

        ragdollState = RagdollState.animated;

        humanoidMat = GetComponentInChildren<SkinnedMeshRenderer>().material;

        // Play idle animation
        try {
            anim = GetComponent<Animator>();
            anim.enabled = true;
            anim.Play("Armature|Idle", 0, Random.Range(0f, 1f));
        } catch (MissingComponentException e) { }

        hipsTransform = anim.GetBoneTransform(HumanBodyBones.Hips);
        hipsRigidbody = hipsTransform.GetComponent<Rigidbody>();

        lastHipsPosition = hipsRigidbody.transform.position;

        /// Fetch Colliders & rigidbodies ///
        //humanoidCollider = GetComponent<Collider>();
        foreach (Collider c in GetComponentsInChildren<Collider>()) {
            if (c != humanoidCollider) {
                ragdollColliders.Add(c);
                c.enabled = false;

                ragdollRigidbodies.Add(c.gameObject.GetComponent<Rigidbody>());
            }
        }
        humanoidCollider.enabled = true;
        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        ///

        SetKinematic(true);

        /// Fetch all transform and save to bodyParts ///
        Transform[] childrenTransforms = GetComponentsInChildren<Transform>();
        List<BodyPart> tmpBodyParts = new List<BodyPart>();
        foreach (Transform t in childrenTransforms) {
            if (t != transform) {
                tmpBodyParts.Add(new BodyPart(t, t != hipsTransform));
            }
        }
        bodyParts = tmpBodyParts.ToArray();
        /// 


        // Get character joints data
        CharacterJoint[] joints = transform.Find("Armature").GetComponentsInChildren<CharacterJoint>();
        characterJointsData = new CharacterJointData[joints.Length];
        for (int i = 0; i < joints.Length; i++) {
            characterJointsData[i] = new CharacterJointData(joints[i]);
        }


        // UI
        if (showUI) {
            uiInstance = (GameObject.Instantiate(uiPrefab) as GameObject).GetComponent<HumanoidUI>();
            uiInstance.Initialize(this, playerLogic);
        }
    }

    public void Update() {
        if (IsRagdoll) {
            if (Time.time > ragdollStartTime + 2f && Time.time < dieTime) {
                if (hipsRigidbody.velocity.magnitude < MIN_RAGDOLL_VELOCITY) {
                    RaycastHit downHit = RagdollHipsRaycastTopHit();
                    if (Mathf.Abs(downHit.point.y - hipsTransform.position.y) < MIN_RAGDOLL_HEIGHT) {
                        DisableRagdoll();
                    }
                }
            }
        }

        if (Time.time > dieTime) {
            if (Time.time < dieTime + DIE_FADE_OUT_DURATION) {
                humanoidMat.SetFloat("_Dissolve", Mathf.Lerp(1f, 0f, Mathf.Clamp01(((dieTime + DIE_FADE_OUT_DURATION) - Time.time) / DIE_FADE_OUT_DURATION)));
            } else {
                humanoidMat.SetFloat("_Dissolve", 1f);
                if (!dieFinalRunOnce) {
                    dieFinalRunOnce = true;
                    DieFinal();
                }
            }
        }

        lastHipsPosition = hipsRigidbody.transform.position;
    }

    public void LateUpdate() {
        anim.SetBool("GetUpFromFront", false);
        anim.SetBool("GetUpFromBack", false);

        if (ragdollState == RagdollState.blendToAnim) {
            if (Time.time < ragdollEndTime + RAGDOLL_TO_MECANIM_BLEND_TIME) {
                float blendT = ((ragdollEndTime + RAGDOLL_TO_MECANIM_BLEND_TIME) - Time.time) / RAGDOLL_TO_MECANIM_BLEND_TIME;
                blendT = Mathf.Clamp01(1f - blendT);

                foreach (BodyPart bp in bodyParts) {
                    bp.Blend(blendT);
                }
            } else {
                ragdollState = RagdollState.animated;
            }
        }
    }

    public void TempIgnoreCollision(Collider[] newIgnoreColliders) {
        foreach (Collider c in newIgnoreColliders) {
            ignoreColliders.Add(c);
        }
        
        Utilities.YieldAction(delegate () {
            foreach (Collider c in newIgnoreColliders) {
                if (c != null && ignoreColliders.Contains(c))
                    ignoreColliders.Remove(c);
            }
        }, 4f);
    }

    public void OnCollisionEnter(Collision collision) {
        OnExternalCollisionEnter(collision);
    }

    public void OnExternalCollisionEnter(Collision collision) {
        if (ragdollOnContact && !IsRagdoll) {
            if (collision.rigidbody != null && collision.rigidbody.velocity.magnitude > MIN_CONTACT_VELOCITY_ENABLE_RAGDOLL && !ignoreColliders.Contains(collision.collider)) {
                EnableRagdoll();
                AddForceToRagDoll(collision.rigidbody.velocity * CONTACT_INHERIT_VELOCITY);
                TakeDamage(ragdollOnCollisionDamage);
            }
        }
    }

    public void InitializeHumanoid(PlayerLogic _playerLogic, float _maxHealth, float _ragdollOnCollisionDamage) {
        playerLogic = _playerLogic;
        invincible = false;
        maxHealth = _maxHealth;
        health = maxHealth;
        ragdollOnCollisionDamage = _ragdollOnCollisionDamage;
        lastDamageTime = float.NegativeInfinity;
        dieTime = float.PositiveInfinity;
        dieFinalRunOnce = false;
    }

    public void ResetHumanoid() {
        health = maxHealth;
        lastDamageTime = float.NegativeInfinity;
        dieTime = float.PositiveInfinity;
        dieFinalRunOnce = false;
        if (humanoidMat == null)
            humanoidMat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        humanoidMat.SetFloat("_Dissolve", 0f);
        DisableRagdoll();
        Utilities.YieldAction(delegate () { anim.enabled = true; }, 0f);
    }

    public virtual void TakeDamage(float damage) {
        if (invincible || Time.time < lastDamageTime + MIN_DAMAGE_INTERVALS || damage <= 0)
            return;

        lastDamageTime = Time.time;

        health -= damage;
        health = Mathf.Max(health, 0f);

        if (health == 0) {
            Die();
        }
    }

    public virtual void Die() {
        //Destroy(gameObject);
        // Fade out
        dieTime = Time.time;

        //if (Utilities.RandomBool(0.6f)) {
        //    AudioHandler.ZonkedSound(hipsRigidbody.transform);
        //}

        Logic.Die();
    }

    public virtual void DieFinal() {
        DisableRagdoll();

        gameObject.SetActive(false);
    }

    public void FightThrowHumanoid(Vector3 force) {
        if (!IsRagdoll) {
            EnableRagdoll();
        }
        AddForceToRagDoll(force);
    }







    //////////////////////////////////////////////////////////////////////////
    //////////                                                 ///////////////
    //////////           RAGDOLL / ANIMATOR CONTROLS           ///////////////
    //////////                                                 ///////////////
    //////////////////////////////////////////////////////////////////////////
    private class BodyPart {
        public Transform transform;
        public Vector3 storedPosition;
        public Quaternion storedRotation;

        public bool applyLocal;

        public BodyPart(Transform _transform, bool _applyLocal) {
            transform = _transform;
            applyLocal = _applyLocal;
        }

        public void StoreTransform() {
            if (applyLocal) {
                storedPosition = transform.localPosition;
                storedRotation = transform.localRotation;
            } else {
                storedPosition = transform.position;
                storedRotation = transform.rotation;
            }
        }

        public void Blend(float t) {
            if (applyLocal) {
                transform.localPosition = Vector3.Lerp(storedPosition, transform.localPosition, t);
                transform.localRotation = Quaternion.Lerp(storedRotation, transform.localRotation, t);
            } else {
                transform.position = Vector3.Lerp(storedPosition, transform.position, t);
                transform.rotation = Quaternion.Lerp(storedRotation, transform.rotation, t);
            }
        }
    }

    private enum RagdollState {
        animated,    //Mecanim is fully in control
        ragdolled,   //Mecanim turned off, physics controls the ragdoll
        blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
    }

    public void EnableRagdoll() {
        if (IsRagdoll)
            return;

        isRagdoll = true;
        ragdollState = RagdollState.ragdolled;

        ragdollStartTime = Time.time;

        // Disable humanoid collider and enable body part colliders
        humanoidCollider.enabled = false;
        // Single frame delay between enabling/disabling colliders to prevent glitching
        Utilities.YieldAction(delegate () {
            foreach (Collider c in ragdollColliders) {
                c.enabled = true;
            }
            anim.enabled = false;
        }, 0f);

        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.velocity = Vector3.zero;
        }

        SetKinematic(false);

        //if (Utilities.RandomBool(0.75f)) {
        //    AudioHandler.GruntSound(transform.position);
        //}

        // Update character joints
        foreach (CharacterJointData cjd in characterJointsData) {
            cjd.Apply();
        }

        OnRagdollEnable();

        if (OnRagDollEnableEvent != null)
            OnRagDollEnableEvent();
    }

    public virtual void OnRagdollEnable() { }

    public void AddForceToRagDoll(Vector3 force) {
        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.AddForce(force);
        }
    }

    public void DisableRagdoll() {
        if (!IsRagdoll)
            return;

        ragdollState = RagdollState.blendToAnim;

        // Enable / disable colliders
        foreach (Collider c in ragdollColliders) {
            c.enabled = false;
        }
        Utilities.YieldAction(delegate () {
            isRagdoll = false;
            humanoidCollider.enabled = true;
        }, 0f);
        anim.enabled = true;

        SetKinematic(true);

        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.velocity = Vector3.zero;
        }

        ragdollEndTime = Time.time;

        // Store transforms for lerping
        foreach (BodyPart bp in bodyParts) {
            bp.StoreTransform();
        }

        // Set anim trigger
        if (hipsTransform.up.y < 0) {
            anim.SetBool("GetUpFromBack", true);
        } else {
            anim.SetBool("GetUpFromFront", true);
        }

        // Update root position
        Vector3 hipsBeforePos = hipsTransform.position;
        Quaternion hipsBeforeRotation = hipsTransform.rotation;
        Vector3 hipsMoveDelta = hipsBeforePos - transform.position;
        transform.position += hipsMoveDelta;
        RaycastHit topHit = RagdollHipsRaycastTopHit();
        transform.position = new Vector3(transform.position.x, topHit.point.y + GROUND_PLANE_OFFSET, transform.position.z);
        hipsTransform.position = hipsBeforePos;

        // Update Rotation
        Vector3 feetPosition = (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + anim.GetBoneTransform(HumanBodyBones.RightFoot).position) / 2f;
        Vector3 bodyDirection = feetPosition - anim.GetBoneTransform(HumanBodyBones.Head).position;
        bodyDirection.y = 0f;
        transform.rotation = Quaternion.LookRotation(-bodyDirection);
        hipsTransform.rotation = hipsBeforeRotation;

        foreach (BodyPart bp in bodyParts) {
            if (bp.transform == hipsTransform) {
                bp.StoreTransform();
                break;
            }
        }

        OnRagdollDisable();

        if (OnRagDollDisableEvent != null)
            OnRagDollDisableEvent();
    }

    public virtual void OnRagdollDisable() { }

    private void SetKinematic(bool val) {
        foreach (Rigidbody rb in ragdollRigidbodies) {
            rb.isKinematic = val;
        }
    }

    private RaycastHit RagdollHipsRaycastTopHit() {
        RaycastHit topHit = new RaycastHit();
        Physics.Raycast(hipsTransform.position + Vector3.up * 5f, Vector3.down, out topHit, 25f, RAGDOLL_IGNORE_COLLISION_LAYERS);
        return topHit;
    }




    private class CharacterJointData {
        public CharacterJoint joint;
        public Vector3 connectedAnchor;
        public Vector3 anchor;

        public CharacterJointData(CharacterJoint _joint) {
            joint = _joint;
            joint.autoConfigureConnectedAnchor = false;
            connectedAnchor = joint.connectedAnchor;
            anchor = joint.anchor;
        }

        public void Apply() {
            joint.connectedAnchor = connectedAnchor;
            joint.anchor = anchor;
        }
    }
}

public interface IDamagable {
    void TakeDamage(float damage);
}