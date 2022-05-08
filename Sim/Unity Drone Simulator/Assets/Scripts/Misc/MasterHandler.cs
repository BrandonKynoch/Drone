using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterHandler : MonoBehaviour {

    private bool isPlaying;

    /// Static ///
    private static MasterHandler staticInstance;

    public static MetricSystem metricSystem;
    ///

    /// Events ///
    public delegate void CallbackEvent();
    /// 

    /// Properties ///
    public static bool IsPlaying { get { return staticInstance.isPlaying; } }
    /// 

    public void Awake() {
        staticInstance = this;

        metricSystem = MetricSystem.Metric;
    }

    public void Start() {
        isPlaying = true;
    }

    public void Update() {
        isPlaying = true;
    }
}

public enum MetricSystem {
    Imperial,
    Metric
}
