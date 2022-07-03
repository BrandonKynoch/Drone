#include <stdlib.h>

struct time_buffer {
    int single_timestep_size;
    int timesteps;

    int buffer_i; // The current buffer index
    double* full_buffer;
    double* buffer; // Points to the current array
};

struct time_buffer* init_timebuffer(int size, int time_steps);

// Copies 'count' elements from 'target' into the timebuffer at the
// correct timestep
// REMEMBER TO CALL TIMEBUFFER INCREMENT WHEN DONE
void timebuffer_set(struct time_buffer* tb, double* target, int count, int buffer_offset);

// Increment the buffer index and set the buffer pointer to the correct time array
void timebuffer_increment(struct time_buffer* tb);

// Copies the data into destination array while shifting offset
void timebuffer_copy_corrected(struct time_buffer* tb, double* target);

double timebuffer_total_size(struct time_buffer* tb);