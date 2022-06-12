using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class DroneServerHandler : MonoBehaviour {
    public GameObject dronePrefab;
    public Transform spawnTransform;

    //private List<Drone> drones = new List<Drone>();
    private Drone[] drones = new Drone[MAX_DRONE_COUNT];
    private int droneCount = 0;

    /// C Server interface ///
    private Thread serverSocketThread;
    Stream networkStream;
    private bool serverConnected = false;

    private byte[] tcpData = new byte[NETWORK_MESSAGE_LENGTH];
    StringBuilder tcpStrBuilder = new StringBuilder();

    private Queue<JObject> inboundMessages = new Queue<JObject>();
    ///

    /// CONSTANTS ///
    private const int SERVER_SOCKET = 1755;

    private const int NETWORK_MESSAGE_LENGTH = 256;

    private const int MAX_DRONE_COUNT = 25;

    // Socket opcodes
    private const int CODE_SPAWN_DRONE = 0x1;
    private const int CODE_MOTOR_OUTPUT = 0x2;


    private const int SPAWN_ROWS_COUNT = 6;
    private const float SPAWN_SPACING = 0.3f;
    ///

    private void Start() {
        Application.runInBackground = true;
        startSimServer();
    }

    public void Update() {
        HandleInboundMessages();
    }

    private void OnApplicationQuit() {
        serverSocketThread.Abort();
    }

    void startSimServer() {
        serverSocketThread = new Thread(SimServerLogic);
        serverSocketThread.IsBackground = true;
        serverSocketThread.Start();
    }

    private void SimServerLogic() {
        TcpListener listener = new TcpListener(SERVER_SOCKET);
        listener.Start();

        print("Waiting for C server to connect");

        Socket soc = listener.AcceptSocket(); // blocks

        serverConnected = true;
        print("C server connected");

        networkStream = new NetworkStream(soc);

        while (true) {
            int bytesRead = networkStream.Read(tcpData, 0, NETWORK_MESSAGE_LENGTH);
            //if ((char)tcpData[bytesRead-1] == '\0') {
            // If null terminator is found then a complete message was parsed
            // MARK: Todo: if bytes read == NETWORK_MESSAGE_LENGTH
            //          then message was probably clipped, we should copy the data
            //          received to a larger buffer and read again, and the concatenate
            //          the data once null terminator is found

            if (bytesRead > 0) {
                tcpStrBuilder.Clear();
                // Cast byte array to string
                foreach (byte b in tcpData)
                {
                    tcpStrBuilder.Append((char)b);
                }
                string message = tcpStrBuilder.ToString();
                JObject jsonIn = null;

                try {
                    jsonIn = JObject.Parse(message);
                }
                catch (Exception e) { }

                if (jsonIn != null) {
                    inboundMessages.Enqueue(jsonIn);
                }

                NullifyTCPData();
            }
            //}
        }
    }

    private void HandleInboundMessages() {
        if (!serverConnected)
            return;

        while (inboundMessages.Count > 0) {
            JObject message = inboundMessages.Dequeue();
            switch (message.GetValue("opcode").Value<int>()) {
                case CODE_SPAWN_DRONE:
                    Drone newDrone = SpawnDrone(message);

                    JObject jsonOut = JObject.FromObject(newDrone.dData);

                    WriteDataToTCPData(jsonOut.ToString());
                    networkStream.Write(tcpData, 0, NETWORK_MESSAGE_LENGTH);
                    NullifyTCPData();
                    break;
                case CODE_MOTOR_OUTPUT:
                    Drone drone = drones[message.GetValue("id").Value<int>()];
                    drone.UpdateMotorOutputs(
                        fl: message.GetValue("motor_fl").Value<double>(),
                        fr: message.GetValue("motor_fr").Value<double>(),
                        br: message.GetValue("motor_br").Value<double>(),
                        bl: message.GetValue("motor_bl").Value<double>()
                        );
                    JObject jsonM = JObject.FromObject(drone.dData);

                    WriteDataToTCPData(jsonM.ToString());
                    networkStream.Write(tcpData, 0, NETWORK_MESSAGE_LENGTH);
                    NullifyTCPData();
                    break;
                default:
                    Debug.LogError("Invalid socket code");
                    break;
            }
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

    private void WriteDataToTCPData(string data) {
        char[] dataChars = data.ToCharArray();
        for (int i = 0; i < data.Length; i++) {
            tcpData[i] = (byte) dataChars[i];
        }
        tcpData[data.Length] = (byte)'\0';
    }

    private void NullifyTCPData() {
        for (int i = 0; i < tcpData.Length; i++) {
            tcpData[i] = 0;
        }
    }


    private Drone SpawnDrone(JObject fromJson) {
        GameObject newDroneGO = GameObject.Instantiate(dronePrefab) as GameObject;
        Drone newDrone = newDroneGO.GetComponent<Drone>();
        newDroneGO.transform.position =
            spawnTransform.position +
            (Vector3.right * (droneCount % SPAWN_ROWS_COUNT) * SPAWN_SPACING) +
            (Vector3.forward * Mathf.FloorToInt(droneCount / SPAWN_ROWS_COUNT) * SPAWN_SPACING);

        newDrone.dData.id = fromJson.GetValue("id").Value<UInt64>();

        drones[newDrone.dData.id] = newDrone;
        droneCount++;

        return newDrone;
    }
}
