using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class DroneCamHandler : MonoBehaviour {
    public Transform focusTargetsRoot;

    private FreeLookCam freeLookCam;
    private FocalPointObject currentFocalPoint;

    private Transform originalFocus;

    private static DroneCamHandler staticInstance;
    public static DroneCamHandler StaticInstance {
        get { return staticInstance; }
    }

    public void Start() {
        staticInstance = this;

        freeLookCam = GetComponent<FreeLookCam>();
        originalFocus = freeLookCam.Target;

        MasterHandler.onUserModeChange += OnUserModeChange;
        ChangeFocalPoint(0);
    }

    public void Update() {
        if (MasterHandler.CurrentUserMode == MasterHandler.UserMode.DroneCam) {
            if (Input.GetKeyDown(KeyCode.O)) {
                ChangeFocalPoint(-1);
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                ChangeFocalPoint(1);
            }
        }
    }

    private void ChangeFocalPoint(int relativeOffset) {
        FocalPointObject[] focalPoints = focusTargetsRoot.GetComponentsInChildren<FocalPointObject>();

        if (focalPoints.Length == 0) {
            return;
        }

        if (currentFocalPoint == null) {
            SetFocalPoint(focalPoints[0]);
            return;
        }

        int indexOfCurrent = -1;
        for (int i = 0; i < focalPoints.Length; i++) {
            if (focalPoints[i] == currentFocalPoint) {
                indexOfCurrent = i;
                break;
            }
        }
        if (indexOfCurrent != -1) {
            int newIndex = (indexOfCurrent + relativeOffset) % (focalPoints.Length);
            if (newIndex < 0)
                newIndex = focalPoints.Length - 1;
            SetFocalPoint(focalPoints[newIndex]);
        }
    }

    public void SetFocalPoint(FocalPointObject focalPoint) {
        currentFocalPoint = focalPoint;
        freeLookCam.SetTarget(currentFocalPoint.transform);
    }

    private void OnUserModeChange() {
        if (MasterHandler.CurrentUserMode != MasterHandler.UserMode.DroneCam) {
            freeLookCam.SetTarget(originalFocus);
        } else {
            originalFocus = freeLookCam.Target;
            ChangeFocalPoint(0);
        }
    }
}
