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
                string[] pathSplit = filepaths[i].Split(new char[] { '/' });
                string[] fileSplit = pathSplit[pathSplit.Length - 1].Split(new char[] { '.' });
                if (fileSplit.Length > 1) {
                    // Has extension -> Is file
                    files.Add(new FileData(filepaths[i], fileSplit[0], fileSplit[1]));
                } else {
                    // No extension -> Is directory
                    files.Add(new FileData(filepaths[i], fileSplit[0], ""));
                }
            }

            return files;
        }

        public struct FileData: IComparable<FileData> {
            public string filePath;
            public int fileNameInt;
            public string fileExention;

            public FileData(string _filePath, string _fileNameInt, string _fileExtension) {
                this.filePath = _filePath;
                try {
                    this.fileNameInt = int.Parse(string.Concat(_fileNameInt.Where(char.IsNumber)));
                } catch {
                    this.fileNameInt = -1;
                }
                this.fileExention = _fileExtension;
            }

            public int CompareTo(FileData other) {
                return fileNameInt.CompareTo(other.fileNameInt);
            }
        }
    }
}

