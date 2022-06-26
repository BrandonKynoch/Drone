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

        // DRONE OPCODES
        public const int OPCODE_SPAWN_DRONE = 0x1;
        public const int OPCODE_REQUEST_TARGET_NN_FROM_SERVER = 0x3;
        public const int CODE_RESET_ALL_DRONES = 0x6;

        // SIMULATION RESPONSE OPCODES
        public const int RESPONSE_OPCODE_SENSOR_DATA = 0x4;
        public const int RESPONSE_OPCODE_LOAD_NN = 0x5;
        public const int REPONSE_OPCODE_RESET_DRONES = 0x7;
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// PROPERTIES /////////////////////////////////////////////////////////
        public static int GetDroneCount { get { return staticInstance.droneCount; } }
        /// PROPERTIES /////////////////////////////////////////////////////////

        private DroneThreadPair[] drones = new DroneThreadPair[MAXIMUM_DRONE_CONNECTIONS];
        private int droneCount = 0;

        static void Main(string[] args) {
            // Initialize instance
            staticInstance = new Master();

            // Initialize Networking
            Networking networkingInstance = new Networking();
            NeuralTrainer neuralTrainerInstance = new NeuralTrainer();
        }

        private Master() {
            if (staticInstance != null) {
                return;
            }
            staticInstance = this;
        }

        /// <summary>
        /// Add drone to drones array and return index of drone
        /// </summary>
        /// <param name="drone"></param>
        /// <returns></returns>
        public int AddDroneToServer(DroneThreadPair drone) {
            int droneIndex = -1;
            lock (drones) {
                // Iterate through array and find free index
                for (int i = 0; i < drones.Length; i++) {
                    if (drones[i] == null) {
                        droneIndex = i;
                        break;
                    }
                }
                if (droneIndex == -1) {
                    throw new Exception("Could not add drone to server. No free index available");
                }

                drones[droneIndex] = drone;
                droneCount++;
            }
            return droneIndex;
        }

        public static DroneThreadPair GetDrone(int droneID) {
            return staticInstance.drones[droneID];
        }
    }

    public class DroneThreadPair {
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