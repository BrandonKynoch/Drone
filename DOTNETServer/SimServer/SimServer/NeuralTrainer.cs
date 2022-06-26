﻿using System;
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

            // Create NNMeta files saving current fitness score
            for (int i = 0; i < Master.GetDroneCount; i++) {
                ConnectedDrone d = Master.GetDrone(i).Drone;

                // Write drone fitness to meta file
                string nnMetaPath = targetFolder + "/" + d.id + NN_META_FILE_EXTENSION;
                JObject droneMetaData = new JObject(new JProperty("fitness", d.Fitness));
                File.WriteAllTextAsync(nnMetaPath, droneMetaData.ToString());
            }

            bool createNew = continueFrom.Equals("");
            if (!createNew) {
                // Copy files over to target folder for new epoch
                string[] previousNNs = Directory.GetFiles(continueFrom);

                foreach (var fileNN in previousNNs) {
                    string file = fileNN.ToString();
                    string copyTo = file.Replace(continueFrom, targetFolder);
                    if (File.Exists(copyTo)) {
                        File.Delete(copyTo);
                    }
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

                    drone.ReceiveMessageFromSimulation(responseToDrone);

                    Thread.Yield();
                }
            }

            // TODO:
            //send message to server telling all drones to reset

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
        /// Applies the genetic algorithm and evolves the contents of a given directory.
        /// </summary>
        public void EvolveDirectoryContents(string targetDirectory) {
            string[] fileNames = Directory.GetFiles(targetDirectory);
            List<NNData> nns = new List<NNData>();
            for (int i = 0; i < fileNames.Length; i++) {
                if (fileNames[i].Contains(NN_FILE_EXTENSION) && !fileNames[i].Contains(NN_META_FILE_EXTENSION)) {
                    nns.Add(new NNData(fileNames[i], fileNames[i].Replace(NN_FILE_EXTENSION, NN_META_FILE_EXTENSION)));
                }
            }

            nns.Sort();

            int totalCount = nns.Count;
            double keep = 0.1; // Keep the top percentage completely unmodified
            double discard = 0.2; // Discard the bottom percentage, their genes will not reproduce. They will be replaced with randomly chosen genes from keep percentile
            double reproduceWithPercentile = 0.9; // When reproducing, crossover genes will be chosen from this top percentile
            double crossOverPercentile = 0.7; 
            double mutationProbability = 0.2; // Likelyhood that a specimin will have any mutation
            double mutationAmount = 0.2f; // When a specimin is mutating, what amount of genes should change

            int keepCount = (int)Math.Ceiling(((double)totalCount) * keep);
            int discardCount = (int)Math.Ceiling(((double)totalCount) * discard);

            // Discard bottom
            for (int i = 0; i < discardCount; i++) {
                nns[totalCount - 1 - i].CopyData(nns[(int)Utils.RandomRange(0, keepCount)]);
            }

            // Perform crossovers
            for (int i = keepCount; i < totalCount; i++) {
                int a = (int) Utils.RandomRange((keepCount + 0.1), (totalCount - 0.1));
                int b = (int)Utils.RandomRange((keepCount + 0.1), (totalCount - 0.1));

                //Console.WriteLine("Cross over: " + a + ":" + b);

                if (a != b) {
                    NNData.CrossOver(nns[a], nns[b], crossOverPercentile);
                }
            }

            // Mutation
            for (int i = keepCount; i < totalCount; i++) {
                if (Utils.Random01Double() < mutationProbability) {
                    nns[i].Mutate(mutationAmount);
                }
            }

            // Write changes to NN file
            foreach (NNData nd in nns) {
                File.Delete(nd.nnFile);
                File.WriteAllBytes(nd.nnFile, nd.modifiedData);
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

                continueSimulation = false;

                while (dronesWaitingToReceiveNN.Count < Master.GetDroneCount) {
                    Thread.Yield();
                }

                currentEpoch++;
                Console.WriteLine("Starting epoch: " + currentEpoch);

                GeneticNNUpdates(currentTrainingDir.ToString() + "/" + (currentEpoch-1), currentTrainingDir.ToString());

                JObject resetRequestJSON = new JObject(new JProperty("opcode", Master.CODE_RESET_ALL_DRONES));

                // TODO: Make sim stream private again
                Networking.SendStringToNetworkStream(Networking.StaticInstance.simStream, resetRequestJSON.ToString());

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





        public class NNData: IComparable<NNData> {
            public string nnFile;

            public byte[] originalData;
            public byte[] modifiedData;
            public int geneticDataIndex; // The start index of data that should be modified (i.e. after header)

            double fitness; // Higher is better

            public NNData(string fromNNFile, string metaFile) {
                nnFile = fromNNFile;

                originalData = File.ReadAllBytes(fromNNFile);

                byte[] neuralSizeRawBytes = new byte[4];
                for (int i = 0; i < 4; i++) {
                    neuralSizeRawBytes[i] = originalData[i];
                }

                Int32 neuralSize = BitConverter.ToInt32(neuralSizeRawBytes, 0);

                geneticDataIndex = 4 * // size of int
                    (1 + // neural size
                    neuralSize + // neural shape
                    (neuralSize - 1)); // activations

                Console.WriteLine("Genetic index: " + geneticDataIndex + " --- " + originalData.Length);

                modifiedData = new byte[originalData.Length];
                for (int i = 0; i < originalData.Length; i++) {
                    modifiedData[i] = originalData[i];
                }


                // Get fitness from meta file
                string meta = File.ReadAllText(metaFile);
                JObject metaJson = JObject.Parse(meta);
                fitness = metaJson.GetValue("fitness").Value<double>();
            }

            public void CopyData(NNData from, bool useOriginal = true) {
                if (useOriginal) {
                    for (int i = geneticDataIndex; i < modifiedData.Length; i++) {
                        modifiedData[i] = from.originalData[i];
                    }
                } else {
                    for (int i = geneticDataIndex; i < modifiedData.Length; i++) {
                        modifiedData[i] = from.modifiedData[i];
                    }
                }
            }

            public static void CrossOver(NNData left, NNData right, double crossOverProbability) {
                bool useLeft = false;
                for (int i = left.geneticDataIndex; i < left.modifiedData.Length; i++) {
                    if (Utils.Random01Double() < crossOverProbability) {
                        if (useLeft) {
                            right.modifiedData[i] = left.modifiedData[i];
                        } else {
                            left.modifiedData[i] = right.modifiedData[i];
                        }
                    }
                }
            }

            public void Mutate(double mutationAmount) {
                for (int i = geneticDataIndex; i < modifiedData.Length - 1; i++) {
                    if (Utils.Random01Double() < mutationAmount) {
                        modifiedData[i] = (byte) ~modifiedData[i];
                    }
                }
            }

            // Sort in descending order of fitness
            public int CompareTo(NNData other) {
                return other.fitness.CompareTo(this.fitness);
            }
        }
    }
}

