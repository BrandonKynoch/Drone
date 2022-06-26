using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimServer {
    public class NeuralTrainer {
        /// Singleton pattern //////////////////////////////////////////////////
        private static NeuralTrainer staticInstance;
        public static NeuralTrainer StaticInstance {
            get { return staticInstance; }
        }
        /// Singleton pattern //////////////////////////////////////////////////

        /// CONSTANTS //////////////////////////////////////////////////////////
        private const string ROOT_TRAINING_DIR = "/Users/brandonkynoch/Desktop/Projects/Drone/Training Data";
        private const string NN_FILE_EXTENSION = ".NN";

        private const double EPOCH_RUN_TIME = 15;   // Time for a single epoch to execute in seconds
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// TRAINING DATA //////////////////////////////////////////////////////
        private Thread neuralTrainingThread;    // Thread dedicated to training the drones

        private DirectoryInfo rootTrainingDir;
        private DirectoryInfo currentTrainingDir;
        private int currentEpoch = 0;

        public bool continueSimulation = false;     // Set to true when the simulation is actually simulating & drones flying
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

        /// <summary>
        /// Initialize NN training
        /// </summary>
        public void InitTrainer() {
            rootTrainingDir = Directory.CreateDirectory(ROOT_TRAINING_DIR);

            string currentTrainingDirName = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();
            currentTrainingDirName = currentTrainingDirName.Replace("/", "-");
            currentTrainingDirName = currentTrainingDirName.Replace(" PM", "");
            currentTrainingDirName = currentTrainingDirName.Replace(" AM", "");
            currentTrainingDirName = currentTrainingDirName.Replace(":", "");
            currentTrainingDirName = currentTrainingDirName.Replace("-2022", "");

            currentTrainingDir = Directory.CreateDirectory(ROOT_TRAINING_DIR + "/" + currentTrainingDirName);
            Console.WriteLine("Current training dir:\n\t" + currentTrainingDir.ToString() + "\n\n");

            // Sleep for a little bit so that other threads can finish printing
            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            Console.WriteLine("Enter the name of the folder to continue training from or n to create new\n");
            bool inputValid = false;
            while (!inputValid) {
                String continueFromDirName = Console.ReadLine();
                String continueFromDirFull = rootTrainingDir.ToString() + "/" + continueFromDirName;

                if (continueFromDirName.ToLower().Equals("n")) {
                    inputValid = true;

                    // Create new NN's
                    GeneticNNUpdates("", currentTrainingDir.ToString());

                    break;
                } else {
                    if (continueFromDirName.Equals("")) {
                        inputValid = false;
                    } else if (Directory.Exists(continueFromDirFull)) {
                        inputValid = true;
                    }
                }

                if (!inputValid) {
                    Console.WriteLine("Invalid directory, try again:\n");
                } else {
                    Console.WriteLine("CONTINUING FROM:\n\t" + continueFromDirFull + "\n\n");

                    // Continue simulation from previous folder
                    GeneticNNUpdates(continueFromDirFull, currentTrainingDir.ToString());
                }
            }
        }

        /// <summary>
        /// Start a new epoch and create new NN files
        /// Respond to drones by sending them the file address of their target NN
        /// </summary>
        /// <param name="continueFrom"></param>
        /// <param name="targetFolder"></param>
        public void GeneticNNUpdates(string continueFrom, string targetFolder) {
            targetFolder = targetFolder + "/" + currentEpoch;
            Directory.CreateDirectory(targetFolder);

            continueSimulation = true;

            bool createNew = continueFrom.Equals("");
            if (!createNew) {
                // Copy files over to target folder for new epoch

                // Execute genetic algorithm here

                // Send files to drone

            }

            // Respond to drones with target files
            lock (dronesWaitingToReceiveNN) {
                while (dronesWaitingToReceiveNN.Count > 0) {
                    ConnectedDrone drone = dronesWaitingToReceiveNN.Dequeue();
                    string nnDir = targetFolder + "/" + drone.id + NN_FILE_EXTENSION;

                    JObject responseToDrone = new JObject(
                        new JProperty("id", drone.id),
                        new JProperty("file", nnDir));

                    drone.ReceiveMessageFromSimulation(responseToDrone);

                    Thread.Yield();
                }
            }
        }


        /// <summary>
        /// Simulation training loop handles when the simulation start a new epoch and handles creating
        /// and updating NN's
        /// </summary>
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

