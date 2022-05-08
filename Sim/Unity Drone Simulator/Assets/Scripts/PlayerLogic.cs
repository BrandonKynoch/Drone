using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerLogic : IComparable<PlayerLogic> {
    public Transform instance;
    private Transform instanceArmature;
    public Rigidbody moveTarget;
    private Humanoid humanoid;
    private Animator anim;
    private Transform holdTransform;

    private float lastInteractionTime;
    private float nextFightHitTime;

    public PlayerLogic closestPlayer;

    private Vector3 originalScale;
    private float playerSize;
    private float playerSizeX;

    private float maxVelocity = 4f;
    private float velocityLerpSpeed = 5f;

    private GameObject explosionParticlesPrefab;
    public Color explosionParticlesColour;

    public Vector2 targetVelocity;
    private Vector2 velocity;

    public Vector3 targetLookVector;

    private bool isDead;
    private bool punchAnimTrigger;

    private bool hasSpawned;

    private List<NearbyPickup> nearbyPickups = new List<NearbyPickup>();
    private Interactable holdingObject;
    private NearbyPickup nearbyInteractableNextPickup;

    public float sortVal;

    /// Constants ///
    private const float MAX_HEALTH = 100f;
    private const float COLLISION_DAMAGE = 10f;

    private const float INITIAL_PLAYER_MASS = 60f;

    private const float FIGHT_DIST_INITIAL = 1.5f;
    private const float FIGHT_PUNCH_DAMAGE = 2f;
    private const float FIGHT_PUNCH_THROW_PROBABILITY = 0.15f;
    private const float FIGHT_PUNCH_THROW_STRENGTH = 2000f;

    private const float MAX_PICK_UP_WEIGHT_INITIAL = 20f;

    private const float GROWTH_RATE = 0.1f;

    private const float ROTATION_SPEED = 3f;

    private const float PICK_UP_RADIUS_INITIAL = 2f;
    public const float PICK_UP_TIME = 2f;
    public const int PICK_UP_LAYER = 1 << 11;

    private const float THROW_FORCE = 15f;

    private const float INTERACTION_ANIM_LAYER_DURATION = 1.5f;
    /// 

    /// Properties ///
    public Humanoid PlayerHumanoid { get { return humanoid; } }
    public bool HasSpawned { get { return hasSpawned; } }
    public bool IsAlive { get { return !isDead; } }
    public bool IsHolding { get { return holdingObject != null && holdingObject.isHolding; } }
    public Interactable HoldingInteractable { get { return holdingObject; } }
    public bool IsCurling { get { return IsHolding && velocity.magnitude < 0.1f; } }
    public float MaxVelocity { get { return maxVelocity; } }
    public float PlayerSize { get { return Mathf.Max(1f, playerSize); } }
    public float PlayerMass { get { return Mathf.RoundToInt(INITIAL_PLAYER_MASS * ((MasterHandler.metricSystem == MetricSystem.Metric)? 1f: 2.20462f) * PlayerSize); } }
    public string PlayerMassString { get { return PlayerMass.ToString() + ((MasterHandler.metricSystem == MetricSystem.Metric)? " KG" : " lb"); } }
    public float MaxPickUpWeight { get { return MAX_PICK_UP_WEIGHT_INITIAL * Mathf.Pow(playerSize, 2f); } }
    public float PickUpRadius { get { return PICK_UP_RADIUS_INITIAL * Mathf.Pow(playerSize, 1.2f); } }
    public float FightDist { get { return FIGHT_DIST_INITIAL * playerSize; } }

    public bool ReadyToPickUpInteractable { get { return !humanoid.IsRagdoll && targetVelocity.magnitude == 0f; } }
    public NearbyPickup NearbyInteractableNextPickup { get { return nearbyInteractableNextPickup; } }
    /// 

    public PlayerLogic(Transform _instance, Rigidbody _moveTarget, Transform _holdTransform, GameObject _explosionParticlePrefab, float _maxVelocity, float _velocityLerpSpeed) {
        instance = _instance;
        instanceArmature = instance.Find("Armature");
        anim = instance.GetComponentInChildren<Animator>();
        holdTransform = _holdTransform;
        explosionParticlesPrefab = _explosionParticlePrefab;

        humanoid = instance.GetComponent<Humanoid>();
        humanoid.InitializeHumanoid(this, MAX_HEALTH, COLLISION_DAMAGE);
        humanoid.OnRagDollEnableEvent += OnRagDollEnable;
        humanoid.OnRagDollDisableEvent += OnRagDollDisable;

        maxVelocity = _maxVelocity;
        velocityLerpSpeed = _velocityLerpSpeed;

        originalScale = instance.localScale;
        playerSizeX = 0f;
        playerSize = CalculatePlayerSize(playerSizeX);

        moveTarget = _moveTarget;
        if (moveTarget != null) {
            moveTarget.transform.SetParent(null);
        }

        isDead = false;
        hasSpawned = false;
        punchAnimTrigger = false;

        lastInteractionTime = -100f;
        
        //instance.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_OutlineColour", new Color(playerCol.r * 0.9f, playerCol.g * 0.9f, playerCol.b * 0.9f, 1f));
        //instance.GetComponentInChildren<TrailRenderer>().material.SetColor("_BaseColor", new Color(playerCol.r, playerCol.g, playerCol.b, 1f));
        //instance.GetComponentInChildren<TrailRenderer>().material.SetColor("_EmissionColor", playerCol);

        //SceneHandler.RegisterPlayer(this);

        //instance.position = SceneLogic.GetRandomAvailablePositionWithWeight(this);

        holdingObject = null;

        sortVal = 0f;

        Utilities.YieldAction(delegate () {
            hasSpawned = true;
        }, 0f);
    }

    public void Update() {
        if (!isDead) {
            /// Move ///
            // Smooth lerp to rigidbody position
            if (moveTarget != null && !humanoid.IsRagdoll) {
                moveTarget.transform.position = new Vector3(moveTarget.transform.position.x, Humanoid.GROUND_PLANE_OFFSET, moveTarget.transform.position.z);

                instance.transform.position = Vector3.Lerp(
                    instance.transform.position,
                    moveTarget.transform.position + (new Vector3(velocity.x, 0f, velocity.y) * 3f * Time.fixedDeltaTime),
                    8f * TimeHandler.CorrectedDeltaTime);

                moveTarget.transform.localScale = Vector3.one * playerSize;
            }

            // Animation
            Vector3 localVelocity = (instance.transform.worldToLocalMatrix * new Vector3(velocity.x, 0f, velocity.y)) / PlayerSize;
            Vector3 scaledLocalVelocity = localVelocity / maxVelocity;
            anim.SetFloat("VelocityX", scaledLocalVelocity.x);
            anim.SetFloat("VelocityY", scaledLocalVelocity.z);
            anim.SetFloat("RunMultiplier", 1f + (scaledLocalVelocity.magnitude * 2f));
            int animMovementLayerIndex = anim.GetLayerIndex("Movement");
            if (localVelocity.magnitude > 0.1f) {
                anim.SetLayerWeight(animMovementLayerIndex, Mathf.Lerp(anim.GetLayerWeight(animMovementLayerIndex), 1f, 3f * TimeHandler.CorrectedDeltaTime));
            } else {
                anim.SetLayerWeight(animMovementLayerIndex, Mathf.Lerp(anim.GetLayerWeight(animMovementLayerIndex), 0f, 3f * TimeHandler.CorrectedDeltaTime));
            }

            anim.SetBool("Curling", IsHolding);
            /// 

            /// Rotation ///
            if (!humanoid.IsRagdoll) {
                if (targetVelocity.magnitude > 0) {
                    anim.SetBool("Fighting", false);

                    if (velocity != Vector2.zero) {
                        instance.transform.rotation = Quaternion.Lerp(instance.transform.rotation, Quaternion.LookRotation(new Vector3(velocity.x, 0f, velocity.y), Vector3.up), (velocity.magnitude / maxVelocity) * ROTATION_SPEED * TimeHandler.CorrectedDeltaTime);
                    }
                } else {
                    /// Fight nearby players ///
                    if (closestPlayer != null && Vector3.Distance(instance.position, closestPlayer.instance.position) < FightDist) {
                        targetLookVector = closestPlayer.instance.position - instance.position;
                        anim.SetBool("Fighting", true);

                        if (punchAnimTrigger) {
                            punchAnimTrigger = false;
                            if (!closestPlayer.humanoid.IsRagdoll) {
                                closestPlayer.humanoid.TakeDamage(FIGHT_PUNCH_DAMAGE * PlayerSize);
                                if (Utilities.RandomBool(FIGHT_PUNCH_THROW_PROBABILITY * PlayerSize)) {
                                    closestPlayer.humanoid.FightThrowHumanoid(targetLookVector.normalized * FIGHT_PUNCH_THROW_STRENGTH * PlayerSize);
                                }

                                EffectsHandler.PunchEffect(holdTransform.position, playerSize);
                            }
                        }
                    } else {
                        anim.SetBool("Fighting", false);
                    }
                    /// 

                    if (targetLookVector.magnitude > 0) {
                        instance.transform.rotation = Quaternion.Lerp(instance.transform.rotation, Quaternion.LookRotation(new Vector3(targetLookVector.x, 0f, targetLookVector.z), Vector3.up), ROTATION_SPEED * TimeHandler.CorrectedDeltaTime);
                    }
                }
            }
            ///


            /// Animations ///
            int interactionAnimLayerIndex = anim.GetLayerIndex("Interaction");
            anim.SetLayerWeight(interactionAnimLayerIndex, Mathf.Lerp(anim.GetLayerWeight(interactionAnimLayerIndex), (Time.time < lastInteractionTime + INTERACTION_ANIM_LAYER_DURATION) ? 1f : 0f, 4f * TimeHandler.CorrectedDeltaTime));
            /// 


            if (IsCurling) {
                playerSizeX += ((GROWTH_RATE * holdingObject.Weight) / 1000f) * TimeHandler.CorrectedDeltaTime;
                playerSize = CalculatePlayerSize(playerSizeX);
                instanceArmature.localScale = originalScale * playerSize;
            }


            //if (holdingObject != null) {
                // Enable Ragdoll arm while running
            //}
        }
    }

    public void FixedUpdate() {
        velocity = Vector2.Lerp(velocity, (humanoid.IsRagdoll) ? Vector2.zero : targetVelocity * playerSize, velocityLerpSpeed * Time.fixedDeltaTime);

        if (moveTarget != null && !humanoid.IsRagdoll) {
            moveTarget.velocity = new Vector3(velocity.x, 0f, velocity.y);
        }

        /// Collision Check For Pick Up Objects ///
        Collider[] nearbyColliders = Physics.OverlapSphere(instance.transform.position, PickUpRadius, PICK_UP_LAYER);
        bool nearbyPickupsChanged = false;
        // Check for collision enter
        foreach (Collider c in nearbyColliders) {
            Interactable interactable = c.GetComponent<Interactable>();

            if (interactable == null && c.transform.parent != null) {
                interactable = c.transform.parent.GetComponent<Interactable>();
            }

            if (interactable != null) {
                bool contains = false;
                foreach (NearbyPickup np in nearbyPickups) {
                    if (np.interactable == interactable) {
                        contains = true;
                        break;
                    }
                }
                if (!contains) {
                    nearbyPickups.Add(new NearbyPickup(interactable, Time.time));
                    nearbyPickupsChanged = true;
                }
            }
        }

        // Check for collision exit
        for (int i = 0; i < nearbyPickups.Count; i++) {
            if (!Utilities.ArraysIntersect<Collider>(nearbyColliders, nearbyPickups[i].interactable.Colliders)) {
                nearbyPickups.RemoveAt(i);
                nearbyPickupsChanged = true;
                i--;
                continue;
            }
        }
        ///

        //if (nearbyPickupsChanged) {
            if (nearbyPickups.Count == 0) {
                nearbyInteractableNextPickup = null;
            } else {
                nearbyPickups.Sort();

                for (int i = 0; i < nearbyPickups.Count; i++) {
                    if (nearbyPickups[i].interactable.Weight < MaxPickUpWeight) {
                        nearbyInteractableNextPickup = nearbyPickups[i];
                        break;
                    }
                }
            }
        //}

        // Reset time for interactables that are not currently trying to be picked up
        foreach (NearbyPickup np in nearbyPickups) {
            if (np == nearbyInteractableNextPickup)
                continue;
            np.enterPickupSpaceTime = Time.time;
        }

        if (nearbyPickups.Count > 0) {
            if (ReadyToPickUpInteractable) {
                for (int i = 0; i < nearbyPickups.Count; i++) {
                    if ((holdingObject == null || holdingObject.Weight < nearbyPickups[i].interactable.Weight) && nearbyPickups[i].interactable.Weight < MaxPickUpWeight && Time.time > nearbyPickups[i].enterPickupSpaceTime + PICK_UP_TIME) {
                        if (holdingObject != null) {
                            DropInteractable();
                        }
                        
                        holdingObject = nearbyPickups[i].interactable;
                        holdingObject.PickUp(holdTransform);
                        nearbyPickups.RemoveAt(i);

                        OnPickup();
                        break;
                    }
                }
            }
        }
    }

    public void PunchAnimTrigger() {
        punchAnimTrigger = true;
    }

    public float CalculatePlayerSize(float x) {
        return (10f * Mathf.Log(x + 1f)) + 1f;
    }

    public void ThrowInteractable(Vector2 direction) {
        lastInteractionTime = Time.time;
        anim.SetLayerWeight(anim.GetLayerIndex("Interaction"), 1f);
        anim.SetTrigger("Throw");

        targetLookVector = new Vector3(direction.x, 0f, direction.y);
        targetVelocity = Vector2.zero;

        Utilities.YieldAction(delegate () {
            if (holdingObject != null) {
                humanoid.TempIgnoreCollision(holdingObject.Colliders);
                holdingObject.Drop();
                holdingObject.RB.velocity = new Vector3(direction.x, UnityEngine.Random.Range(0f, 0.2f), direction.y) * THROW_FORCE * playerSize;
                holdingObject = null;

                foreach (NearbyPickup np in nearbyPickups) {
                    np.enterPickupSpaceTime = Time.time;
                }
            }
        }, 0.5f);
    }

    public void DropInteractable() {
        if (holdingObject != null) {
            humanoid.TempIgnoreCollision(holdingObject.Colliders);
            holdingObject.Drop();
            holdingObject = null;

            foreach (NearbyPickup np in nearbyPickups) {
                np.enterPickupSpaceTime = Time.time;
            }
        }
    }

    public virtual void OnRagDollEnable() {
        DropInteractable();
    }
    public virtual void OnRagDollDisable() {
        foreach (NearbyPickup np in nearbyPickups) {
            np.enterPickupSpaceTime = Time.time + 2f;
        }

        //if (moveTarget != null) {
        //    moveTarget.position = humanoid.HipsTransform.position;
        //}
    }

    public virtual void OnPickup() { }

    public virtual void OnDie() { }
    public void Die() {
        OnDie();

        isDead = true;

        humanoid.EnableRagdoll();

        //EffectsHandler.OnPlayerDie(this);

        //SceneHandler.DeRegisterPlayer(this);

        GameObject explosionParticles = GameObject.Instantiate(explosionParticlesPrefab) as GameObject;
        explosionParticles.transform.position = instance.transform.position + (Vector3.up * 0.2f);
        explosionParticles.transform.rotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
        explosionParticles.transform.localScale = Vector3.one * playerSize;
        ParticleSystem.MainModule particleModule = explosionParticles.GetComponent<ParticleSystem>().main;
        particleModule.startColor = explosionParticlesColour;
        Utilities.YieldAction(delegate () {
            GameObject.Destroy(explosionParticles);
        }, 10f);

        //GameObject.Destroy(instance.gameObject);
    }

    public void SetNearbyPickUpSortValFromTargetPosition(Vector3 position, float scaler) {
        foreach (NearbyPickup np in nearbyPickups) {
            np.interactable.sortVal = Vector3.Distance(position, np.interactable.transform.position) * scaler;
        }
    }

    public int CompareTo(PlayerLogic other) {
        return sortVal.CompareTo(other.sortVal);
    }



    public class NearbyPickup : IComparable<NearbyPickup> {
        public Interactable interactable;
        public float enterPickupSpaceTime;

        public NearbyPickup(Interactable _interactable, float _enterPickupSpaceTime) {
            interactable = _interactable;
            enterPickupSpaceTime = _enterPickupSpaceTime;
        }

        public int CompareTo(NearbyPickup other) {
            return (interactable.sortVal - interactable.Weight).CompareTo(other.interactable.sortVal - other.interactable.Weight);
        }
    }
}
