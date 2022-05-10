using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivotController : MonoBehaviour {
    public float lerpSpeed = 5;

    private Vector3 originalOffset;
    private Vector3 targetOffset;

    public void Start() {
        originalOffset = transform.localPosition;
    }

    public void Update() {
        // This script simply controls the pivot point
        // Camera distance is controlled in 'ProtectCameraFromWallClip.cs'
        if (MasterHandler.CurrentUserMode == MasterHandler.UserMode.DroneCam) {
            targetOffset = Vector3.zero;
        } else {
            targetOffset = originalOffset;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, lerpSpeed * Time.deltaTime);
    }
}
