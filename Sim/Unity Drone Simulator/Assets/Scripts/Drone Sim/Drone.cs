using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {
    private DroneData data = new DroneData();

    public DroneData GetDroneData { get { return data; } }
}


public class DroneData {
    public UInt64 id;

    public string random = "donkey";
    public int testKak = 73;
}