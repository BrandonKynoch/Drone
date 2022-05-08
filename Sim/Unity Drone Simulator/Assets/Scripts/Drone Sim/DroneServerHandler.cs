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
    volatile bool keepReading = false;

    private Socket listener;
    private Socket handler;

    private void Start() {
        Application.runInBackground = true;

        // Resolve Hostname and IP
        string name = Dns.GetHostName();
        try {
            IPAddress[] addrs = Dns.Resolve(name).AddressList;
            foreach (IPAddress addr in addrs)
                print(name + "\t:\t" + addr.ToString());
        } catch (Exception e) {
            print(e.Message);
        }

        startSimServer();
    }

    void startSimServer() {
        serverSocketThread = new Thread(SimServerLogic);
        serverSocketThread.IsBackground = true;
        serverSocketThread.Start();
    }

    private void SimServerLogic() {
        TcpListener listener = new TcpListener(1755);
        listener.Start();

        print("TEST - Waiting for connection");

        Socket soc = listener.AcceptSocket(); // blocks

        print("TEST - Connected");

        Stream s = new NetworkStream(soc);
    }







        /*
        void stopServer() {
            keepReading = false;

            //stop thread
            if (serverSocketThread != null) {
                serverSocketThread.Abort();
            }

            if (handler != null && handler.Connected) {
                handler.Disconnect(false);
                Debug.Log("Disconnected!");
            }
        }

        private void SimServerLogic() {
            string data;

            // Data buffer for incoming data
            byte[] bytes = new byte[256];

            // host running the application.
            Debug.Log("Ip " + getIPAddress().ToString());
            IPAddress[] ipArray = Dns.GetHostAddresses(getIPAddress());
            IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], 8042);

            // Create a TCP/IP socket.
            listener = new Socket(ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.

            try {
                listener.Bind(localEndPoint);
                listener.Listen(1755);

                // Start listening for connections.
                while (true) {
                    keepReading = true;

                    // Program is suspended while waiting for an incoming connection.
                    Debug.Log("Waiting for Connection");

                    handler = listener.Accept();
                    Debug.Log("Client Connected");
                    data = null;

                    // An incoming connection needs to be processed.
                    while (keepReading) {
                        bytes = new byte[256];
                        int bytesRec = handler.Receive(bytes);
                        Debug.Log("Received from Server");

                        if (bytesRec <= 0) {
                            keepReading = false;
                            handler.Disconnect(true);
                            break;
                        }

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1) {
                            break;
                        } else {
                            print("Received data: " + data);
                        }

                        System.Threading.Thread.Sleep(1);
                    }

                    System.Threading.Thread.Sleep(1);
                }
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }

        void OnDisable() {
            stopServer();
        }

        private string getIPAddress() {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    localIP = ip.ToString();
                }

            }
            return localIP;
        }*/
    }
