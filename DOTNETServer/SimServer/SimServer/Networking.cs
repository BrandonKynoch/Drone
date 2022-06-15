using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimServer {
    public class Networking {
        /// Singleton pattern //////////////////////////////////////////////////
        private static Networking staticInstance;
        public static Networking StaticInstance {
            get { return staticInstance; }
        }
        /// Singleton pattern //////////////////////////////////////////////////

        /// CONSTANTS //////////////////////////////////////////////////////////
        private const Int32 STD_MSG_LEN = 1024;
        private const int PACKAGE_HEADER_SIZE = 4; // 4 bytes for UInt32 to determine size of the payload

        private const Int32 SIM_SERVER_SOCKET_PORT = 1755;
        private const Int32 DRONE_CONNECTIONS_SOCKET_PORT = 8060;
        /// CONSTANTS //////////////////////////////////////////////////////////

        // Simulation Variables
        private Thread simSendThread;       // Thread dedicated to sending simulation messages
        private Thread simReceiveThread;    // Thread dedicated to receiving messages from the simulation
        private NetworkStream simStream;    // Network stream connected to the simulation
        private byte[] simBuffer = new byte[STD_MSG_LEN * 10];  // Network buffer for the simulation


        // Drone Variables
        private Thread awaitDroneConnectionsThread;     // Thread dedicated to receiving new drone connections


        public Networking() {
            if (staticInstance != null) {
                return;
            }

            /// TODO:
            // Connect drones to server
            // receive drone messages independently, create a list of drone request to send to sim and handle sequentially
            // forward server responses to drones

            /// Unity Connection ////////////////////////////////////////////////////////////////
            //ConnectToUnity();

            //simReceiveThread = new Thread(new ThreadStart(SimNetworkingReceiveLoop));
            //simReceiveThread.Start();

            //while (!simReceiveThread.IsAlive) { }

            //simSendThread = new Thread(new ThreadStart(SimNetworkingSendLoop));
            //simSendThread.Start();
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
            for (int i = 0; i < 25; i++) {
                // TODO: Replace with actual drone requests

                Console.WriteLine("Spawning Drone");
                SendSimMessage("{\"opcode\":\"1\",\"id\":\"" + i + "\"}");

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Receive all network messages coming from unity simulation. Messages are forward to the
        /// corresponding drone according to the drone id
        /// </summary>
        private void SimNetworkingReceiveLoop() {
            while (true) {
                // TODO: Forward messages to corresponding drone

                string response = ReceiveSimMessage();
                Console.WriteLine("\nReceived response from sim:\n" + response + "\n");
            }
        }

        /// <summary> Send simulation a message </summary>
        private void SendSimMessage(string s) {
            SendStringToNetworkStream(simStream, s);
        }

        /// <summary> Receive a message from the simulation </summary>
        private string ReceiveSimMessage() {
            return readStringFromNetworkStream(simStream, simBuffer);
        }
        #endregion

        #region DRONES
        private void AwaitConnecetions() {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, DRONE_CONNECTIONS_SOCKET_PORT);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();

            Console.WriteLine("\nAwaiting drone connections\n");
            while (true) {
                Socket newDroneSocket = listener.AcceptSocket(); // blocks thread
                Console.WriteLine("Drone connected on endpoint: " + newDroneSocket.ToString());

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
        private string readStringFromNetworkStream(NetworkStream stream, byte[] tcpBuffer) {
            MemoryStream ms = new MemoryStream();
            int numBytesRead;

            // Get package size
            ms.SetLength(0);
            numBytesRead = 0;
            while ((numBytesRead = stream.Read(tcpBuffer, 0, PACKAGE_HEADER_SIZE - numBytesRead)) > 0) {
                ms.Write(tcpBuffer, 0, numBytesRead);
            }
            byte[] packageSizeArray = ms.ToArray();

            if (packageSizeArray.Length < PACKAGE_HEADER_SIZE) {
                throw new Exception("Invalid network package header received");
            }

            UInt32 packageSize = BitConverter.ToUInt32(packageSizeArray, 0);

            ms.SetLength(0);
            numBytesRead = 0;
            while ((numBytesRead = stream.Read(tcpBuffer, 0, (int)packageSize - numBytesRead)) > 0) {
                ms.Write(tcpBuffer, 0, numBytesRead);
            }

            return Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
        }

        /// <summary>
        /// Send a message to a network stream
        /// </summary>
        /// <param name="stream"> The stream to send on </param>
        /// <param name="message"> The message to be sent </param>
        private void SendStringToNetworkStream(NetworkStream stream, string message) {
            byte[] buffer = packageString(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Package string with a simple network header that contains the size of the payload
        /// </summary>
        /// <param name="s"> The payload to be packaged </param>
        /// <returns> The packaged payload as an array of bytes </returns>
        private byte[] packageString(string s) {
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

