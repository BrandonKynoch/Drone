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
        public const string META_FILE = "Drone";
        public const string NN_FILE_EXTENSION = ".NN";
        public const string NN_META_FILE_EXTENSION = ".NNM";

        private const double EPOCH_RUN_TIME = 6;   // Time for a single epoch to execute in seconds
        private const int SUPER_EVOLUTION_CYCLE = 20; // Super evolution every n epochs
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

                        currentTrainingDir = new DirectoryInfo(continueFromDirFull);

                        GeneticNNUpdates(startNew: false);
                    } else {
                        GeneticNNUpdates(startNew: true);
                    }
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
            bool isSuperEvolution = (currentEpoch % SUPER_EVOLUTION_CYCLE) == 0 && sessionStarted;

            string targetFolderName = currentEpoch.ToString();
            if (isSuperEvolution)
                targetFolderName += " (Super)";

            previousTrainingDir = currentTrainingDir;
            currentTrainingDir = Directory.CreateDirectory(sessionTrainingDir.ToString() + "/" + targetFolderName);

            // Create NNMeta files saving current fitness score
            for (int i = 0; i < Master.GetDroneCount; i++) {
                ConnectedDrone d = Master.GetDrone(i).Drone;

                // Write drone fitness to meta file
                string droneEnclosingDir = ((!startNew) ? previousTrainingDir.ToString() : currentTrainingDir.ToString()) + "/" + d.id;
                if (!Directory.Exists(droneEnclosingDir)) {
                    Directory.CreateDirectory(droneEnclosingDir);
                }
                string nnMetaPath = droneEnclosingDir + "/" + META_FILE + NN_META_FILE_EXTENSION;
                JObject droneMetaData = new JObject(new JProperty("fitness", d.Fitness));
                File.WriteAllText(nnMetaPath, droneMetaData.ToString());
            }

            
            if (!startNew) {
                // Copy files over to target folder for new epoch
                string[] previousNNs = Directory.GetDirectories(previousTrainingDir.ToString());

                if (!isSuperEvolution) {
                    // Normal evolution
                    CopyNNFolders(previousTrainingDir.ToString(), currentTrainingDir.ToString(), overwrite: true);
                } else {
                    // Super evolution
                    string[] epochDirectories = Directory.GetDirectories(sessionTrainingDir.ToString());
                    List<Utils.FileData> epochDirs = Utils.ExtractFileData(epochDirectories);
                    epochDirs.Sort();
                    epochDirs.Reverse();
                    for (int i = 1; i < SUPER_EVOLUTION_CYCLE + 1; i++) { // Start at index 1 so that we don't copy elements from/to same dir
                        CopyNNFolders(epochDirs.ElementAt(i).fullFilePath, currentTrainingDir.ToString(), overwrite: false);
                    }
                    CullNNS(currentTrainingDir.ToString(), Master.GetDroneCount);
                    for (int i = 1; i < SUPER_EVOLUTION_CYCLE; i++) {
                        Directory.Delete(epochDirs.ElementAt(i).fullFilePath, true);
                    }
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
                    string nnPath = currentTrainingDir + "/" + drone.id;

                    JObject responseToDrone = new JObject(
                        new JProperty("id", drone.id),
                        new JProperty("opcode", Master.RESPONSE_OPCODE_LOAD_NN),
                        new JProperty("nnFolder", nnPath));

                    drone.ReceiveMessageFromSimulation(responseToDrone);

                    Thread.Yield();
                }
            }
        }

        private void CopyNNFolders(string from, string to, bool overwrite) {
            // Copy files over to target folder
            string[] previousNNs = Directory.GetDirectories(from);

            if (overwrite) {
                foreach (var dirNN in previousNNs) {
                    DirectoryInfo nnDirInfo = new DirectoryInfo(dirNN);
                    string copyTo = dirNN.Replace(from, to);
                    if (Directory.Exists(copyTo)) {
                        Directory.Delete(copyTo, true);
                    }
                    nnDirInfo.DeepCopy(copyTo);
                }
            } else {
                string[] NNSInTargetFolder = Directory.GetDirectories(to);
                int maxI = 0;
                List<Utils.FileData> existingFolders = Utils.ExtractFileData(NNSInTargetFolder);
                foreach (Utils.FileData fd in existingFolders) {
                    if (fd.fileNameInt > maxI) {
                        maxI = fd.fileNameInt;
                    }
                }

                if (maxI <= Master.GetDroneCount) {
                    maxI = Master.GetDroneCount + 1;    // Reserve the low indices so that we don't overwrite when writing back there
                }

                List<Utils.FileData> previousNNFolders = Utils.ExtractFileData(previousNNs);
                foreach (Utils.FileData fd in previousNNFolders) {
                    DirectoryInfo nnDirInfo = new DirectoryInfo(fd.fullFilePath);
                    string copyTo = to + "/" + (fd.fileNameInt + maxI + 1);
                    nnDirInfo.DeepCopy(copyTo);
                }
            }
        }

        private void CullNNS(string inDirectory, int keepCount) {
            List<Utils.FileData> nnsFDs = Utils.ExtractFileData(Directory.GetDirectories(inDirectory));

            if (nnsFDs.Count <= keepCount) {
                return; // There is nothing to cull
            }

            List<NNMetaData> nns = new List<NNMetaData>();
            for (int i = 0; i < nnsFDs.Count; i++) {
                nns.Add(new NNMetaData(new Utils.FileData(nnsFDs[i].fullFilePath + "/" + META_FILE + NN_META_FILE_EXTENSION)));
            }

            nns.Sort();

            for (int i = nns.Count - 1; i >= keepCount; i--) {
                Directory.Delete(nns[i].nnMetaFile.enclosingDir, true);
            }

            // Rename NN folders
            for (int i = keepCount - 1; i >= 0; i--) {
                Utils.FileData nnFolderFD = new Utils.FileData(nns[i].nnMetaFile.enclosingDir);
                Directory.Move(nnFolderFD.fullFilePath, nnFolderFD.enclosingDir + "/" + i);
            }
        }

        /// <summary>
        /// Applies the genetic algorithm and evolves the contents of a given directory.
        /// </summary>
        public void EvolveDirectoryContents(string targetDirectory) {
            List<Utils.FileData> nnFDs = Utils.ExtractFileData(Directory.GetDirectories(targetDirectory)); // nn folders
            List<NNGroupData> nns = new List<NNGroupData>();
            for (int i = 0; i < nnFDs.Count; i++) {
                nns.Add(new NNGroupData(nnFDs[i].fullFilePath, nnFDs[i].fullFilePath + "/" + META_FILE + NN_META_FILE_EXTENSION));
            }

            nns.Sort();

            int totalCount = nns.Count;
            double keep = 0.2; // Keep the top percentage completely unmodified
            //double discard = 0.2; // Discard the bottom percentage, their genes will not reproduce. They will be replaced with randomly chosen genes from keep percentile
            //double reproduceWithPercentile = 0.9; // When reproducing, crossover genes will be chosen from this top percentile
            double crossOverPopulation = 0.9f;
            double crossOverAmount = 0.1f; // Amount of genes to take when crossing over
            double mutationProbability = 0.6; // Likelyhood that a specimin will have any mutation
            double speciminMutationProbability = 0.2f; // When a specimin is mutating, what amount of genes should change
            double speciminMutationAmount = 0.1f; // When a specimin is mutating, by how much should a single genome change

            int keepCount = (int)Math.Ceiling(((double)totalCount) * keep);
            //int discardCount = (int)Math.Ceiling(((double)totalCount) * discard);

            // Discard bottom
            //for (int i = 0; i < discardCount; i++) {
            //    nns[totalCount - 1 - i].CopyData(nns[(int)Utils.RandomRange(0, totalCount - discardCount)]);
            //}

            // Perform crossovers
            for (int i = 0; i < totalCount * crossOverPopulation; i++) {
                int a = (int) Utils.RandomRange(0, totalCount * keep);
                int b = (int)Utils.RandomRange((keepCount + 0.1), (totalCount - 0.1));

                if (a != b) {
                    NNGroupData.CrossOver(nns[a], nns[b], crossOverAmount);
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
            foreach (NNGroupData ngd in nns) {
                Console.Write(".");
                Directory.Delete(ngd.nnFolder.fullFilePath, true);
                Directory.CreateDirectory(ngd.nnFolder.fullFilePath);
                ngd.WriteToFile();
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
    }





    // Represents a complex NN consisting of multiple NN shapes
    public class NNGroupData : IComparable<NNGroupData> {
        public Utils.FileData nnFolder;

        private Dictionary<string, NNData> nns = new Dictionary<string, NNData>(); // NN name : nnData
        public Dictionary<string, NNData>.ValueCollection NNs {
            get { return nns.Values; }
        }

        private double fitness; // Higher is better

        public NNGroupData(string _nnFolder, string metaFile) {
            this.nnFolder = new Utils.FileData(_nnFolder);

            // Fetch all NNs in nnFolder
            List<Utils.FileData> filesInNNFolder = Utils.ExtractFileData(Directory.GetFiles(_nnFolder));
            string nnFileExtensionWithoutDot = NeuralTrainer.NN_FILE_EXTENSION.Replace(".", "");
            foreach (Utils.FileData fd in filesInNNFolder) {
                if (fd.fileExtension.Equals(nnFileExtensionWithoutDot)) {
                    nns.Add(fd.fileName, new NNData(fd.fullFilePath));
                }
            }

            // Get fitness from meta file
            string meta = File.ReadAllText(metaFile);
            JObject metaJson = JObject.Parse(meta);
            fitness = metaJson.GetValue("fitness").Value<double>();
        }

        public void CopyData(NNGroupData from, bool useOriginal = true) {
            foreach (string nnKey in from.nns.Keys) {
                if (nns.ContainsKey(nnKey)) {
                    nns[nnKey].CopyData(from.nns[nnKey], useOriginal);
                }
            }
        }

        public static void CrossOver(NNGroupData a, NNGroupData b, double crossOverProbability) {
            foreach (string nnKey in a.nns.Keys) {
                if (b.nns.ContainsKey(nnKey)) {
                    NNData.CrossOver(a.nns[nnKey], b.nns[nnKey], crossOverProbability);
                }
            }
        }

        public void Mutate(double mutationProbability, double mutationAmount) {
            foreach (NNData nn in nns.Values) {
                nn.Mutate(mutationProbability, mutationAmount);
            }
        }

        public void WriteToFile() {
            foreach (NNData nn in nns.Values) {
                nn.WriteToFile();
            }
        }

        // Sort in descending order of fitness
        public int CompareTo(NNGroupData other) {
            return other.fitness.CompareTo(this.fitness);
        }
    }

    // Represents a single NN shape
    public class NNData {
        public string nnFile;

        public byte[] rawHeader;
        public double[] originalData;
        public double[] modifiedData;
        public int geneticDataIndex; // The start index of data that should be modified (i.e. after header)

        public NNData(string fromNNFile) {
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
                    to.modifiedData[i] = from.modifiedData[i];
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
    }

    // Lightweight class that only loads meta data from text file
    public class NNMetaData : IComparable<NNMetaData> {
        public Utils.FileData nnMetaFile;

        double fitness; // Higher is better

        public NNMetaData(Utils.FileData metaFile) {
            nnMetaFile = metaFile;

            // Get fitness from meta file
            string meta = File.ReadAllText(metaFile.fullFilePath);
            JObject metaJson = JObject.Parse(meta);
            fitness = metaJson.GetValue("fitness").Value<double>();
        }

        public int CompareTo(NNMetaData other) {
            return other.fitness.CompareTo(this.fitness);
        }
    }
}

