using System;
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

        private Thread neuralTrainingThread;    // Thread dedicated to training the drones

        private int currentEpoch = 0;

        public NeuralTrainer() {
            if (staticInstance != null) {
                return;
            }
            staticInstance = this;

            neuralTrainingThread = new Thread(new ThreadStart(TrainingLoop));
            neuralTrainingThread.Priority = ThreadPriority.AboveNormal;
            neuralTrainingThread.Start();
        }

        public void TrainingLoop() {
            while (true) {
                Thread.Sleep(TimeSpan.FromSeconds(EPOCH_RUN_TIME));

                currentEpoch++;
                Console.WriteLine("Starting epoch: " + currentEpoch);
            }
        }
    }
}

