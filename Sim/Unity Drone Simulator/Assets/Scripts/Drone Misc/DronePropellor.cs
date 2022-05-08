using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePropellor : MonoBehaviour {
    public float propSpeed = 10;

    // Update is called once per frame
    void Update() {
        transform.Rotate(Vector3.forward * propSpeed * 360 * TimeHandler.CorrectedDeltaTime);
    }
}
