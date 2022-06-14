using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimServer {
    class Application {
        /// Singleton pattern //////////////////////////////////////////////////
        private static Application staticInstance;
        public static Application StaticInstance {
            get { return staticInstance; }
        }
        /// Singleton pattern //////////////////////////////////////////////////


        /// CONSTANTS //////////////////////////////////////////////////////////
        private const Int32 STD_MSG_LEN = 1024;
        private const int PACKAGE_HEADER_SIZE = 4; // 4 bytes for UInt32 to determine size of the payload

        private const Int32 SIM_SERVER_SOCKET_PORT = 1755;
        /// CONSTANTS //////////////////////////////////////////////////////////

        private Thread simSendThread;
        private Thread simReceiveThread;
        private NetworkStream simStream;
        private byte[] simBuffer = new byte[STD_MSG_LEN * 10];

        static void Main(string[] args) {
            // Initialize instance
            staticInstance = new Application();
        }


        /// TODO: MOVE TO NETWORKING CLASS
        private Application() {
            if (staticInstance != null) {
                return;
            }

            ConnectToUnity();

            simReceiveThread = new Thread(new ThreadStart(SimNetworkingReceiveLoop));
            simReceiveThread.Start();

            while (!simReceiveThread.IsAlive) { }

            simSendThread = new Thread(new ThreadStart(SimNetworkingSendLoop));
            simSendThread.Start();
        }

        private void ConnectToUnity() {
            TcpClient simSocket = new TcpClient("127.0.0.1", SIM_SERVER_SOCKET_PORT);
            simStream = simSocket.GetStream();
        }

        private void SimNetworkingSendLoop() {
            for (int i = 0; i < 25; i++) {
                Console.WriteLine("Spawning Drone");
                SendSimMessage("{\"opcode\":\"1\",\"id\":\"" + i + "\"}");
            }
        }

        private void SimNetworkingReceiveLoop() {
            while (true) {
                string response = ReceiveSimMessage();
                Console.WriteLine("\nReceived response from sim:\n" + response + "\n");
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //////                                                                            //////////////////
        //////   PACKAGE DESIGN USES FIRST 4 BYTES FOR PACKAGE SIZE HEADER                //////////////////
        //////                                                                            //////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void SendSimMessage(string s) {
            byte[] buffer = packageString(s);
            simStream.Write(buffer, 0, buffer.Length);
        }

        private string ReceiveSimMessage() {
            return readStringFromNetworkStream(simStream, simBuffer);
        }

        private byte[] packageString(string s) {
            byte[] asciiBuffer = Encoding.ASCII.GetBytes(s);
            byte[] packageBuffer = new byte[asciiBuffer.Length + sizeof(UInt32)];
            byte[] sizeOfString = BitConverter.GetBytes(s.Length);
            Buffer.BlockCopy(sizeOfString, 0, packageBuffer, 0, sizeOfString.Length);
            Buffer.BlockCopy(asciiBuffer, 0, packageBuffer, sizeOfString.Length, asciiBuffer.Length);
            return packageBuffer;
        }

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
    }
}