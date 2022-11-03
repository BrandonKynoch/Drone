The goal of this project is to develop a deep neural net that can be deployed on an embedded system to control the motor outputs of a drone so that it can fly and navigate complex environments. The drone design will have minimal/low cost sensors. Specifically, it will have and array of 10 time of flight sensors as well as sensors to detect velocity, rotation and torque.

This repo includes:
 - The C source code that will be used on the drone itself.
 - My own neural network framework that include visualization tools for training.
 - An application that allows the drone source code to be simulated in Unity and faciliates the training of the neural network.


##############################################

Required frameworks for compilation include:
BLAS & CBlAS wrapper // Matrix multiplication
Json-C // Json functionality in C
