using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventReceiver : MonoBehaviour {
    private PlayerLogic logic;

    private void Start() {
        Utilities.YieldAction(delegate () {
            logic = UserPlayerController.Logic;
        }, 0f);
    }

    public void PunchAnimTrigger() {
        logic.PunchAnimTrigger();
    }
}
