using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {
    public static InputEvent OnTouchDown;
    public static InputEvent OnTouchUp;

    public delegate void InputEvent();

    private static Vector2 touchDownPosition;
    private static Vector2 currentTouchPosition;
    private static float touchDownTime;

    private static bool isHolding;

    private static float resolutionScaler;

    /// Properties ///
    public static bool IsHolding { get { return isHolding; } }
    public static Vector2 TouchPosition { get { return currentTouchPosition; } }
    public static Vector2 TouchDownPositionNormalized { get { return new Vector2(touchDownPosition.x / Screen.width, touchDownPosition.y / Screen.height); } }
    public static Vector2 TouchPositionPercent { get { return new Vector2((float) currentTouchPosition.x / Screen.width, (float) currentTouchPosition.y / Screen.height); } }
    public static Vector2 TouchPositionPercentAspectCorrected { get { return new Vector2((float) currentTouchPosition.x / Screen.width, (float) currentTouchPosition.y / Screen.width); } }

    // Returned values are as a percentage of screen width
    public static Vector2 HoldDeltaPos {
        get {
#if UNITY_EDITOR
            if (Input.touchCount == 0) {
                if (isHolding) {
                    return (currentTouchPosition - touchDownPosition) * resolutionScaler;
                } else {
                    return Vector2.zero;
                }
            }
#endif

            if (isHolding) {
                return (currentTouchPosition - touchDownPosition) * resolutionScaler;
            } else {
                return Vector2.zero;
            }
        }
    }
    public static float HoldDuration { get { return Time.time - touchDownTime; } }
    /// 

    public void Awake() {
        resolutionScaler = (1f / Screen.width);
        isHolding = false;
    }

    public void Update() {
        if (MasterHandler.IsPlaying) {

#if UNITY_EDITOR
            if (Input.touchCount == 0) {
                if (Input.GetMouseButton(0)) {
                    currentTouchPosition = Input.mousePosition;
                }
                if (Input.GetMouseButtonDown(0)) {
                    touchDownPosition = Input.mousePosition;
                    touchDownTime = Time.time;
                    LocalOnTouchDown();
                }
                if (Input.GetMouseButtonUp(0)) {
                    LocalOntouchUp();
                }
            }
#endif

            if (Input.touchCount > 0) {
                currentTouchPosition = Input.GetTouch(0).position;
                if (Input.GetTouch(0).phase == TouchPhase.Began) {
                    touchDownPosition = Input.GetTouch(0).position;
                    touchDownTime = Time.time;
                    LocalOnTouchDown();
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended) {
                    LocalOntouchUp();
                }
            } else {
                touchDownTime = Time.time;
            }
        }
    }

    public void LocalOnTouchDown() {
        isHolding = true;

        if (OnTouchDown != null)
            OnTouchDown();
    }

    public void LocalOntouchUp() {
        if (OnTouchUp != null)
            OnTouchUp();

        isHolding = false;
    }
}
