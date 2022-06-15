using System;
using System.Net.Sockets;

namespace SimServer {
    public class ConnectedDrone {
        public Socket socket;
        public int id;

        public ConnectedDrone(Socket _socket) {
            this.socket = _socket;
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

            droneThread.Start();
        }

        private void DroneMain() {
            while (true) {
                Console.WriteLine("Drone alive: " + id);
                Thread.Sleep(50);
            }
        }
    }
}

