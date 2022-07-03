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

        private const double EPOCH_RUN_TIME = 7;   // Time for a single epoch to execute in seconds
        private const int SUPER_EVOLUTION_CYCLE = 10; // Super evolution every n epochs
        /// CONSTANTS //////////////////////////////////////////////////////////

        /// TRAINING DATA //////////////////////////////////////////////////////
        private Thread neuralTrainingThread;    // Thread dedicated to training the drones

        private DirectoryInfo rootTrainingDir;
        private DirectoryInfo sessionTrainingDir;
        private DirectoryInfo currentTrainingDir;
        private DirectoryInfo previousTrainingDir;
        private int currentEpoch = 0;
        private int firstIteration = 1;
        public int staleDroneCount = 0;

        private Queue<ConnectedDrone> dronesWaitingToReceiveNN = new Queue<ConnectedDrone>();

        public bool continueSimulation = false;     // Set to true when the simulation is actually simulating & drones flying (This is false during genetic NN updates)
        public bool sessionStarted = false;         // Set to true once session path has been specified and simulation has started, remains true for duration of program
        /// TRAINING DATA //////////////////////////////////////////////////////

        /// PROPERTIES /////////////////////////////////////////////////////////
        public static bool ContinueSimulation { get { return staticInstance.continueSimulation; } }
        public static bool SessionStarted { get { return staticInstance.sessionStarted; } }
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

            string sessionTrainingDirName = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();
            sessionTrainingDirName = sessionTrainingDirName.Replace("/", "-");
            sessionTrainingDirName = sessionTrainingDirName.Replace(" PM", "");
            sessionTrainingDirName = sessionTrainingDirName.Replace(" AM", "");
            sessionTrainingDirName = sessionTrainingDirName.Replace(":", "");
            sessionTrainingDirName = sessionTrainingDirName.Replace("-2022", "");

            sessionTrainingDir = Directory.CreateDirectory(ROOT_TRAINING_DIR + "/" + sessionTrainingDirName);
            Console.WriteLine("Current training dir:\n\t" + sessionTrainingDir.ToString() + "\n\n");

            // Sleep for a little bit so that other threads can finish printing
            Thread.Sleep(TimeSpan.FromSeconds(0.1));

            Console.WriteLine("Enter the name of the folder to continue training from or n to create new\n");
            bool inputValid = false;
            bool directoryValid = false;
            while (!inputValid) {
                String continueFromDirName = Console.ReadLine();
                String continueFromDirFull = rootTrainingDir.ToString() + "/" + continueFromDirName;

                // Handle user input
                if (!continueFromDirName.Equals("")) {
                    if (continueFromDirName.ToLower().Equals("n")) {
                        inputValid = true;
                    }

                    if (Directory.Exists(continueFromDirFull)) {
                        inputValid = true;
                        directoryValid = true;
                    }
                }


                if (!inputValid) {
                    Console.WriteLine("Invalid directory, try again:\n");
                } else {
                    if (directoryValid) {
                        // Read the last epoch that simulation stopped at from directory name
                        string[] dirSplit = continueFromDirFull.Split(new char[] { '/' });
                        string intName = string.Concat(dirSplit[dirSplit.Length - 1].Where(char.IsNumber));
                        int continuingEpoch = int.Parse(intName);
                        currentEpoch = continuingEpoch;

                        Console.WriteLine("CONTINUING FROM:\n\t" + continueFromDirFull + "\n\n");

                        previousTrainingDir = new DirectoryInfo(continueFromDirFull);
                    }

                    GeneticNNUpdates(startNew: true);
                }
            }
        }

        /// <summary>
        /// Start a new epoch and create new NN files
        /// Respond to drones by sending them the file address of their target NN
        /// </summary>
        /// <param name="continueFrom"></param>
        /// <param name="targetFolder"></param>
        public void GeneticNNUpdates(bool startNew = false) {
            bool isSuperEvolution = (currentEpoch % SUPER_EVOLUTION_CYCLE) == 0;

            string targetFolderName = currentEpoch.ToString();
            if (isSuperEvolution)
                targetFolderName += " (Super)";

            previousTrainingDir = currentTrainingDir;
            currentTrainingDir = Directory.CreateDirectory(sessionTrainingDir.ToString() + "/" + targetFolderName);

            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // Super evolution code 
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:
            // TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:TODO:

            // Create NNMeta files saving current fitness score
            for (int i = 0; i < Master.GetDroneCount; i++) {
                ConnectedDrone d = Master.GetDrone(i).Drone;

                // Write drone fitness to meta file
                string nnMetaPath = ((!startNew) ? previousTrainingDir.ToString() : currentTrainingDir.ToString()) + "/" + d.id + NN_META_FILE_EXTENSION;
                JObject droneMetaData = new JObject(new JProperty("fitness", d.Fitness));
                File.WriteAllText(nnMetaPath, droneMetaData.ToString());
            }

            if (!startNew) {
                // Copy files over to target folder for new epoch
                string[] previousNNs = Directory.GetFiles(previousTrainingDir.ToString());

                foreach (var fileNN in previousNNs) {
                    string file = fileNN.ToString();
                    string copyTo = file.Replace(previousTrainingDir.ToString(), currentTrainingDir.ToString());
                    if (File.Exists(copyTo)) {
                        File.Delete(copyTo);
                    }
                    File.Copy(file, copyTo);
                }


                // Execute genetic algorithm here
                if (firstIteration < 0 && !Networking.StaticInstance.isResettingNetwork) {
                    EvolveDirectoryContents(currentTrainingDir.ToString());
                } else {
                    firstIteration--;
                    Console.WriteLine("\n\nSkipping evolution on current epoch...\n\n");
                }

                // MARK: TODO: Check that number of agents matches number of files
            }

            // Respond to drones with target files
            lock (dronesWaitingToReceiveNN) {
                continueSimulation = true;
                Networking.StaticInstance.isResettingNetwork = false;
                Networking.lastSimPacketReceivedTimeTicks = DateTime.Now.Ticks;
                while (dronesWaitingToReceiveNN.Count > 0) {
                    ConnectedDrone drone = dronesWaitingToReceiveNN.Dequeue();
                    string nnPath = currentTrainingDir + "/" + drone.id + NN_FILE_EXTENSION;

                    JObject responseToDrone = new JObject(
                        new JProperty("id", drone.id),
                        new JProperty("opcode", Master.RESPONSE_OPCODE_LOAD_NN),
                        new JProperty("file", nnPath));

                    drone.ReceiveMessageFromSimulation(responseToDrone);

                    Thread.Yield();
                }
            }
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
            //double reproduceWithPercentile = 0.9; // When reproducing, crossover genes will be chosen from this top percentile
            double crossOverPopulation = 1.3f;
            double crossOverPercentile = 0.3; 
            double mutationProbability = 0.7; // Likelyhood that a specimin will have any mutation
            double speciminMutationProbability = 0.3f; // When a specimin is mutating, what amount of genes should change
            double speciminMutationAmount = 1f; // When a specimin is mutating, by how much should a single genome change

            int keepCount = (int)Math.Ceiling(((double)totalCount) * keep);
            int discardCount = (int)Math.Ceiling(((double)totalCount) * discard);

            // Discard bottom
            for (int i = 0; i < discardCount; i++) {
                nns[totalCount - 1 - i].CopyData(nns[(int)Utils.RandomRange(0, keepCount)]);
            }

            // Perform crossovers
            for (int i = 0; i < totalCount * crossOverPopulation; i++) {
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
                    nns[i].Mutate(speciminMutationProbability, speciminMutationAmount);
                }
            }

            // Write changes to NN file
            Console.Write("Writing NN files: ");
            foreach (NNData nd in nns) {
                Console.Write(".");
                File.Delete(nd.nnFile);
                nd.WriteToFile();
            }
            Console.WriteLine(" Finished");
        }


        /// <summary>
        /// Simulation training loop handles when the simulation start a new epoch and handles creating
        /// and updating NN's
        /// </summary>
        public void TrainingLoop() {
            while (!continueSimulation) {
                Thread.Yield();
            }

            sessionStarted = true;

            while (true) {
                int progressUILength = 40;
                for (int i = 0; i < progressUILength; i++) {
                    Console.Write("_");
                }
                Console.WriteLine("");
                ConsoleColor originalConsoleCol = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                for (int i = 0; i < progressUILength; i++) {
                    if (Networking.StaticInstance.isResettingNetwork) {
                        break;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(EPOCH_RUN_TIME / progressUILength));
                    Console.Write("█");
                }
                Console.ForegroundColor = originalConsoleCol;
                Console.WriteLine("\n\n");

                continueSimulation = false; // After setting this to false Network handler adds all incoming request drones to dronesWaitingToReceiveNN

                while (dronesWaitingToReceiveNN.Count < Master.GetDroneCount - staleDroneCount) {
                    Thread.Yield();
                }

                currentEpoch++;
                Console.WriteLine("Starting epoch: " + currentEpoch);

                // Decrease stale drone count once simulation is stable again
                if (!Networking.StaticInstance.isResettingNetwork) {
                    if (currentEpoch % 10 == 0 && staleDroneCount > 0) {
                        staleDroneCount--;
                    }
                }

                GeneticNNUpdates();

                // Tell the simulation to reset all drones
                JObject resetRequestJSON = new JObject(new JProperty("opcode", Master.CODE_RESET_ALL_DRONES));

                Networking.SendStringToNetworkStream(Networking.SimStream, resetRequestJSON.ToString());

                Thread.Yield();
            }
        }

        /// <summary>
        /// Add drone to queue of drones waiting to receive a response from the server with the file that should be loaded from training
        /// </summary>
        /// <param name="d"></param>
        public static void AddDroneToNNWaitingQueue(ConnectedDrone d) {
            lock (staticInstance.dronesWaitingToReceiveNN) {
                if (!staticInstance.dronesWaitingToReceiveNN.Contains(d)) {
                    staticInstance.dronesWaitingToReceiveNN.Enqueue(d);
                }
            }
        }





        public class NNData: IComparable<NNData> {
            public string nnFile;

            public byte[] rawHeader;
            public double[] originalData;
            public double[] modifiedData;
            public int geneticDataIndex; // The start index of data that should be modified (i.e. after header)

            double fitness; // Higher is better

            public NNData(string fromNNFile, string metaFile) {
                nnFile = fromNNFile;

                byte[] rawData = File.ReadAllBytes(fromNNFile);
                Int32 neuralSize = BitConverter.ToInt32(rawData, 0);

                geneticDataIndex = 4 * // size of int
                    (1 + // neural size
                    neuralSize + // neural shape
                    (neuralSize - 1)); // activations

                rawHeader = new byte[geneticDataIndex];
                for (int i = 0; i < geneticDataIndex; i++) {
                    rawHeader[i] = rawData[i];
                }

                int doubleCount = (rawData.Length - geneticDataIndex) / 8; // number of doubles in weights and biases combined
                originalData = new double[doubleCount];
                modifiedData = new double[doubleCount];

                for (int i = 0; i < doubleCount; i++) {
                    originalData[i] = BitConverter.ToDouble(rawData, geneticDataIndex + (i * 8)); // 8bytes
                    modifiedData[i] = originalData[i];
                }

                // Get fitness from meta file
                string meta = File.ReadAllText(metaFile);
                JObject metaJson = JObject.Parse(meta);
                fitness = metaJson.GetValue("fitness").Value<double>();
            }

            public void WriteToFile() {
                if (File.Exists(nnFile)) {
                    File.Delete(nnFile);
                }
                using (FileStream fs = new FileStream(nnFile, FileMode.CreateNew, FileAccess.Write)) {
                    fs.Write(rawHeader, 0, rawHeader.Length);
                    byte[] dataBytes = new byte[originalData.Length * 8];
                    Buffer.BlockCopy(modifiedData, 0, dataBytes, 0, dataBytes.Length);
                    fs.Write(dataBytes, 0, dataBytes.Length);
                    fs.Close();
                }
            }

            public void CopyData(NNData from, bool useOriginal = true) {
                if (useOriginal) {
                    for (int i = 0; i < modifiedData.Length; i++) {
                        modifiedData[i] = from.originalData[i];
                    }
                } else {
                    for (int i = 0; i < modifiedData.Length; i++) {
                        modifiedData[i] = from.modifiedData[i];
                    }
                }
            }

            public static void CrossOver(NNData from, NNData to, double crossOverProbability) {
                for (int i = 0; i < from.originalData.Length; i++) {
                    if (Utils.Random01Double() < crossOverProbability) {
                        to.modifiedData[i] = from.originalData[i];
                    }
                }
            }

            public void Mutate(double mutationProbability, double mutationAmount) {
                for (int i = 0; i < modifiedData.Length; i++) {
                    if (Utils.Random01Double() < mutationProbability) {
                        modifiedData[i] += Utils.RandomRange(-mutationAmount, mutationAmount);
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

