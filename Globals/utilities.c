#include <utilities.h>

double random_range(double min, double max)  {
    double range = (max - min); 
    double div = RAND_MAX / range;
    return min + (rand() / div);
}