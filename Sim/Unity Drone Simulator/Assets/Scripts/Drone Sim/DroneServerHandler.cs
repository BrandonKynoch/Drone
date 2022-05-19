using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DroneServerHandler : MonoBehaviour {
    public GameObject donePrefab;
    public Transform spawnPosition;

    private Thread serverSocketThread;

    private byte[] tcpData = new byte[NETWORK_MESSAGE_LENGTH];
    StringBuilder tcpStrBuilder = new StringBuilder();

    /// CONSTANTS ///
    private const int NETWORK_MESSAGE_LENGTH = 256;

    private const int CODE_SPAWN_DRONE = 0x1;
    ///

    private void Start() {
        Application.runInBackground = true;
        startSimServer();
    }

    public void Update() {
        HandleIncomingMessage();
    }

    void startSimServer() {
        serverSocketThread = new Thread(SimServerLogic);
        serverSocketThread.IsBackground = true;
        serverSocketThread.Start();
    }

    private void SimServerLogic() {
        TcpListener listener = new TcpListener(1755);
        listener.Start();

        print("Waiting for C server to connect");

        Socket soc = listener.AcceptSocket(); // blocks

        print("C server connected");

        Stream s = new NetworkStream(soc);

        while (true) {
            s.Read(tcpData, 0, NETWORK_MESSAGE_LENGTH);
        }
    }

    private void HandleIncomingMessage() {
        // Cast byte array to string
        foreach (byte b in tcpData) {
            tcpStrBuilder.Append((char)b);
        }
        String message = tcpStrBuilder.ToString();
        tcpStrBuilder.Clear();

        switch (tcpData[0]) {
            case CODE_SPAWN_DRONE:
                // Spawn Drone - Return drone coords here
                break;
            default:
                Debug.LogError("Invalid socket code");
                break;
        }
    }

    private void PrintIPAddrs() {
        string name = Dns.GetHostName();
        try {
            IPAddress[] addrs = Dns.Resolve(name).AddressList;
            foreach (IPAddress addr in addrs)
                print(name + "\t:\t" + addr.ToString());
        } catch (Exception e) {
            print(e.Message);
        }
    }




    private void SpawnDrone() {

    }
}
