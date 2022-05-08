using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EPOOutline;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Outlinable))]
public class Interactable : MonoBehaviour, System.IComparable<Interactable> {
    [SerializeField]
    private string interactableName;

    private Collider[] colliders;
    private Rigidbody rb;
    private NavMeshObstacle navObstacle;

    private Outlinable outlineable;
    public float viewingOutlinableTime;

    [HideInInspector]
    public bool isHolding;

    private Transform originalParent;
    private Transform holdTransform;
    private Quaternion holdRotation;

    [HideInInspector]
    public float sortVal;

    /// Properties ///
    public Collider[] Colliders { get { return colliders; } }
    public Rigidbody RB { get { return rb; } }
    public float Weight { get { return rb.mass; } }
    public string InteractableName {
        get {
            return interactableName.Replace("[W]",
                (Mathf.RoundToInt(Weight * ((MasterHandler.metricSystem == MetricSystem.Metric) ? 1f : 2.20462f))).ToString() + ((MasterHandler.metricSystem == MetricSystem.Metric) ? " KG" : " lb")
                );
        }
    }
    /// 

    public void Start() {
        colliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        navObstacle = GetComponent<NavMeshObstacle>();
        outlineable = GetComponent<Outlinable>();

        originalParent = transform.parent;

        isHolding = false;
        viewingOutlinableTime = -100f;

        //SceneHandler.RegisterInteractable(this);
    }

    public void Update() {
        outlineable.enabled = Time.time < viewingOutlinableTime + 0.2f;

        if (holdTransform != null) {
            if (!isHolding) {
                transform.position = Vector3.Lerp(transform.position, holdTransform.position, 8f * TimeHandler.CorrectedDeltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, holdTransform.rotation, 8f * TimeHandler.CorrectedDeltaTime);
                if (Vector3.Distance(transform.position, holdTransform.position) < 0.5f) {
                    isHolding = true;
                }
            } else {
                transform.position = holdTransform.position;
                transform.rotation = holdTransform.rotation;
            }
        } else {
            if (transform.position.y < 0f) {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            }
        }
    }

    public void PickUp(Transform _holdTransform) {
        holdTransform = _holdTransform;
        foreach (Collider c in colliders) {
            c.enabled = false;
        }
        rb.isKinematic = true;
        navObstacle.enabled = false;
    }

    public void Drop() {
        foreach (Collider c in colliders) {
            c.enabled = true;
        }
        //collider.isTrigger = true;
        //Utilities.YieldAction(delegate () { collider.isTrigger = false; }, 0.5f);
        rb.isKinematic = false;
        navObstacle.enabled = true;

        isHolding = false;

        holdTransform = null;
    }

    //public void OnTriggerExit(Collider other) {
    //    collider.isTrigger = false;
    //}


    public int CompareTo(Interactable other) {
        return sortVal.CompareTo(other.sortVal);
    }
}
