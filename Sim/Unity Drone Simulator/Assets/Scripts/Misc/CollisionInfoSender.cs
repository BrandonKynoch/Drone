using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInfoSender : MonoBehaviour {
    public List<GameObject> receivers = new List<GameObject>();

    public void OnCollisionEnter(Collision collision) {
        foreach (GameObject receiver in receivers) {
            if (receiver.GetComponent<ICollisionInfoReciever>() != null) {
                receiver.GetComponent<ICollisionInfoReciever>().OnExternalCollisionEnter(collision);
            }
        }
    }
}

public interface ICollisionInfoReciever {
    void OnExternalCollisionEnter(Collision collision);
}