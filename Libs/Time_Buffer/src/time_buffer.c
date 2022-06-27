#include <time_buffer.h>

int main() {
    double test[] = {0.1, 0.2, 0.3};

    int size = 3;
    int time_steps = 4;

    struct time_buffer* timebuf = init_timebuffer(size, time_steps);

    double* corrected_time_buffer = calloc(size * time_steps, sizeof(double));

    for (int i = 0; i < 8; i++) {
        timebuffer_set(timebuf, test, 3, 0);
        timebuffer_increment(timebuf);

        timebuffer_copy_corrected(timebuf, corrected_time_buffer);

        printf("Full timebuffer after iteration: %d\n", i);
        for (int j = 0; j < timebuf->single_timestep_size * timebuf->timesteps; j++) {
            printf("%f\t\t%f\n", timebuf->full_buffer[j], corrected_time_buffer[j]);
        }

        printf("\n\n\n\n\n\n\n\n");
        
        test[0] += 1.0;
        test[1] += 1.0;
        test[2] += 1.0;
    }
}

struct time_buffer* init_timebuffer(int size, int time_steps) {
    struct time_buffer* tb = calloc(1, sizeof(struct time_buffer));

    tb->single_timestep_size = size;
    tb->timesteps = time_steps;

    tb->full_buffer = malloc(size * time_steps);
    tb->buffer = tb->full_buffer;
    tb->buffer_i = 0;

    return tb;
}

void timebuffer_set(struct time_buffer* tb, double* target, int count, int buffer_offset){
    tb->buffer = tb->full_buffer + (tb->buffer_i * tb->single_timestep_size); // Ensure current buffer pointer is correct
    for (int i = 0; i < count; i++) {
        tb->buffer[i + buffer_offset] = target[i];
    }
}

void timebuffer_increment(struct time_buffer* tb) {
    tb->buffer_i++;
    if (tb->buffer_i >= tb->timesteps) {
        tb->buffer_i = 0;
    }
    tb->buffer = tb->full_buffer + (tb->buffer_i * tb->single_timestep_size);
}

void timebuffer_copy_corrected(struct time_buffer* tb, double* target) {
    int targetI = 0;
    for (int i = tb->buffer_i * tb->single_timestep_size; i < tb->single_timestep_size * tb->timesteps; i++) {
        target[targetI] = tb->full_buffer[i];
        targetI++;
    }
    for (int i = 0; i < tb->buffer_i * tb->single_timestep_size; i++) {
        target[targetI] = tb->full_buffer[i];
        targetI++;
    }
}