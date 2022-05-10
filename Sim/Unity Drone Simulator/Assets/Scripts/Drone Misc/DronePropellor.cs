using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePropellor : MonoBehaviour {
    public float propSpeed = 1;

    // Update is called once per frame
    void Update() {
        transform.Rotate(Vector3.forward * propSpeed * 1200 * TimeHandler.CorrectedDeltaTime);
    }
}
