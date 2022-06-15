using System;
using System.Net.Sockets;

namespace SimServer {
    class Master {
        /// Singleton pattern //////////////////////////////////////////////////
        private static Master staticInstance;
        public static Master StaticInstance {
            get { return staticInstance; }
        }
        /// Singleton pattern //////////////////////////////////////////////////

        /// CONSTANTS //////////////////////////////////////////////////////////
        private const int MAXIMUM_DRONE_CONNECTIONS = 50;
        /// CONSTANTS //////////////////////////////////////////////////////////

        private DroneThreadPair[] drones = new DroneThreadPair[MAXIMUM_DRONE_CONNECTIONS];
        private int droneCount = 0;

        static void Main(string[] args) {
            // Initialize instance
            staticInstance = new Master();

            // Initialize Networking
            Networking networkingInstance = new Networking();
        }

        private Master() {
            if (staticInstance != null) {
                return;
            }
        }

        /// <summary>
        /// Add drone to drones array and return index of drone
        /// </summary>
        /// <param name="drone"></param>
        /// <returns></returns>
        public int AddDroneToServer(DroneThreadPair drone) {
            int droneIndex;
            lock (drones) {
                // TODO: iterate through array and find free index
                droneIndex = droneCount;
                drones[droneCount] = drone;
                droneCount++;
            }
            return droneIndex;
        }
    }

    class DroneThreadPair {
        private ConnectedDrone drone;
        private Thread droneThread;

        public ConnectedDrone Drone { get { return drone; } }
        public Thread DroneThread { get { return droneThread; } }

        public DroneThreadPair(ConnectedDrone _drone, Thread _droneThread) {
            this.drone = _drone;
            this.droneThread = _droneThread;
        }
    }
}