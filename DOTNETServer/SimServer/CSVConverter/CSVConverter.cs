//using System
using System.Text;
using SimServer;

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

            List<NNGroupData> nngs = new List<NNGroupData>();
            for (int i = 0; i < nnFoldersFD.Count; i++) {
                nngs.Add(new NNGroupData(nnFoldersFD[i].fullFilePath, nnFoldersFD[i].fullFilePath + "/" + NeuralTrainer.META_FILE + NeuralTrainer.NN_META_FILE_EXTENSION));
            }

            // Create output folders
            foreach (NNGroupData nng in nngs) {
                string outputFolder = saveCSVsTo + "/" + nng.nnFolder.fileName;
                Directory.CreateDirectory(outputFolder);
                WriteCSVsFromGroup(nng, outputFolder);
            }
        }

        static void WriteCSVsFromGroup(NNGroupData nng, string outputFolder) {
            foreach (NNData nn in nng.NNs) {
                WriteCSVFromNN(nn, outputFolder);
            }
        }

        static void WriteCSVFromNN(NNData nn, string outputFolder) {
            string saveNNPath = outputFolder + "/" + new Utils.FileData(nn.nnFile).fileName + ".csv";

            if (File.Exists(saveNNPath)) {
                File.Delete(saveNNPath);
            }
            using (FileStream fs = new FileStream(saveNNPath, FileMode.CreateNew, FileAccess.Write)) {
                //string text = "W, ";
                //fs.Write(Encoding.Unicode.GetBytes(text));
                fs.Write(Encoding.Unicode.GetBytes(nn.originalData[0].ToString()));
                for (int i = 0; i < nn.originalData.Length; i++) {
                    fs.Write(Encoding.UTF8.GetBytes("\n" + nn.originalData[i]));
                }
                fs.Close();
            }
        }
    }
}