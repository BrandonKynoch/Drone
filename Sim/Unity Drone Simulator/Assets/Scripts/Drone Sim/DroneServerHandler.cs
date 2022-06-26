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
    Socket socket;
    private bool serverConnected = false;

    private byte[] tcpData = new byte[NETWORK_MESSAGE_LENGTH * 10];
    StringBuilder tcpStrBuilder = new StringBuilder();

    private Queue<JObject> inboundMessages = new Queue<JObject>();
    ///

    /// CONSTANTS ///
    private const int SERVER_SOCKET = 1755;

    private const int NETWORK_MESSAGE_LENGTH = 1024;

    private const int MAX_DRONE_COUNT = 40;

    // drone opcodes
    private const int CODE_SPAWN_DRONE = 0x1;
    private const int CODE_MOTOR_OUTPUT = 0x2;

    // Server response opcodes
    public const int RESPONSE_OPCODE_SENSOR_DATA = 0x4;

    private const int SPAWN_ROWS_COUNT = 6;
    private const float SPAWN_SPACING = 0.3f;
    ///

    private void Start() {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

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
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, SERVER_SOCKET);
        TcpListener listener = new TcpListener(endPoint);
        listener.Start();

        print("Waiting for C server to connect");

        socket = listener.AcceptSocket(); // blocks

        serverConnected = true;
        print("C server connected");

        networkStream = new NetworkStream(socket);
        MemoryStream ms = new MemoryStream();
        int numBytesRead;

        while (true) {
            // UNPACK MESSAGES AND ENQUEUE

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
            while ((numBytesRead = networkStream.Read(tcpData, 0, (int) packageSize - numBytesRead)) > 0) {
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

            //NullifyTCPData();
        }
    }

    private void HandleInboundMessages() {
        if (!serverConnected)
            return;

        while (inboundMessages.Count > 0) {
            JObject message = inboundMessages.Dequeue();
            int opCode = -1;

            try {
                opCode = message.GetValue("opcode").Value<int>();
            } catch {
                print("Recieved corrupt packet");

                // TODO: SEND MESSAGE TO SERVER TO RESET
                // Wait for to receive all responses from drones. Then reset and
                // send message to all drones to reset
            }

            JObject jsonOut = null;
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
        newDroneGO.transform.position =
            spawnTransform.position +
            (Vector3.right * (droneCount % SPAWN_ROWS_COUNT) * SPAWN_SPACING) +
            (Vector3.forward * Mathf.FloorToInt(droneCount / SPAWN_ROWS_COUNT) * SPAWN_SPACING);

        newDrone.dData.id = fromJson.GetValue("id").Value<UInt64>();

        drones[newDrone.dData.id] = newDrone;
        droneCount++;

        DroneCamHandler.StaticInstance.SetFocalPoint(newDroneGO.GetComponent<FocalPointObject>());
        MasterHandler.StaticInstance.SetUserMode(MasterHandler.UserMode.DroneCam);

        return newDrone;
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
