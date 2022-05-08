using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsHandler : MonoBehaviour {
    [SerializeField]
    private GameObject[] punchEffectsPrefabs;

    private RandomInstancePool punchEffectsPool;

    private static EffectsHandler staticInstance;

    /// Constants ///
    private const int PUNCH_EFFECT_POOL_SIZE = 25;
    /// 

    public void Start() {
        punchEffectsPool = new RandomInstancePool(punchEffectsPrefabs, "Punch FX", PUNCH_EFFECT_POOL_SIZE);

        staticInstance = this;
    }

    public static void PunchEffect(Vector3 position, float playerSize) {
        RandomInstancePool.Instance instance = staticInstance.punchEffectsPool.GetInnactiveInstance();
        if (instance != null) {
            instance.SetActive(true);
            ParticleSystem ps = instance.GOInstance.GetComponent<ParticleSystem>();
            ps.transform.position = position;
            ps.transform.localScale = Vector3.one * 0.5f * playerSize;
            ps.Play();
            Utilities.YieldAction(delegate () {
                instance.SetActive(false);
            }, 1.5f);
        }
    }
}
