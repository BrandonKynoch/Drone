using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HumanoidUI : MonoBehaviour {
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Text massText;

    private Humanoid humanoid;

    private CanvasGroup cg;

    private Vector3 offset;
    private Vector3 originalScale;

    /// Constants ///
    private const float ROTATION_PROJECTION_DIST = 1f;

    private const float VISIBLE_DIST_FROM_CAMERA = 5f;
    /// 

    public void Initialize(Humanoid _humanoid, PlayerLogic _logic) {
        humanoid = _humanoid;

        cg = GetComponent<CanvasGroup>();

        offset = (Vector3.up * 2.25f);
        transform.position = humanoid.transform.position + offset;
        transform.rotation = Quaternion.LookRotation(-(Camera.main.transform.position - transform.position), Vector3.up);
        originalScale = transform.localScale;
    }

    public void Update() {
        if (humanoid.gameObject.activeSelf) {
            if (humanoid.IsAlive && MasterHandler.IsPlaying) {
                healthSlider.value = humanoid.HealthPercentage;

                massText.text = humanoid.Logic.PlayerMassString;



                transform.position = humanoid.transform.position + offset * humanoid.Logic.PlayerSize;
                transform.rotation = Quaternion.LookRotation(-(Camera.main.transform.position - transform.position), Vector3.up);
                transform.localScale = originalScale * humanoid.Logic.PlayerSize;

                float targetAlpha = (humanoid.IsRagdoll) ? 0f : 1f;
                if (Vector3.Distance(transform.position, Camera.main.transform.position) > VISIBLE_DIST_FROM_CAMERA * Mathf.Lerp(UserPlayerController.PlayerSize, 1f, 0.9f)) {
                    targetAlpha = 0f;
                }
                cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, 5f * TimeHandler.CorrectedDeltaTime);
            } else {
                cg.alpha = Mathf.Lerp(cg.alpha, 0f, 5f * TimeHandler.CorrectedDeltaTime);
            }
        } else {
            cg.alpha = 0f;
        }
    }
}
