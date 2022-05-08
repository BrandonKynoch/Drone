using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartFade : MonoBehaviour {
    public void Start() {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.enabled = true;
        cg.alpha = 1f;

        Utilities.FadeUI(cg, 0f, 4f, false, delegate () {
            Destroy(gameObject);
        });
    }
}
