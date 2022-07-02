using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneUI : MonoBehaviour {
    public Drone drone;

    [SerializeField]
    private Text idText;

    [SerializeField]
    private Text distFitnessText;
    [SerializeField]
    private Text rotationFitnessText;
    [SerializeField]
    private Text airborneFitnessText;

    private CanvasGroup cg;

    /// CONSTANTS //////////////////////////////////////////////
    private const float VISIBLE_DIST_FROM_CAMERA = 3f;
    ////////////////////////////////////////////////////////////

    public void Start() {
        cg = GetComponent<CanvasGroup>();

        transform.position = drone.transform.position;
        transform.rotation = Quaternion.LookRotation(-(Camera.main.transform.position - transform.position), Vector3.up);
    }

    public void Update() {
        distFitnessText.text = drone.DistFitness.ToString();
        rotationFitnessText.text = drone.RotationFitness.ToString();
        airborneFitnessText.text = drone.AirborneFitness.ToString();


        transform.position = drone.transform.position;
        transform.rotation = Quaternion.LookRotation(-(Camera.main.transform.position - transform.position), Vector3.up);

        float targetAlpha = 1f;
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > VISIBLE_DIST_FROM_CAMERA) {
            targetAlpha = 0f;
        }
        cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, 5f * TimeHandler.CorrectedDeltaTime);
    }

    public void SetID(int id) {
        idText.text = "ID: " + id;
    }
}
