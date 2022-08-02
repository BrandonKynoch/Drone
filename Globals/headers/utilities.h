#ifndef HEADER_UTIL_H
#define HEADER_UTIL_H

#include <stdlib.h>
#include <stdio.h>

double random_range(double min, double max);
double custom_lerp(double a, double b, double t);
double clamp(double val, double min, double max);

#endif