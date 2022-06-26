using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SimServer {
    public class Networking {
        /// SINGLETON PATTERN //////////////////////////////////////////////////
        private static Networking staticInstance;
        public static Networking StaticInstance {
            get { return staticInstance; }
        }
        /// SINGLETON PATTERN //////////////////////////////////////////////////

        /// CONSTANTS //////////////////////////////////////////////////////////
        // When true the DOTNET server will emulate responses from the server so that unity doesn't have to be running
        // NOTE: FAKE SIMULATION DOES NOT SUPPORT MULTIPLE DRONES!!!
        public const bool USE_FAKE_SIM = false;

        public const Int32 STD_MSG_LEN = 1024;
        private const int PACKAGE_HEADER_SIZE = 4; // 4 bytes for UInt32 to determine size of the payload

        private const Int32 SIM_SERVER_SOCKET_PORT = 1755;
        private const Int32 DRONE_CONNECTIONS_SOCKET_PORT = 8060;


        private const string FAKE_SIM_RESPONSE = "{\"id\":0,\"motorOutputs\":[0.0,0.0,0.0,0.0],\"sensorData\":[0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0]}";
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// SIMULATION VARIABLES ///////////////////////////////////////////////
        private Thread simSendThread;           // Thread dedicated to sending simulation messages
        private Thread simReceiveThread;        // Thread dedicated to receiving messages from the simulation
        private NetworkStream simStream;        // Network stream connected to the simulation
        private byte[] simReceiveBuffer = new byte[STD_MSG_LEN * 10];  // Network buffer for the simulation
        private Queue<DroneMessage> outgoingDroneMessageRequests = new Queue<DroneMessage>();

        private Thread awaitDroneConnectionsThread;     // Thread dedicated to receiving new drone connections
        /// SIMULATION VARIABLES ///////////////////////////////////////////////
        

        public Networking() {
            if (staticInstance != null) {
                return;
            }
            staticInstance = this;

            /// Unity Connection ////////////////////////////////////////////////////////////////
            if (!USE_FAKE_SIM) {
                ConnectToUnity();

                simReceiveThread = new Thread(new ThreadStart(SimNetworkingReceiveLoop));
                simReceiveThread.Priority = ThreadPriority.AboveNormal;
                simReceiveThread.Start();

                while (!simReceiveThread.IsAlive) { }
            } else {
                Console.WriteLine("\n##########################################");
                Console.WriteLine("###          FAKE SIM ACTIVE           ###");
                Console.WriteLine("##########################################\n\n");
            }

            simSendThread = new Thread(new ThreadStart(SimNetworkingSendLoop));
            simSendThread.Priority = ThreadPriority.AboveNormal;
            simSendThread.Start();
            /// Unity Connection ////////////////////////////////////////////////////////////////

            awaitDroneConnectionsThread = new Thread(new ThreadStart(AwaitConnecetions));
            awaitDroneConnectionsThread.Start();
        }

        #region SIMULATION
        /// <summary>
        /// Establish connection with unity simulation
        /// </summary>
        private void ConnectToUnity() {
            TcpClient simSocket = new TcpClient("127.0.0.1", SIM_SERVER_SOCKET_PORT);
            simStream = simSocket.GetStream();
        }

        /// <summary>
        /// Forward all drone message requests to unity simulation
        /// As well as any other network requests that need to be sent to unity simulation
        /// </summary>
        private void SimNetworkingSendLoop() {
            DroneMessage nullMSG = new DroneMessage();
            bool msgIsNull = true;
            while (true) {
                DroneMessage msg = nullMSG;
                msgIsNull = true;

                lock (outgoingDroneMessageRequests) {
                    if (outgoingDroneMessageRequests.Count > 0) {
                        msg = outgoingDroneMessageRequests.Dequeue();
                        msgIsNull = false;
                    }
                }

                if (!msgIsNull) {
                    JObject responseJson = JObject.Parse(msg.message);

                    int opCode = responseJson.GetValue("opcode").Value<int>();
                    ConnectedDrone responseDrone = Master.GetDrone(responseJson.GetValue("id").Value<int>()).Drone;

                    bool forwardToServer;
                    switch (opCode) {
                        case Master.OPCODE_REQUEST_TARGET_NN_FROM_SERVER:
                            forwardToServer = false;
                            NeuralTrainer.StaticInstance.dronesWaitingToReceiveNN.Enqueue(responseDrone);
                            break;
                        default:
                            forwardToServer = true;
                            break;
                    }

                    if (forwardToServer) {
                        if (NeuralTrainer.ContinueSimulation || opCode == Master.OPCODE_SPAWN_DRONE) {
                            // No action needed from server -> forward message directly to simulation
                            if (!USE_FAKE_SIM) {
                                SendSimStreamMessage(msg.message);
                            } else {
                                // Immediately forward to drone for emulated simulation
                                JObject forwardMessageJson = JObject.Parse(FAKE_SIM_RESPONSE);
                                responseDrone.ReceiveMessageFromSimulation(forwardMessageJson);
                            }
                        } else {
                            // TODO: add drone to queue of drones waiting for server response
                            Console.WriteLine("FIX THIS");
                        }
                    }
                }

                Thread.Yield();
            }
        }

        /// <summary>
        /// Receive all network messages coming from unity simulation. Messages are forward to the
        /// corresponding drone according to the drone id
        /// </summary>
        private void SimNetworkingReceiveLoop() {
            while (true) {
                string response = ReceiveSimStreamMessage();
                JObject responseJson = JObject.Parse(response);
                ConnectedDrone responseDrone = Master.GetDrone(responseJson.GetValue("id").Value<int>()).Drone;
                responseDrone.ReceiveMessageFromSimulation(responseJson);

                Thread.Yield();
            }
        }

        /// <summary>
        /// Send simulation a message
        /// This function actually sends data to the network stream
        /// Should not be exposed outside of networking class
        /// </summary>
        private void SendSimStreamMessage(string s) {
            SendStringToNetworkStream(simStream, s);
        }

        /// <summary>
        /// Receive a message from the simulation
        /// This function actually receives the message from the network stream and returns the string value
        /// Should not be exposed outside of networking class
        /// </summary>
        private string ReceiveSimStreamMessage() {
            return ReadStringFromNetworkStream(simStream, simReceiveBuffer);
        }

        /// <summary>
        /// Allows a drone to forward a message to the simulation through the simulation network stream
        /// </summary>
        /// <param name="s"></param>
        public static void SendToSim(ConnectedDrone drone, string s) {
            DroneMessage droneMessage = new DroneMessage(drone, s);
            lock (staticInstance.outgoingDroneMessageRequests) {
                StaticInstance.outgoingDroneMessageRequests.Enqueue(droneMessage);
            }
        }

        private struct DroneMessage {
            public ConnectedDrone drone;
            public string message;

            public DroneMessage(ConnectedDrone _drone, string _message) {
                this.drone = _drone;
                this.message = _message;
            }
        }
        #endregion

        #region DRONES
        private void AwaitConnecetions() {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, DRONE_CONNECTIONS_SOCKET_PORT);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();

            Console.WriteLine("\n##########################################");
            Console.WriteLine("###    AWAITING DRONE CONNECTIONS      ###");
            Console.WriteLine("##########################################\n");
            while (true) {
                Socket newDroneSocket = listener.AcceptSocket(); // blocks thread
                ConnectedDrone.InitializeDrone(newDroneSocket);
            }
        }
        #endregion

        #region NETWORKING_FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //////                                                                            //////////////////
        //////   PACKAGE DESIGN USES FIRST 4 BYTES FOR PACKAGE SIZE HEADER                //////////////////
        //////                                                                            //////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read the next message from a network stream
        /// </summary>
        /// <param name="stream"> The stream to read from </param>
        /// <param name="tcpBuffer"> The TMP buffer to store the stream data in </param>
        /// <returns> The next message received on the network stream as a string </returns>
        /// <exception cref="Exception"></exception>
        public static string ReadStringFromNetworkStream(NetworkStream stream, byte[] tcpBuffer) {
            MemoryStream ms = new MemoryStream();
            int numBytesRead;

            // Get package size
            ms.SetLength(0);
            numBytesRead = 0;
            while ((numBytesRead = stream.Read(tcpBuffer, 0, PACKAGE_HEADER_SIZE - numBytesRead)) > 0) {
                ms.Write(tcpBuffer, 0, numBytesRead);
                if (PACKAGE_HEADER_SIZE - numBytesRead == 0) {
                    break;
                }
            }

            byte[] packageSizeArray = ms.ToArray();

            if (packageSizeArray.Length < PACKAGE_HEADER_SIZE) {
                throw new Exception("Invalid network package header received");
            }
            
            UInt32 packageSize = BitConverter.ToUInt32(packageSizeArray, 0);

            ms = new MemoryStream();
            ms.SetLength(0);
            numBytesRead = 0;
            while ((numBytesRead = stream.Read(tcpBuffer, 0, (int)packageSize - numBytesRead)) > 0) {
                ms.Write(tcpBuffer, 0, numBytesRead);
                if ((int)packageSize - numBytesRead == 0) {
                    break;
                }
            }

            return Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
        }

        /// <summary>
        /// Send a message to a network stream
        /// </summary>
        /// <param name="stream"> The stream to send on </param>
        /// <param name="message"> The message to be sent </param>
        public static void SendStringToNetworkStream(NetworkStream stream, string message) {
            byte[] buffer = PackageString(message);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        /// <summary>
        /// Package string with a simple network header that contains the size of the payload
        /// </summary>
        /// <param name="s"> The payload to be packaged </param>
        /// <returns> The packaged payload as an array of bytes </returns>
        private static byte[] PackageString(string s) {
            byte[] asciiBuffer = Encoding.ASCII.GetBytes(s);
            byte[] packageBuffer = new byte[asciiBuffer.Length + sizeof(UInt32)];
            byte[] sizeOfString = BitConverter.GetBytes(s.Length);
            Buffer.BlockCopy(sizeOfString, 0, packageBuffer, 0, sizeOfString.Length);
            Buffer.BlockCopy(asciiBuffer, 0, packageBuffer, sizeOfString.Length, asciiBuffer.Length);
            return packageBuffer;
        }
        #endregion
    }
}

