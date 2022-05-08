using UnityEngine;
using System.Collections;

public class ScreenCapture : MonoBehaviour {
#if UNITY_EDITOR
    public KeyCode keyToPress = KeyCode.K;
    public int resolutionModifier = 1;
    public string prefix = "ss";

    bool takePicture = false;

    void Start() {
        if (!System.IO.Directory.Exists(Application.dataPath + "/../Screenshots")) {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/../Screenshots");
        }

        StartCoroutine(ContinuousCapture());
    }

    private IEnumerator ContinuousCapture() {
        while (true) {
            takePicture = true;
            yield return new WaitForSeconds(2f);
        }
    }

    void Update() {
        if (Input.GetKeyDown(keyToPress)) {
            takePicture = true;
        }

        if (takePicture) {
            string dateTime = System.DateTime.Now.Month.ToString() + "-" +
                System.DateTime.Now.Day.ToString() + "_" +
                System.DateTime.Now.Hour.ToString() + "-" +
                System.DateTime.Now.Minute.ToString() + "-" +
                System.DateTime.Now.Second.ToString();
            string filename = prefix + "_" + dateTime + ".png";
            UnityEngine.ScreenCapture.CaptureScreenshot((Application.dataPath + "/../Screenshots/" + filename), resolutionModifier);
            takePicture = false;
        }
    }

    public void OnPostRender() {

    }
#endif
}