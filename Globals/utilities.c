#include <utilities.h>

double random_range(double min, double max) {
    double range = (max - min); 
    double div = RAND_MAX / range;
    return min + (rand() / div);
}

double custom_lerp(double a, double b, double t) {
    return a + ((b - a) * t);
}

double clamp(double val, double min, double max) {
    if (val < min) {
        return min;
    }
    if (val > max) {
        return max;
    }
    return val;
}