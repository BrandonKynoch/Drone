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
    }
}

