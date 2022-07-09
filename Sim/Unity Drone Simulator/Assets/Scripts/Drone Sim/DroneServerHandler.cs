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

    private Drone fittestDrone;

    /// C Server interface ///
    private List<Thread> serverSocketThreads = new List<Thread>();
    Stream networkStream;
    Socket socket;
    private bool serverConnected = false;

    private byte[] tcpData = new byte[NETWORK_MESSAGE_LENGTH * 10];
    StringBuilder tcpStrBuilder = new StringBuilder();

    private Queue<JObject> inboundMessages = new Queue<JObject>();

    private float corruptPacketTimeout;
    public static bool isInTimeoutFromCorruptPacket = false;
    ///

    /// Static ///
    private static DroneServerHandler staticInstance;
    public static DroneServerHandler StaticInstance {
        get { return staticInstance; }
    }
    /// 

    /// CONSTANTS ///
    private const int SERVER_SOCKET = 1755;
    private const float CORRUPT_PACKET_TIMEOUT_DURATION = 0.5f; // After receiving a corrupt packet, disregard all incoming requests for this amount of time

    private const int NETWORK_MESSAGE_LENGTH = 1024;

    private const int MAX_DRONE_COUNT = 40;

    // drone opcodes
    private const int CODE_SPAWN_DRONE = 0x1;
    private const int CODE_MOTOR_OUTPUT = 0x2;
    private const int CODE_RESET_ALL_DRONES = 0x6;

    // Server response opcodes
    public const int RESPONSE_OPCODE_SENSOR_DATA = 0x4;
    public const int REPONSE_OPCODE_RESET_DRONES = 0x7;
    public const int RESPONSE_OPCODE_CORRUPT_PACKET_RECEIVED = 0x8;
    ///

    /// Neural network fitness variables ///
    private float maximumDroneDistFromTarget = 100;
    ///

    /// Properties ///
    public static Drone[] Drones { get { return staticInstance.drones; } }
    public static Drone FittestDrone { get { return staticInstance.fittestDrone; } }

    public static float MaximumDroneDistFromTarget {
        get { return staticInstance.maximumDroneDistFromTarget; }
    }
    /// 

    private void Awake() {
        staticInstance = this;
    }

    private void Start() {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        corruptPacketTimeout = float.NegativeInfinity;

        startSimServer();
    }

    public void FixedUpdate() {
        maximumDroneDistFromTarget = 0;
        foreach (Drone d in drones) {
            if (d != null) {
                float dist = Vector3.Distance(d.transform.position, MasterHandler.DroneTarget.position);
                if (dist > maximumDroneDistFromTarget) {
                    maximumDroneDistFromTarget = dist;
                }
            }
        }
    }

    public void Update() {
        HandleInboundMessages();

        CalculateFittestDrone();

        isInTimeoutFromCorruptPacket = Time.time < staticInstance.corruptPacketTimeout;
    }

    private void OnApplicationQuit() {
        foreach (Thread t in serverSocketThreads) {
            if (t != null) {
                t.Abort();
            }
        }
    }

    void startSimServer() {
        Thread serverSocketThread = new Thread(SimServerLogic);
        serverSocketThread.IsBackground = true;
        serverSocketThreads.Add(serverSocketThread);
        serverSocketThread.Start();
    }

    private void SimServerLogic() {
        int listeningSocket = SERVER_SOCKET + (serverSocketThreads.Count - 1);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, listeningSocket);
        TcpListener listener = new TcpListener(endPoint);
        listener.Start();

        print("Listening for server connection on socket: " + listeningSocket);

        socket = listener.AcceptSocket(); // blocks

        serverConnected = true;
        print("C server connected");

        startSimServer();

        networkStream = new NetworkStream(socket);
        MemoryStream ms = new MemoryStream();
        int numBytesRead;

        while (true) {
            if (isInTimeoutFromCorruptPacket) {
                continue;
            }

            // UNPACK MESSAGES AND ENQUEUE

            try {
                // Get package size
                ms.SetLength(0);
                numBytesRead = 0;
                while ((numBytesRead = networkStream.Read(tcpData, 0, 4 - numBytesRead)) > 0) {
                    ms.Write(tcpData, 0, numBytesRead);
                }
                byte[] packageSizeArray = ms.ToArray();

                if (packageSizeArray.Length < 4) {
                    continue; // Invalid packet header, try again
                }

                UInt32 packageSize = BitConverter.ToUInt32(packageSizeArray, 0);

                ms.SetLength(0);
                numBytesRead = 0;
                while ((numBytesRead = networkStream.Read(tcpData, 0, (int)packageSize - numBytesRead)) > 0) {
                    ms.Write(tcpData, 0, numBytesRead);
                }

                String streamMsg = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);

                JObject jsonIn = null;

                try {
                    jsonIn = JObject.Parse(streamMsg);
                } catch (Exception e) {
                    Debug.Log("Could not parse JSON:\n" + streamMsg);
                }

                if (jsonIn != null) {
                    inboundMessages.Enqueue(jsonIn);
                }
            } catch (Exception e) { }

            //NullifyTCPData();
        }
    }

    private void HandleInboundMessages() {
        if (!serverConnected)
            return;

        while (inboundMessages.Count > 0) {
            JObject message = inboundMessages.Dequeue();
            int opCode = -1;

            JObject jsonOut = null;

            try {
                opCode = message.GetValue("opcode").Value<int>();
            } catch {
                Debug.LogError("Corrupt packet received - Resetting");

                inboundMessages.Clear();

                corruptPacketTimeout = Time.time + CORRUPT_PACKET_TIMEOUT_DURATION;

                jsonOut = new JObject();
                jsonOut.Add(new JProperty("opcode", RESPONSE_OPCODE_CORRUPT_PACKET_RECEIVED));
                sendCServerData(jsonOut);

                continue;
            }

            switch (opCode) {
                case CODE_SPAWN_DRONE:
                    Drone newDrone = SpawnDrone(message);

                    jsonOut = JObject.FromObject(newDrone.dData);
                    jsonOut.Add(new JProperty("opcode", RESPONSE_OPCODE_SENSOR_DATA));
                    sendCServerData(jsonOut);
                    break;
                case CODE_MOTOR_OUTPUT:
                    Drone drone = drones[message.GetValue("id").Value<int>()];
                    drone.UpdateMotorOutputs(
                        fl: message.GetValue("motor_fl").Value<double>(),
                        fr: message.GetValue("motor_fr").Value<double>(),
                        br: message.GetValue("motor_br").Value<double>(),
                        bl: message.GetValue("motor_bl").Value<double>()
                        );

                    jsonOut = JObject.FromObject(drone.dData);
                    jsonOut.Add(new JProperty("opcode", RESPONSE_OPCODE_SENSOR_DATA));
                    sendCServerData(jsonOut);
                    break;
                case CODE_RESET_ALL_DRONES:
                    print("Resetting all drones for next epoch");

                    ResetAllDrones();

                    jsonOut = new JObject();
                    jsonOut.Add(new JProperty("opcode", REPONSE_OPCODE_RESET_DRONES));
                    sendCServerData(jsonOut);
                    break;
                case -1:
                    // TODO: Send server message to handle broken request
                    // server needs to find drone that couldn't send message and reply with empty reponse
                    //JObject jsonM = JObject.FromObject(drone.dData);
                    //sendCServerData(jsonM);
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
        newDroneGO.transform.SetParent(spawnTransform);
        Drone newDrone = newDroneGO.GetComponent<Drone>();
        

        newDrone.dData.id = fromJson.GetValue("id").Value<UInt64>();

        drones[newDrone.dData.id] = newDrone;
        droneCount++;

        DroneCamHandler.StaticInstance.SetFocalPoint(newDroneGO.GetComponent<FocalPointObject>());
        MasterHandler.StaticInstance.SetUserMode(MasterHandler.UserMode.DroneCam);

        newDrone.ResetDrone(spawnTransform.position);

        return newDrone;
    }

    private void ResetAllDrones() {
        float maxRange = 30f;
        MasterHandler.DroneTarget.position = new Vector3(
            30 + UnityEngine.Random.Range(-maxRange, maxRange),
            MasterHandler.DroneTarget.position.y,
            UnityEngine.Random.Range(-maxRange, maxRange));
        foreach (Drone d in drones) {
            if (d != null) {
                d.ResetDrone(spawnTransform.position);
            }
        }
    }

    private void CalculateFittestDrone() {
        if (fittestDrone == null) {
            foreach (Drone d in drones) {
                if (d != null) {
                    fittestDrone = d;
                    break;
                }
            }
        }

        foreach (Drone d in drones) {
            if (d != null) {
                if (d.dData.fitness > fittestDrone.dData.fitness) {
                    fittestDrone = d;
                }
            }
        }
    }






    private void sendCServerData(JObject data) {
        byte[] buffer = packageString(data.ToString());
        networkStream.Write(buffer, 0, buffer.Length);
    }

    private byte[] packageString(string s) {
        byte[] asciiBuffer = Encoding.ASCII.GetBytes(s);
        byte[] packageBuffer = new byte[asciiBuffer.Length + sizeof(UInt32)];
        byte[] sizeOfString = BitConverter.GetBytes((UInt32) s.Length);
        Buffer.BlockCopy(sizeOfString, 0, packageBuffer, 0, sizeOfString.Length);
        Buffer.BlockCopy(asciiBuffer, 0, packageBuffer, sizeOfString.Length, asciiBuffer.Length);
        return packageBuffer;
    }
}
