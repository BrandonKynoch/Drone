using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour, IEqualityComparer {
    private DroneData data = new DroneData();

    public DroneData dData { get { return data; } }

    public void UpdateMotorOutputs(double fl, double fr, double br, double bl) {
        data.motorOutputs[DroneData.FL] = fl;
        data.motorOutputs[DroneData.FR] = fr;
        data.motorOutputs[DroneData.BR] = br;
        data.motorOutputs[DroneData.BL] = bl;
    }




    public void Update() {
        // Apply forces to rigidbody

        print("Motors: " + data.motorOutputs[0] + " : " + data.motorOutputs[1] + " : " + data.motorOutputs[2] + " : " + data.motorOutputs[3]);
    }

    /// Object overrides /// 
    bool IEqualityComparer.Equals(object x, object y) {
        if (x == null && y == null)
            return true;

        Drone dx = x as Drone;
        Drone dy = y as Drone;

        if (dx == null || dy == null)
            return false;

        return dx.data.id == dy.data.id;
    }

    int IEqualityComparer.GetHashCode(object obj) {
        return data.HashFunction();
    }
    ///
}


public class DroneData {
    public UInt64 id;

    public double[] motorOutputs = new double[4];

    public const int FL = 0; // Front left
    public const int FR = 1; // Front right
    public const int BR = 2; // Back right
    public const int BL = 3; // Back left

    public int HashFunction() {
        return id.GetHashCode();
    }
}