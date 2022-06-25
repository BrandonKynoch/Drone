using System;
using System.IO;

namespace SimServer {
    public class NeuralTrainer {
        /// Singleton pattern //////////////////////////////////////////////////
        private static NeuralTrainer staticInstance;
        public static NeuralTrainer StaticInstance {
            get { return staticInstance; }
        }
        /// Singleton pattern //////////////////////////////////////////////////

        /// CONSTANTS //////////////////////////////////////////////////////////
        private const double EPOCH_RUN_TIME = 15;   // Time for a single epoch to execute in seconds
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// TRAINING DATA //////////////////////////////////////////////////////
        private Thread neuralTrainingThread;    // Thread dedicated to training the drones

        private DirectoryInfo rootTrainingDir;
        private DirectoryInfo currentTrainingDir;
        private int currentEpoch = 0;

        public bool continueSimulation = false;
        public Queue<ConnectedDrone> dronesWaitingToReceiveNN = new Queue<ConnectedDrone>();
        /// TRAINING DATA //////////////////////////////////////////////////////

        /// PROPERTIES /////////////////////////////////////////////////////////
        public static bool ContinueSimulation { get { return staticInstance.continueSimulation; } }
        /// PROPERTIES /////////////////////////////////////////////////////////

        public NeuralTrainer() {
            if (staticInstance != null) {
                return;
            }
            staticInstance = this;

            InitTrainer();

            neuralTrainingThread = new Thread(new ThreadStart(TrainingLoop));
            neuralTrainingThread.Priority = ThreadPriority.AboveNormal;
            neuralTrainingThread.Start();
        }

        public void InitTrainer() {
            rootTrainingDir = Directory.CreateDirectory("..\\Training Data");

            string currentTrainingDirName = DateTime.Now.ToShortDateString() + "::" + DateTime.UtcNow.ToShortTimeString();
            currentTrainingDir = Directory.CreateDirectory("..\\Training Data\\" + currentTrainingDirName);

            Thread.Sleep(TimeSpan.FromSeconds(1));

            Console.WriteLine("Enter the name of the folder to continue training from\n\tOr n to create new\n");
            bool inputValid = false;
            while (!inputValid) {
                String targetDir = Console.ReadLine();
                if (targetDir.ToLower().Equals("n")) {
                    // Send message to drones to create new NN's
                    inputValid = true;


                    // Do stuff


                } else {
                    // Read NN's from folder and send to drones
                    inputValid = true;


                    // Copy NN's at max epoch from targetDir to currentTrainingDir

                    // Do stuff


                    continueSimulation = true;

                    if (targetDir.Equals("")) {
                        inputValid = false;
                    }
                }

                if (!inputValid) {
                    Console.WriteLine("Invalid directory, try again:\n");
                }
            }
        }

        public void tellDroneSetNeuralNetworks(string targetFolder, bool createNew = false) {
            // itterate through all dronesWaitingToReceiveNN

            if (createNew) {

            } else {

            }

            // Respond to drone after getting/creating file
        }

        public void TrainingLoop() {
            while (!continueSimulation) {
                Thread.Yield();
            }

            while (true) {
                Thread.Sleep(TimeSpan.FromSeconds(EPOCH_RUN_TIME));

                currentEpoch++;
                Console.WriteLine("Starting epoch: " + currentEpoch);

                Thread.Yield();
            }
        }
    }
}

