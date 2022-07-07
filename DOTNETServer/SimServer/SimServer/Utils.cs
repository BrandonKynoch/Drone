using System;
using System.Security.Cryptography;

namespace SimServer {
    public class Utils {
        private static Random random = new Random();

        public static double RandomRange(double lowerBound, double upperBound) {
            double sample = random.NextDouble();
            return (upperBound * sample) + (lowerBound * (1d - sample));
        }

        public static double Random01Double() {
            return RandomRange(0, 1);
        }

        // Returns full file path, file name cast as int, file extension
        public static List<FileData> ExtractFileNameInts(string[] filepaths) {
            List<FileData> files = new List<FileData>();
            for (int i = 0; i < filepaths.Length; i++) {
                files.Add(new FileData(filepaths[i]));
            }

            return files;
        }

        public struct FileData: IComparable<FileData> {
            public string fullFilePath;
            public string enclosingDir;
            public string fileName;
            public int fileNameInt; // The integer component of the file name
            public string fileExtension;

            public FileData(string _filePath) {
                this.fullFilePath = _filePath;

                string[] filePathSplit = _filePath.Split(new char[] { '/' });
                this.enclosingDir = "";
                for (int i = 0; i < filePathSplit.Length - 1; i++) {
                    this.enclosingDir = enclosingDir + "/" + filePathSplit[i];
                }

                this.fileName = filePathSplit[filePathSplit.Length - 1];
                this.fileExtension = "";
                string[] nameSplit = fileName.Split(new char[] { '.' });
                if (nameSplit.Length > 1) {
                    this.fileName = nameSplit[0];
                    this.fileExtension = nameSplit[nameSplit.Length - 1];
                }

                try {
                    this.fileNameInt = int.Parse(string.Concat(fileName.Where(char.IsNumber)));
                } catch {
                    this.fileNameInt = -1;
                }
            }

            public int CompareTo(FileData other) {
                return fileNameInt.CompareTo(other.fileNameInt);
            }
        }
    }
}

