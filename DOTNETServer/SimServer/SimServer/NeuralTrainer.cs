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
        private const string NN_META_FILE_EXTENSION = ".NNM";

        private const double EPOCH_RUN_TIME = 15;   // Time for a single epoch to execute in seconds
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// TRAINING DATA //////////////////////////////////////////////////////
        private Thread neuralTrainingThread;    // Thread dedicated to training the drones

        private DirectoryInfo rootTrainingDir;
        private DirectoryInfo currentTrainingDir;
        private int currentEpoch = 0;

        private Queue<ConnectedDrone> dronesWaitingToReceiveNN = new Queue<ConnectedDrone>();

        public bool continueSimulation = false;     // Set to true when the simulation is actually simulating & drones flying
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
                string[] previousNNs = Directory.GetFiles(continueFrom);

                foreach (var fileNN in previousNNs) {
                    string file = fileNN.ToString();
                    string copyTo = file.Replace(continueFrom, targetFolder);
                    File.Copy(file, copyTo);
                }


                // Execute genetic algorithm here
                EvolveDirectoryContents(targetFolder);

                // MARK: TODO: Check that number of agents matches number of files
            }

            // Respond to drones with target files
            lock (dronesWaitingToReceiveNN) {
                while (dronesWaitingToReceiveNN.Count > 0) {
                    ConnectedDrone drone = dronesWaitingToReceiveNN.Dequeue();
                    string nnPath = targetFolder + "/" + drone.id + NN_FILE_EXTENSION;

                    JObject responseToDrone = new JObject(
                        new JProperty("id", drone.id),
                        new JProperty("opcode", Master.RESPONSE_OPCODE_LOAD_NN),
                        new JProperty("file", nnPath));

                    // Write drone fitness to meta file
                    if (!continueFrom.Equals("")) {
                        string nnMetaPath = continueFrom + "/" + drone.id + NN_META_FILE_EXTENSION;
                        JObject droneMetaData = new JObject(new JProperty("fitness", drone.Fitness));
                        File.WriteAllTextAsync(nnMetaPath, droneMetaData.ToString());
                    }

                    drone.ReceiveMessageFromSimulation(responseToDrone);

                    Thread.Yield();
                }
            }

            // TODO:
            //send message to server telling all drones to reset
        }

        /// <summary>
        /// Applies the genetic algorithm and evolves the contents of a given directory.
        /// </summary>
        public void EvolveDirectoryContents(string targetDirectory) {
            // TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            //  TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TOD
            // O TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TO
            // DO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO T
            // ODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 
            // TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            //  TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TOD
            // O TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TO
            // DO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO T
            // ODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            // TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            //  TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TOD
            // O TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TO
            // DO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO T
            // ODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            // TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
            //  TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TOD
            // O TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TO
            // DO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO T
            // ODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 
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

                continueSimulation = false;

                while (dronesWaitingToReceiveNN.Count < Master.GetDroneCount) {
                    Thread.Yield();
                }

                currentEpoch++;
                Console.WriteLine("Starting epoch: " + currentEpoch);

                GeneticNNUpdates(currentTrainingDir.ToString() + "/" + (currentEpoch-1), currentTrainingDir.ToString());

                Thread.Yield();
            }
        }

        /// <summary>
        /// Add drone to queue of drones waiting to receive a response from the server with the file that should be loaded from training
        /// </summary>
        /// <param name="d"></param>
        public static void AddDroneToNNWaitingQueue(ConnectedDrone d) {
            lock (staticInstance.dronesWaitingToReceiveNN) {
                staticInstance.dronesWaitingToReceiveNN.Enqueue(d);
            }
        }
    }
}

