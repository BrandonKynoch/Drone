//using System
using System.Text;
using SimServer;
using Newtonsoft.Json.Linq;

namespace CSVConverter {
    class CSVConverter {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Invalid args");
                return;
            }

            string loadNNsFrom = args[0];
            Utils.FileData loadNNsFromFD = new Utils.FileData(loadNNsFrom);
            string saveCSVsTo = loadNNsFromFD.enclosingDir + "/" + loadNNsFromFD.fileName + " CSV";

            // load NN's from folder
            string[] nnFolders = Directory.GetDirectories(loadNNsFrom);
            List<Utils.FileData> nnFoldersFD = Utils.ExtractFileData(nnFolders);

            List<NNGroupDataExact> nngs = new List<NNGroupDataExact>();
            for (int i = 0; i < nnFoldersFD.Count; i++) {
                nngs.Add(new NNGroupDataExact(nnFoldersFD[i].fullFilePath, nnFoldersFD[i].fullFilePath + "/" + NeuralTrainer.META_FILE + NeuralTrainer.NN_META_FILE_EXTENSION));
            }

            // Create output folders
            foreach (NNGroupDataExact nng in nngs) {
                string outputFolder = saveCSVsTo + "/" + nng.nnFolder.fileName;
                Directory.CreateDirectory(outputFolder);
                nng.WriteCSVs(outputFolder);
            }
        }
    }



    // Exact representation of NN data -> all values are fully unwrapped from NN file and stored in this class
    public class NNGroupDataExact : IComparable<NNGroupDataExact> {
        public Utils.FileData nnFolder;

        private Dictionary<string, NNDataExact> nns = new Dictionary<string, NNDataExact>(); // NN name : nnData
        public Dictionary<string, NNDataExact>.ValueCollection NNs {
            get { return nns.Values; }
        }

        private double fitness; // Higher is better

        public NNGroupDataExact(string _nnFolder, string metaFile) {
            this.nnFolder = new Utils.FileData(_nnFolder);

            // Fetch all NNs in nnFolder
            List<Utils.FileData> filesInNNFolder = Utils.ExtractFileData(Directory.GetFiles(_nnFolder));
            string nnFileExtensionWithoutDot = NeuralTrainer.NN_FILE_EXTENSION.Replace(".", "");
            foreach (Utils.FileData fd in filesInNNFolder) {
                if (fd.fileExtension.Equals(nnFileExtensionWithoutDot)) {
                    nns.Add(fd.fileName, new NNDataExact(fd.fullFilePath));
                }
            }

            // Get fitness from meta file
            string meta = File.ReadAllText(metaFile);
            JObject metaJson = JObject.Parse(meta);
            fitness = metaJson.GetValue("fitness").Value<double>();
        }

        public void WriteCSVs(string outputFolder) {
            foreach (NNDataExact nn in NNs) {
                nn.WriteCSV(outputFolder);
            }
        }

        // Sort in descending order of fitness
        public int CompareTo(NNGroupDataExact other) {
            return other.fitness.CompareTo(this.fitness);
        }
    }

    // Represents a single NN shape
    // Exact representation of NN data -> all values are fully unwrapped from NN file and stored in this class
    public class NNDataExact {
        public string nnFile;

        public Int32 neuralSize;
        public Int32[] neuralShape;
        public Int32[] activations;
        public double[][] weights; // first dimension is layer index - second dimension is flattened weights at that index
        public double[][] biases; // first dimension is layer index - second dimension is biases at that index

        public NNDataExact(string fromNNFile) {
            nnFile = fromNNFile;

            byte[] rawData = File.ReadAllBytes(fromNNFile);
            int fileIndex = 0;

            // Get neural size
            this.neuralSize = BitConverter.ToInt32(rawData, 0);
            fileIndex += 4;

            // Get neural shape
            this.neuralShape = new Int32[neuralSize];
            for (int i = 0; i < neuralSize; i++) {
                neuralShape[i] = BitConverter.ToInt32(rawData, fileIndex);
                fileIndex += 4;
            }

            // Get activations
            this.activations = new Int32[neuralSize - 1];
            for (int i = 0; i < activations.Length; i++) {
                activations[i] = BitConverter.ToInt32(rawData, fileIndex);
                fileIndex += 4;
            }

            // Load weights and biases
            this.weights = new double[neuralSize - 1][];
            this.biases = new double[neuralSize - 1][];
            for (int i = 0; i < neuralSize - 1; i++) {
                int rows = neuralShape[i + 1];
                int cols = neuralShape[i];

                // Load weights
                weights[i] = new double[rows * cols];
                for (int j = 0; j < rows * cols; j++) {
                    weights[i][j] = BitConverter.ToDouble(rawData, fileIndex);
                    fileIndex += 8;
                }

                // Load biases
                biases[i] = new double[rows];
                for (int j = 0; j < rows; j++) {
                    biases[i][j] = BitConverter.ToDouble(rawData, fileIndex);
                    fileIndex += 8;
                }
            }
        }

        public void WriteCSV(string outputFolder) {
            string saveNNPath = outputFolder + "/" + new Utils.FileData(nnFile).fileName + ".csv";

            if (File.Exists(saveNNPath)) {
                File.Delete(saveNNPath);
            }
            using (FileStream fs = new FileStream(saveNNPath, FileMode.CreateNew, FileAccess.Write)) {
                // Find row count of output csv
                int maxIndex = 0;
                if (neuralShape.Length > maxIndex) maxIndex = neuralShape.Length;
                if (activations.Length > maxIndex) maxIndex = activations.Length;
                foreach (double[] w in weights) {
                    if (w.Length > maxIndex) maxIndex = w.Length;
                }
                foreach (double[] b in biases) {
                    if (b.Length > maxIndex) maxIndex = b.Length;
                }

                int weightsCount = weights.Length;

                // Write field names
                fs.Write(Encoding.UTF8.GetBytes("neural_shape,activations"));
                for (int i = 0; i < weightsCount; i++) {
                    fs.Write(Encoding.UTF8.GetBytes(",weights_" + i + ",bias_" + i));
                }
                fs.Write(Encoding.UTF8.GetBytes("\n"));


                // Write field values row by row
                for (int i = 0; i < maxIndex; i++) {
                    // Neural shape
                    if (i < neuralShape.Length) {
                        fs.Write(Encoding.UTF8.GetBytes(neuralShape[i].ToString() + ","));
                    } else {
                        fs.Write(Encoding.UTF8.GetBytes(","));
                    }

                    // Activations
                    if (i < activations.Length) {
                        fs.Write(Encoding.UTF8.GetBytes(activations[i].ToString() + ","));
                    } else {
                        fs.Write(Encoding.UTF8.GetBytes(","));
                    }

                    for (int j = 0; j < weightsCount; j++) {
                        // Weights_{j}
                        if (i < weights[j].Length) {
                            fs.Write(Encoding.UTF8.GetBytes(weights[j][i].ToString() + ","));
                        } else {
                            fs.Write(Encoding.UTF8.GetBytes(","));
                        }

                        // Bias_{j}
                        bool trailingComma = (j < weightsCount - 1);
                        if (i < biases[j].Length) {
                            if (trailingComma) {
                                fs.Write(Encoding.UTF8.GetBytes(biases[j][i].ToString() + ","));
                            } else {
                                fs.Write(Encoding.UTF8.GetBytes(biases[j][i].ToString()));
                            }
                        } else if (trailingComma) {
                            fs.Write(Encoding.UTF8.GetBytes(","));
                        }
                    }

                    fs.Write(Encoding.UTF8.GetBytes("\n"));
                }

                fs.Close();
            }
        }
    }
}