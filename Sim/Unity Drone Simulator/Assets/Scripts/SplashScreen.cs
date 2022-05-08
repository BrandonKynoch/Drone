using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour {
    public float speed = 5;
    public float showYield = 0.5f;

    private CanvasGroup cg;

    public void Awake() {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0;
    }

    public void Start() {
        StartCoroutine(Splash());
    }

    private IEnumerator Splash() {
        yield return new WaitForSeconds(1f);
        while (true) {
            cg.alpha = Mathf.Lerp(cg.alpha, 1f, speed * Time.deltaTime);

            if (cg.alpha >= 0.995f) {
                cg.alpha = 1f;
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(showYield);
        while (true) {
            cg.alpha = Mathf.Lerp(cg.alpha, 0f, speed * Time.deltaTime);

            if (cg.alpha <= 0.005f) {
                cg.alpha = 0f;
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("Main");
    }
}
