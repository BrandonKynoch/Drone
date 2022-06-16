using System;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimServer {
    public class ConnectedDrone {
        // Networking
        private Socket socket;
        private NetworkStream netStream;
        private byte[] tcpBuffer = new byte[Networking.STD_MSG_LEN * 10];

        private Queue<JObject> pendingSimResponses = new Queue<JObject>();

        public int id;

        // Debug vars
        DateTime startTime;

        /// Properties ////////////////////////////////////////////////////////////////////////////
        public TimeSpan UpTime { get { return DateTime.Now - startTime; } }
        /// Properties ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize the drone
        /// Drone execution is only started at a later time in Drone main
        /// </summary>
        /// <param name="_socket"></param>
        public ConnectedDrone(Socket _socket) {
            this.socket = _socket;
            this.netStream = new NetworkStream(_socket);
        }

        /// <summary>
        /// This function should be called from the Networking thread.
        /// A new drone thread and connectedDrone instance will be created and added to the Master instance
        /// </summary>
        /// <param name="fromSocket"> The socket that the drone connection is established on </param>
        public static void InitializeDrone(Socket fromSocket) {
            ConnectedDrone drone = new ConnectedDrone(fromSocket);
            Thread droneThread = new Thread(new ThreadStart(drone.DroneMain));
            DroneThreadPair droneThreadPair = new DroneThreadPair(drone, droneThread);

            drone.id = Master.StaticInstance.AddDroneToServer(droneThreadPair);
            Console.WriteLine("Drone ID: " + drone.id);

            droneThread.Start();
        }

        private void DroneMain() {
            startTime = DateTime.UtcNow;

            // Respond to drone with drone id in Master
            // Drone still hasn't spawned in unity yet
            JObject initResponse = new JObject(new JProperty("id", id));
            Networking.SendStringToNetworkStream(netStream, initResponse.ToString());

            string response = Networking.ReadStringFromNetworkStream(netStream, tcpBuffer);
            JObject responseJson = JObject.Parse(response);
            if (responseJson.GetValue("opcode").Value<int>() != Master.OPCODE_SPAWN_DRONE) {
                throw new Exception("Invalid opcode while establishing connetion");
            }

            Networking.SendToSim(this, response); // TODO: sleep thread after calling sendtosim

            while (true) {
                lock (pendingSimResponses) {
                    if (pendingSimResponses.Count > 0) {
                        JObject simResponse = pendingSimResponses.Dequeue();
                        Console.WriteLine(simResponse.ToString());
                        SendDroneMessage(simResponse.ToString());

                        JObject droneResponse = ReadDroneMessage();

                        Networking.SendToSim(this, droneResponse.ToString());
                    }
                }

                //Console.Write("A");
            }
        }

        private void SendDroneMessage(string message) {
            Networking.SendStringToNetworkStream(netStream, message);
        }

        private JObject ReadDroneMessage() {
            string response = Networking.ReadStringFromNetworkStream(netStream, tcpBuffer);
            JObject responseJson = JObject.Parse(response);
            return responseJson;
        }

        public void ReceiveMessageFromSimulation(JObject json) {
            lock (pendingSimResponses) {
                pendingSimResponses.Enqueue(json);
            }
            // TODO: wake up thread here
            //Master.GetDrone(id).DroneThread.Resume();
        }
    }
}

