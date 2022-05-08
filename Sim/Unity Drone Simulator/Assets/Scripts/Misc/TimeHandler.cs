using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeHandler : MonoBehaviour {
    public float slowMotionTimeScale = 0.1f;
    public float lerpSpeed = 4f;

    private bool isOverwritingTimeScale;

    private static TimeHandler staticInstance;

    private bool slowMotion;

    private float timeScale;
    public static float TimeScale {
        get { return staticInstance.timeScale; }
    }

    public static float TimeScaleLerpSpeed { get { return staticInstance.lerpSpeed; } }

    private float targetTimeScale;

    public void Awake() {
        staticInstance = this;
        timeScale = 1f;
        targetTimeScale = 1f;
        isOverwritingTimeScale = false;

        slowMotion = false;
    }

    public void Update() {
        if (slowMotion) {
            targetTimeScale = slowMotionTimeScale;
        } else {
            targetTimeScale = 1f;
        }

        if (!isOverwritingTimeScale) {
            timeScale = Mathf.Lerp(timeScale, targetTimeScale, lerpSpeed * Time.deltaTime);
        }

        timeScale = Mathf.Clamp01(timeScale);

        //Time.timeScale = timeScale;
        // 0.034 = updates 30 times per second in game
        Time.timeScale = timeScale;
    }

    public static void SlowMotion() {
        staticInstance.slowMotion = true;
    }

    public static void CancelSlowMotion() {
        staticInstance.slowMotion = false;
    }

    public static float CorrectedDeltaTime {
        get { return Time.deltaTime * TimeScale; }
    }
    
    public static void OverwriteTimeScale(float val) {
        staticInstance.isOverwritingTimeScale = true;
        staticInstance.timeScale = val;
    }
    
    public static void CancelOverwriteTimeScale() {
        staticInstance.isOverwritingTimeScale = false;
    }
}
