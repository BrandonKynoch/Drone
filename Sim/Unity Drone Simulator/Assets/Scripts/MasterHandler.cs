using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class MasterHandler : MonoBehaviour {
    public FocalPointObject playerFocalPoint;
    public Transform droneTarget;
    public Transform environmentRoof;

    private bool isPlaying;

    private UserMode usermode;
    private bool playerInputEnabled;

    public enum UserMode { 
        Player,
        DroneCam,
        FreeCam
    }

    /// Static ///
    private static MasterHandler staticInstance;
    public static MasterHandler StaticInstance {
        get { return staticInstance; }
    }

    public static Transform DroneTarget { get { return staticInstance.droneTarget; } }

    public static MetricSystem metricSystem;
    ///

    /// Events ///
    public delegate void CallbackEvent();

    public static CallbackEvent onUserModeChange;
    /// 

    /// Properties ///
    public static bool IsPlaying { get { return staticInstance.isPlaying; } }
    public static bool PlayerInputEnabled { get { return staticInstance.playerInputEnabled; } }
    public static UserMode CurrentUserMode { get { return staticInstance.usermode; } }

    public static Transform EnvironmentRoof { get { return staticInstance.environmentRoof; } }
    /// 

    public void Awake() {
        staticInstance = this;

        metricSystem = MetricSystem.Metric;

        SetUserMode(UserMode.Player);
    }

    public void Start() {
        isPlaying = true;
    }

    public void Update() {
        isPlaying = true;

        if (Input.GetKeyDown(KeyCode.F1)) {
            SetUserMode(UserMode.Player);
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            SetUserMode(UserMode.FreeCam);
        }
        if (Input.GetKeyDown(KeyCode.F3)) {
            SetUserMode(UserMode.DroneCam);
        }
    }

    public void SetUserMode(UserMode mode) {
        usermode = mode;

        playerInputEnabled = (usermode == UserMode.Player);

        if (usermode == UserMode.FreeCam) {
            //Camera.main.transform.GetComponent<SimpleCameraController>().enabled = true;
            Camera.main.transform.parent.parent.GetComponent<FreeLookCam>().enabled = false;
            Camera.main.transform.parent.parent.GetComponent<ProtectCameraFromWallClip>().enabled = false;
        } else {
            //Camera.main.transform.GetComponent<SimpleCameraController>().enabled = false;
            Camera.main.transform.parent.parent.GetComponent<FreeLookCam>().enabled = true;
            Camera.main.transform.parent.parent.GetComponent<ProtectCameraFromWallClip>().enabled = true;

            if (usermode == UserMode.Player) {
                DroneCamHandler.StaticInstance.SetFocalPoint(playerFocalPoint);
            }
        }

        if (onUserModeChange != null) {
            onUserModeChange();
        }
    }
}

public enum MetricSystem {
    Imperial,
    Metric
}
